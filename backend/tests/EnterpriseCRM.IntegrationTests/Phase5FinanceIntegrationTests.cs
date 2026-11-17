using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using EnterpriseCRM.Application.Common.Models;
using EnterpriseCRM.Application.Features.Accounts;
using EnterpriseCRM.Application.Features.Auth;
using EnterpriseCRM.Application.Features.Contracts;
using EnterpriseCRM.Application.Features.Finance;
using EnterpriseCRM.Domain.Enums;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;
using Xunit.Abstractions;

namespace EnterpriseCRM.IntegrationTests;

[Collection("IntegrationTests")]
public class Phase5FinanceIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;
    private readonly ITestOutputHelper _output;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public Phase5FinanceIntegrationTests(WebApplicationFactory<Program> factory, ITestOutputHelper output)
    {
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

    private async Task<ContractDto> CreateTestContractAsync(string token)
    {
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // 1. Create Account
        var accResp = await _client.PostAsJsonAsync("/api/v1/accounts", new CreateAccountRequest(
            $"Finance Client {Guid.NewGuid():N}"[..22],
            "0109988776",
            "Technology",
            "https://finance-client.vn",
            $"+8490{Random.Shared.Next(1000000, 9999999)}",
            $"finance.{Guid.NewGuid():N}"[..18] + "@client.vn",
            "123 Nguyen Hue, D1, HCMC",
            "VIP",
            "Active",
            "Test Account for Finance",
            null
        ));
        Assert.Equal(HttpStatusCode.Created, accResp.StatusCode);
        var accResult = await accResp.Content.ReadFromJsonAsync<ApiResponse<AccountDto>>(JsonOptions);
        var accountId = accResult!.Data!.Id;

        // 2. Create Contract
        var contractResp = await _client.PostAsJsonAsync("/api/v1/contracts", new CreateContractRequest(
            Title: $"Cloud Architecture Master SOW - {accountId}",
            AccountId: accountId,
            ContactId: null,
            QuotationId: null,
            TemplateId: null,
            StartDate: DateTime.UtcNow,
            EndDate: DateTime.UtcNow.AddMonths(12),
            TotalValue: 100000000m,
            ScopeOfWork: "Enterprise Architecture Deployment",
            PaymentTerms: "30% upfront, 40% milestone 1, 30% final",
            SpecialTerms: "Standard warranty"
        ));
        var contractResult = await contractResp.Content.ReadFromJsonAsync<ApiResponse<ContractDto>>(JsonOptions);
        return contractResult!.Data!;
    }

    [Fact]
    public async Task PaymentSchedules_GenerateMultiInstallments_And_ValidatePercentage()
    {
        var token = await GetAdminTokenAsync();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var contract = await CreateTestContractAsync(token);

        // 1. Invalid Percentage (> 100%)
        var invalidReq = new GenerateInstallmentSchedulesRequest(new List<InstallmentPlanItemInput>
        {
            new("First 50%", 50m, DateOnly.FromDateTime(DateTime.UtcNow.AddDays(10))),
            new("Second 60%", 60m, DateOnly.FromDateTime(DateTime.UtcNow.AddDays(30)))
        });

        var invalidResp = await _client.PostAsJsonAsync($"/api/v1/payment-schedules/contract/{contract.Id}/generate", invalidReq);
        Assert.Equal(HttpStatusCode.BadRequest, invalidResp.StatusCode);

        // 2. Valid 30% / 40% / 30% = 100%
        var validReq = new GenerateInstallmentSchedulesRequest(new List<InstallmentPlanItemInput>
        {
            new("Kickoff Advance", 30m, DateOnly.FromDateTime(DateTime.UtcNow.AddDays(7)), "30% upfront"),
            new("Beta Delivery", 40m, DateOnly.FromDateTime(DateTime.UtcNow.AddDays(30)), "40% beta signoff"),
            new("Final Handover", 30m, DateOnly.FromDateTime(DateTime.UtcNow.AddDays(60)), "30% final signoff")
        });

        var validResp = await _client.PostAsJsonAsync($"/api/v1/payment-schedules/contract/{contract.Id}/generate", validReq);
        Assert.Equal(HttpStatusCode.OK, validResp.StatusCode);
        var genResult = await validResp.Content.ReadFromJsonAsync<ApiResponse<IReadOnlyList<PaymentScheduleDto>>>(JsonOptions);
        Assert.NotNull(genResult?.Data);
        Assert.Equal(3, genResult.Data.Count);

        // Check amount sum equals contract value
        var totalScheduled = genResult.Data.Sum(s => s.Amount);
        Assert.Equal(contract.TotalValue, totalScheduled);

        // 3. Fetch contract schedules
        var fetchResp = await _client.GetAsync($"/api/v1/payment-schedules/contract/{contract.Id}");
        Assert.Equal(HttpStatusCode.OK, fetchResp.StatusCode);
        var fetchResult = await fetchResp.Content.ReadFromJsonAsync<ApiResponse<IReadOnlyList<PaymentScheduleDto>>>(JsonOptions);
        Assert.NotNull(fetchResult?.Data);
        Assert.Equal(3, fetchResult.Data.Count);
    }

    [Fact]
    public async Task Payment_Create_Confirm_Reverse_Lifecycle()
    {
        var token = await GetAdminTokenAsync();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var contract = await CreateTestContractAsync(token);

        // 1. Generate schedules
        var genReq = new GenerateInstallmentSchedulesRequest(new List<InstallmentPlanItemInput>
        {
            new("Kickoff Advance", 50m, DateOnly.FromDateTime(DateTime.UtcNow.AddDays(5))),
            new("Final Completion", 50m, DateOnly.FromDateTime(DateTime.UtcNow.AddDays(30)))
        });
        var genResp = await _client.PostAsJsonAsync($"/api/v1/payment-schedules/contract/{contract.Id}/generate", genReq);
        var schedules = (await genResp.Content.ReadFromJsonAsync<ApiResponse<IReadOnlyList<PaymentScheduleDto>>>(JsonOptions))!.Data!;

        var firstSched = schedules[0];

        // 2. Create Payment (Pending)
        var createPayReq = new CreatePaymentRequest(
            ContractId: contract.Id,
            AccountId: contract.AccountId,
            PaymentScheduleId: firstSched.Id,
            Amount: firstSched.Amount,
            PaymentDate: DateTime.UtcNow,
            Method: PaymentMethod.BankTransfer,
            TransactionReference: $"TXN-{Guid.NewGuid():N}"[..16].ToUpper(),
            BankName: "Vietcombank",
            BankAccountNumber: "0071000123456",
            SenderName: "Test Client",
            Notes: "Payment for Milestone 1",
            AttachmentUrl: null
        );

        var createPayResp = await _client.PostAsJsonAsync("/api/v1/payments", createPayReq);
        Assert.Equal(HttpStatusCode.OK, createPayResp.StatusCode);
        var payResult = await createPayResp.Content.ReadFromJsonAsync<ApiResponse<PaymentDto>>(JsonOptions);
        Assert.NotNull(payResult?.Data);
        Assert.Equal("Pending", payResult.Data.Status);
        var paymentId = payResult.Data.Id;

        // 3. Confirm Payment
        var confirmResp = await _client.PostAsJsonAsync($"/api/v1/payments/{paymentId}/confirm", new ConfirmPaymentRequest("Bank wire verified against statement"));
        Assert.Equal(HttpStatusCode.OK, confirmResp.StatusCode);
        var confirmedPayment = (await confirmResp.Content.ReadFromJsonAsync<ApiResponse<PaymentDto>>(JsonOptions))!.Data!;
        Assert.Equal("Confirmed", confirmedPayment.Status);
        Assert.NotNull(confirmedPayment.ConfirmedAt);

        // 4. Verify Financial Summary
        var summaryResp = await _client.GetAsync($"/api/v1/payments/contract/{contract.Id}/summary");
        Assert.Equal(HttpStatusCode.OK, summaryResp.StatusCode);
        var summaryResult = await summaryResp.Content.ReadFromJsonAsync<ApiResponse<ContractFinancialSummaryDto>>(JsonOptions);
        Assert.NotNull(summaryResult?.Data);
        Assert.Equal(firstSched.Amount, summaryResult.Data.PaidAmount);
        Assert.Equal(contract.TotalValue - firstSched.Amount, summaryResult.Data.OutstandingAmount);

        // 5. Reverse Payment (with mandatory reason)
        var reverseResp = await _client.PostAsJsonAsync($"/api/v1/payments/{paymentId}/reverse", new ReversePaymentRequest("Customer issued chargeback request due to erroneous duplicate transfer"));
        Assert.Equal(HttpStatusCode.OK, reverseResp.StatusCode);
        var reversedPayment = (await reverseResp.Content.ReadFromJsonAsync<ApiResponse<PaymentDto>>(JsonOptions))!.Data!;
        Assert.Equal("Reversed", reversedPayment.Status);
        Assert.NotNull(reversedPayment.CancelledAt);
        Assert.Equal("Customer issued chargeback request due to erroneous duplicate transfer", reversedPayment.ReversalReason);

        // 6. Verify Financial Summary after reversal: PaidAmount returns to 0
        var summaryAfterRev = await _client.GetAsync($"/api/v1/payments/contract/{contract.Id}/summary");
        var summaryAfterRevResult = await summaryAfterRev.Content.ReadFromJsonAsync<ApiResponse<ContractFinancialSummaryDto>>(JsonOptions);
        Assert.Equal(0m, summaryAfterRevResult!.Data!.PaidAmount);
        Assert.Equal(contract.TotalValue, summaryAfterRevResult.Data.OutstandingAmount);
    }

    [Fact]
    public async Task RevenueAnalytics_Endpoint_ReturnsValidMetrics()
    {
        var token = await GetAdminTokenAsync();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await _client.GetAsync("/api/v1/finance/revenue");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var result = await response.Content.ReadFromJsonAsync<ApiResponse<RevenueAnalyticsResponse>>(JsonOptions);
        Assert.NotNull(result?.Data);
        Assert.NotNull(result.Data.Summary);
        Assert.True(result.Data.Summary.TotalContractValue >= 0);
        Assert.True(result.Data.Summary.TotalCollectedCash >= 0);
        Assert.NotNull(result.Data.TrendByPeriod);
        Assert.NotNull(result.Data.ByCustomer);
        Assert.NotNull(result.Data.ByContract);
    }

    [Fact]
    public async Task ReceivablesAndDebtAging_Summary_And_CollectionLogs()
    {
        var token = await GetAdminTokenAsync();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var contract = await CreateTestContractAsync(token);

        // 1. Check Aging Summary
        var sumResp = await _client.GetAsync("/api/v1/finance/receivables/summary");
        Assert.Equal(HttpStatusCode.OK, sumResp.StatusCode);
        var sumResult = await sumResp.Content.ReadFromJsonAsync<ApiResponse<ReceivableAgingSummaryDto>>(JsonOptions);
        Assert.NotNull(sumResult?.Data);

        // 2. Fetch Receivables List
        var listResp = await _client.GetAsync("/api/v1/finance/receivables");
        Assert.Equal(HttpStatusCode.OK, listResp.StatusCode);
        var listResult = await listResp.Content.ReadFromJsonAsync<ApiResponse<IReadOnlyList<AccountsReceivableItemDto>>>(JsonOptions);
        Assert.NotNull(listResult?.Data);

        // 3. Add Collection Log
        var logReq = new CreateReceivableCollectionLogRequest(
            AccountId: contract.AccountId,
            ContractId: contract.Id,
            PaymentScheduleId: null,
            ActivityType: "Email Reminder",
            Notes: "Sent first formal overdue reminder to accounting department.",
            NextFollowUpDate: DateOnly.FromDateTime(DateTime.UtcNow.AddDays(7)),
            Status: "Open"
        );

        var logResp = await _client.PostAsJsonAsync("/api/v1/finance/collection-logs", logReq);
        Assert.Equal(HttpStatusCode.OK, logResp.StatusCode);
        var logResult = await logResp.Content.ReadFromJsonAsync<ApiResponse<ReceivableCollectionLogDto>>(JsonOptions);
        Assert.NotNull(logResult?.Data);
        Assert.Equal("Email Reminder", logResult.Data.ActivityType);

        // 4. Fetch logs for account
        var fetchLogsResp = await _client.GetAsync($"/api/v1/finance/collection-logs?accountId={contract.AccountId}");
        Assert.Equal(HttpStatusCode.OK, fetchLogsResp.StatusCode);
        var fetchLogsResult = await fetchLogsResp.Content.ReadFromJsonAsync<ApiResponse<IReadOnlyList<ReceivableCollectionLogDto>>>(JsonOptions);
        Assert.NotNull(fetchLogsResult?.Data);
        Assert.Contains(fetchLogsResult.Data, l => l.Notes.Contains("Sent first formal overdue reminder"));
    }

    [Fact]
    public async Task ElectronicInvoice_Issue_Sync_And_Cancel_Lifecycle()
    {
        var token = await GetAdminTokenAsync();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var contract = await CreateTestContractAsync(token);

        // 1. Issue Invoice
        var issueReq = new IssueInvoiceRequest(
            ContractId: contract.Id,
            AccountId: contract.AccountId,
            InvoiceRequestId: null,
            PaymentId: null,
            CustomerName: "Test Enterprise Corp",
            CustomerTaxCode: "0109988776",
            CustomerAddress: "123 Le Loi, Hanoi",
            CustomerEmail: "finance@testcorp.vn",
            IssueDate: DateOnly.FromDateTime(DateTime.UtcNow),
            ElectronicInvoiceProvider: "Mock",
            Items: new List<InvoiceItemInput>
            {
                new(null, "Consulting & Cloud Migration Services", "Gói", 1, 50000000m, 8m)
            }
        );

        var issueResp = await _client.PostAsJsonAsync("/api/v1/invoices", issueReq);
        Assert.Equal(HttpStatusCode.OK, issueResp.StatusCode);
        var invoiceResult = await issueResp.Content.ReadFromJsonAsync<ApiResponse<InvoiceDto>>(JsonOptions);
        Assert.NotNull(invoiceResult?.Data);
        Assert.NotNull(invoiceResult.Data.LookupCode);
        Assert.NotNull(invoiceResult.Data.LookupUrl);
        Assert.Equal(54000000m, invoiceResult.Data.TotalAmount);
        var invoiceId = invoiceResult.Data.Id;

        // 2. Fetch invoice PDF
        var pdfResp = await _client.GetAsync($"/api/v1/invoices/{invoiceId}/pdf");
        Assert.Equal(HttpStatusCode.OK, pdfResp.StatusCode);
        var pdfBytes = await pdfResp.Content.ReadAsByteArrayAsync();
        Assert.True(pdfBytes.Length > 0);

        // 3. Cancel Invoice
        var cancelResp = await _client.PostAsJsonAsync($"/api/v1/invoices/{invoiceId}/cancel", new CancelInvoiceRequest("Wrong tax code provided by client; reissue needed"));
        Assert.Equal(HttpStatusCode.OK, cancelResp.StatusCode);
        var cancelledResult = await cancelResp.Content.ReadFromJsonAsync<ApiResponse<InvoiceDto>>(JsonOptions);
        Assert.Equal(InvoiceStatus.Cancelled, cancelledResult!.Data!.Status);
        Assert.Equal("Wrong tax code provided by client; reissue needed", cancelledResult.Data.CancellationReason);
    }

    [Fact]
    public async Task FinancialReconciliation_Run_And_Resolve()
    {
        var token = await GetAdminTokenAsync();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // 1. Run Reconciliation
        var runReq = new RunReconciliationRequest(
            PeriodStart: DateOnly.FromDateTime(DateTime.UtcNow.AddMonths(-1)),
            PeriodEnd: DateOnly.FromDateTime(DateTime.UtcNow),
            Notes: "Automated monthly reconciliation run"
        );

        var runResp = await _client.PostAsJsonAsync("/api/v1/finance/reconciliation/run", runReq);
        Assert.Equal(HttpStatusCode.OK, runResp.StatusCode);
        var recResult = await runResp.Content.ReadFromJsonAsync<ApiResponse<FinancialReconciliationRecordDto>>(JsonOptions);
        Assert.NotNull(recResult?.Data);
        Assert.True(recResult.Data.TotalContractValue >= 0);

        // If discrepancies detected, test resolving one
        if (recResult.Data.Items != null && recResult.Data.Items.Count > 0)
        {
            var firstItem = recResult.Data.Items[0];
            var resolveResp = await _client.PostAsJsonAsync($"/api/v1/finance/reconciliation/items/{firstItem.Id}/resolve",
                new ResolveDiscrepancyRequest("ConfirmAdjustment", "Audited and verified against general ledger adjustment entry."));
            Assert.Equal(HttpStatusCode.OK, resolveResp.StatusCode);
            var resolvedItem = (await resolveResp.Content.ReadFromJsonAsync<ApiResponse<ReconciliationItemDto>>(JsonOptions))!.Data!;
            Assert.Equal("Confirmed", resolvedItem.ResolutionStatus);
        }
    }
}
