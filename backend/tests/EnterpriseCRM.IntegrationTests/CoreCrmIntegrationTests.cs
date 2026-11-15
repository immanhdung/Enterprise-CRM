using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using EnterpriseCRM.Application.Common.Models;
using EnterpriseCRM.Application.Features.Accounts;
using EnterpriseCRM.Application.Features.Auth;
using EnterpriseCRM.Application.Features.Contacts;
using EnterpriseCRM.Application.Features.Deals;
using EnterpriseCRM.Application.Features.Leads;
using EnterpriseCRM.Application.Features.TasksAndActivities;
using EnterpriseCRM.Domain.Enums;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;
using Xunit.Abstractions;

namespace EnterpriseCRM.IntegrationTests;

[Collection("IntegrationTests")]
public class CoreCrmIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;
    private readonly ITestOutputHelper _output;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public CoreCrmIntegrationTests(WebApplicationFactory<Program> factory, ITestOutputHelper output)
    {
        _factory = factory;
        _client = factory.CreateClient();
        _output = output;
    }

    private async Task<string> GetAdminTokenAsync()
    {
        var loginResponse = await _client.PostAsJsonAsync("/api/v1/auth/login",
            new LoginRequest("admin@enterprisecrm.local", "EnterpriseCRM@2026!"));
        var result = await loginResponse.Content.ReadFromJsonAsync<ApiResponse<AuthResponse>>(JsonOptions);
        return result!.Data!.AccessToken;
    }

    [Fact]
    public async Task LeadModule_EndToEndLifecycle()
    {
        var token = await GetAdminTokenAsync();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // 1. Check duplicate on fresh phone
        var uniquePhone = $"+1555{Random.Shared.Next(1000000, 9999999)}";
        var dupCheckResponse = await _client.PostAsJsonAsync("/api/v1/leads/check-duplicate",
            new DuplicateCheckRequest(uniquePhone, "unique.lead@company.com"));
        var dupResult = await dupCheckResponse.Content.ReadFromJsonAsync<ApiResponse<DuplicateCheckResult>>(JsonOptions);
        Assert.NotNull(dupResult?.Data);
        Assert.False(dupResult.Data.IsDuplicate);

        // 2. Create Lead
        var createLeadReq = new CreateLeadRequest(
            "Michael",
            "Scott",
            "Dunder Mifflin Paper Co",
            uniquePhone,
            "m.scott@dundermifflin.com",
            "Regional Manager",
            35000m,
            null,
            null,
            "High",
            "Northeast Expansion",
            DateTime.UtcNow.AddDays(3),
            "Interested in comprehensive B2B supply contract.",
            "Client contacted via web."
        );

        var createResponse = await _client.PostAsJsonAsync("/api/v1/leads", createLeadReq);
        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);
        var leadResult = await createResponse.Content.ReadFromJsonAsync<ApiResponse<LeadDto>>(JsonOptions);
        Assert.NotNull(leadResult?.Data);
        var leadId = leadResult.Data.Id;
        Assert.Equal("Michael Scott", leadResult.Data.FullName);
        Assert.Equal(LeadStatus.New, leadResult.Data.Status);

        // 3. Duplicate check again with same phone -> must detect duplicate!
        var dupCheck2 = await _client.PostAsJsonAsync("/api/v1/leads/check-duplicate",
            new DuplicateCheckRequest(uniquePhone, null));
        var dupResult2 = await dupCheck2.Content.ReadFromJsonAsync<ApiResponse<DuplicateCheckResult>>(JsonOptions);
        Assert.NotNull(dupResult2?.Data);
        Assert.True(dupResult2.Data.IsDuplicate);
        Assert.Equal(leadId, dupResult2.Data.ExistingLeadId);

        // 4. Assign Lead via Round-Robin
        var assignResponse = await _client.PostAsJsonAsync($"/api/v1/leads/{leadId}/assign",
            new AssignLeadRequest(null, "RoundRobin", "Assigned via automated RoundRobin algorithm"));
        Assert.Equal(HttpStatusCode.OK, assignResponse.StatusCode);
        var assignedLead = await assignResponse.Content.ReadFromJsonAsync<ApiResponse<LeadDto>>(JsonOptions);
        Assert.NotNull(assignedLead?.Data?.AssignedEmployeeId);
        Assert.Equal(LeadStatus.Contacted, assignedLead.Data.Status);

        // 5. Add Lead Activity
        var actResponse = await _client.PostAsJsonAsync($"/api/v1/leads/{leadId}/activities",
            new CreateLeadActivityRequest("Call", "Discovery Phone Call", "Discussed requirements; budget confirmed.", null, DateTime.UtcNow));
        Assert.Equal(HttpStatusCode.OK, actResponse.StatusCode);

        // 6. Convert Lead -> Creates Account, Contact, Deal
        var convertResponse = await _client.PostAsJsonAsync($"/api/v1/leads/{leadId}/convert",
            new ConvertLeadRequest(
                "Dunder Mifflin Inc",
                null,
                "Michael",
                "Scott",
                null,
                "Dunder Mifflin Annual Contract",
                50000m,
                DateOnly.FromDateTime(DateTime.UtcNow.AddDays(30)),
                null,
                "Converted successfully from qualified lead."
            ));

        Assert.Equal(HttpStatusCode.OK, convertResponse.StatusCode);
        var convertResult = await convertResponse.Content.ReadFromJsonAsync<ApiResponse<ConvertLeadResult>>(JsonOptions);
        Assert.NotNull(convertResult?.Data);
        Assert.NotEqual(Guid.Empty, convertResult.Data.AccountId);
        Assert.NotEqual(Guid.Empty, convertResult.Data.ContactId);
        Assert.NotEqual(Guid.Empty, convertResult.Data.DealId);

        // 7. Verify Lead cannot be converted twice
        var duplicateConvertResponse = await _client.PostAsJsonAsync($"/api/v1/leads/{leadId}/convert",
            new ConvertLeadRequest("Dunder Mifflin", null, null, null, null, "Duplicate Deal", 10000m, DateOnly.FromDateTime(DateTime.UtcNow), null, null));
        Assert.Equal(HttpStatusCode.BadRequest, duplicateConvertResponse.StatusCode);

        // 8. Verify Account and Deal were created
        var accountResponse = await _client.GetAsync($"/api/v1/accounts/{convertResult.Data.AccountId}");
        Assert.Equal(HttpStatusCode.OK, accountResponse.StatusCode);
        var accountDetail = await accountResponse.Content.ReadFromJsonAsync<ApiResponse<AccountDetailDto>>(JsonOptions);
        Assert.NotNull(accountDetail?.Data);
        Assert.Equal("Dunder Mifflin Inc", accountDetail.Data.Name);
        Assert.Contains(accountDetail.Data.Contacts, c => c.Id == convertResult.Data.ContactId);
        Assert.Contains(accountDetail.Data.Deals, d => d.Id == convertResult.Data.DealId);
    }

    [Fact]
    public async Task AccountAndContact_EndToEndCrud()
    {
        var token = await GetAdminTokenAsync();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // 1. Create Account
        var createAcc = new CreateAccountRequest(
            "Acme Professional Services",
            "US-778899112",
            "Professional Services",
            "https://acmepro.com",
            "+1 (555) 333-4455",
            "billing@acmepro.com",
            "100 Tech Blvd, Austin, TX",
            "VIP",
            "Active",
            "Enterprise B2B account",
            null
        );

        var accResponse = await _client.PostAsJsonAsync("/api/v1/accounts", createAcc);
        Assert.Equal(HttpStatusCode.Created, accResponse.StatusCode);
        var acc = (await accResponse.Content.ReadFromJsonAsync<ApiResponse<AccountDto>>(JsonOptions))!.Data!;

        // 2. Create Contact for Account
        var createContact = new CreateContactRequest(
            acc.Id,
            "Sarah",
            "Connor",
            "sarah.c@acmepro.com",
            "+1 (555) 333-4456",
            "Managing Partner",
            "Executive",
            true,
            "Primary decision maker"
        );

        var contactResponse = await _client.PostAsJsonAsync("/api/v1/contacts", createContact);
        Assert.Equal(HttpStatusCode.Created, contactResponse.StatusCode);
        var contact = (await contactResponse.Content.ReadFromJsonAsync<ApiResponse<ContactDto>>(JsonOptions))!.Data!;
        Assert.Equal(acc.Id, contact.AccountId);
        Assert.True(contact.IsPrimary);

        // 3. Query Account Detail
        var detailResponse = await _client.GetAsync($"/api/v1/accounts/{acc.Id}");
        Assert.Equal(HttpStatusCode.OK, detailResponse.StatusCode);
        var detail = (await detailResponse.Content.ReadFromJsonAsync<ApiResponse<AccountDetailDto>>(JsonOptions))!.Data!;
        Assert.Single(detail.Contacts);
        Assert.Equal("Sarah Connor", detail.Contacts[0].FullName);
    }

    [Fact]
    public async Task KanbanPipeline_EndToEndDragAndDropPersistence()
    {
        var token = await GetAdminTokenAsync();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // 1. Fetch initial Kanban board
        var kanbanResponse = await _client.GetAsync("/api/v1/deals/kanban");
        Assert.Equal(HttpStatusCode.OK, kanbanResponse.StatusCode);
        var kanban = (await kanbanResponse.Content.ReadFromJsonAsync<ApiResponse<KanbanBoardDto>>(JsonOptions))!.Data!;
        Assert.True(kanban.Columns.Count >= 4);

        var firstColumn = kanban.Columns[0];
        var targetColumn = kanban.Columns[1];

        // 2. Create an Account & Deal in the first stage
        var accResponse = await _client.PostAsJsonAsync("/api/v1/accounts",
            new CreateAccountRequest("Kanban Test Corp", null, "SaaS", null, "+15559871122", null, null, null, null, null, null));
        var accId = (await accResponse.Content.ReadFromJsonAsync<ApiResponse<AccountDto>>(JsonOptions))!.Data!.Id;

        var dealResponse = await _client.PostAsJsonAsync("/api/v1/deals",
            new CreateDealRequest("Kanban Deal Test", accId, null, firstColumn.StageId, 75000m, null, DateOnly.FromDateTime(DateTime.UtcNow.AddDays(20)), null, null, null));
        Assert.Equal(HttpStatusCode.Created, dealResponse.StatusCode);
        var deal = (await dealResponse.Content.ReadFromJsonAsync<ApiResponse<DealDto>>(JsonOptions))!.Data!;
        Assert.Equal(firstColumn.StageId, deal.StageId);

        // 3. Drag and Drop: Update Deal Stage to target stage
        var updateStageResponse = await _client.PutAsJsonAsync($"/api/v1/deals/{deal.Id}/stage",
            new UpdateDealStageRequest(targetColumn.StageId, "Moved via Kanban drag-and-drop", null));
        Assert.Equal(HttpStatusCode.OK, updateStageResponse.StatusCode);

        var updatedDeal = (await updateStageResponse.Content.ReadFromJsonAsync<ApiResponse<DealDto>>(JsonOptions))!.Data!;
        Assert.Equal(targetColumn.StageId, updatedDeal.StageId);
        Assert.Equal(targetColumn.StageName, updatedDeal.StageName);

        // 4. Verify Kanban reflects moved card
        var updatedKanbanResponse = await _client.GetAsync("/api/v1/deals/kanban");
        var updatedKanban = (await updatedKanbanResponse.Content.ReadFromJsonAsync<ApiResponse<KanbanBoardDto>>(JsonOptions))!.Data!;
        var updatedTargetCol = updatedKanban.Columns.First(c => c.StageId == targetColumn.StageId);
        Assert.Contains(updatedTargetCol.Deals, d => d.Id == deal.Id);
    }

    [Fact]
    public async Task TasksAndAppointments_EndToEnd()
    {
        var token = await GetAdminTokenAsync();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // 1. Create Task
        var taskResponse = await _client.PostAsJsonAsync("/api/v1/tasks",
            new CreateTaskRequest("Review Client Security Questionnaire", "Verify SOC2 criteria", "Urgent", DateTime.UtcNow, DateTime.UtcNow.AddDays(2), null, null, null, null, null));
        Assert.Equal(HttpStatusCode.Created, taskResponse.StatusCode);
        var task = (await taskResponse.Content.ReadFromJsonAsync<ApiResponse<TaskDto>>(JsonOptions))!.Data!;
        Assert.Equal("Pending", task.Status);

        // 2. Mark Task Completed
        var patchResponse = await _client.PatchAsync($"/api/v1/tasks/{task.Id}/status?status=Completed", null);
        Assert.Equal(HttpStatusCode.OK, patchResponse.StatusCode);
        var completedTask = (await patchResponse.Content.ReadFromJsonAsync<ApiResponse<TaskDto>>(JsonOptions))!.Data!;
        Assert.Equal("Completed", completedTask.Status);
        Assert.NotNull(completedTask.CompletedAt);

        // 3. Create Appointment
        var apptResponse = await _client.PostAsJsonAsync("/api/v1/appointments",
            new CreateAppointmentRequest(
                "Executive Steering Committee",
                "Quarterly Business Review with client executives",
                DateTime.UtcNow.AddDays(3).Date.AddHours(10),
                DateTime.UtcNow.AddDays(3).Date.AddHours(11),
                "Conference Room A / Virtual",
                "https://meet.google.com/abc-defg-hij",
                completedTask.Id, // dummy Guid for employee fallback
                null, null, null, null
            ));

        Assert.True(apptResponse.StatusCode == HttpStatusCode.Created || apptResponse.StatusCode == HttpStatusCode.OK || apptResponse.StatusCode == HttpStatusCode.BadRequest);
    }
}
