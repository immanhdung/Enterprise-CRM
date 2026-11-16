using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using EnterpriseCRM.Application.Common.Models;
using EnterpriseCRM.Application.Features.Accounts;
using EnterpriseCRM.Application.Features.Auth;
using EnterpriseCRM.Application.Features.Contracts;
using EnterpriseCRM.Application.Features.ElectronicSignature;
using EnterpriseCRM.Application.Features.Quotations;
using EnterpriseCRM.Application.Features.Services;
using EnterpriseCRM.Domain.Enums;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;
using Xunit.Abstractions;

namespace EnterpriseCRM.IntegrationTests;

[Collection("IntegrationTests")]
public class Phase4QuotationAndContractIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;
    private readonly ITestOutputHelper _output;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public Phase4QuotationAndContractIntegrationTests(WebApplicationFactory<Program> factory, ITestOutputHelper output)
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

    [Fact]
    public async Task ServiceCatalog_FullLifecycle()
    {
        var token = await GetAdminTokenAsync();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // 1. Create Service Category
        var catCode = $"CAT-TEST-{Guid.NewGuid():N}"[..12].ToUpper();
        var catResponse = await _client.PostAsJsonAsync("/api/v1/service-categories", new CreateServiceCategoryRequest(
            catCode,
            "Cloud Security Engineering",
            "Security testing and audits",
            10
        ));
        Assert.Equal(HttpStatusCode.OK, catResponse.StatusCode);
        var catResult = await catResponse.Content.ReadFromJsonAsync<ApiResponse<ServiceCategoryDto>>(JsonOptions);
        Assert.NotNull(catResult?.Data);
        var categoryId = catResult.Data.Id;

        // 2. Create Service
        var svcCode = $"SVC-TEST-{Guid.NewGuid():N}"[..12].ToUpper();
        var svcResponse = await _client.PostAsJsonAsync("/api/v1/services", new CreateServiceRequest(
            svcCode,
            "Autonomous Red Team Assessment",
            "Simulated adversary attack assessment",
            "Assessment",
            60000000m,
            35000000m,
            categoryId
        ));
        Assert.Equal(HttpStatusCode.OK, svcResponse.StatusCode);
        var svcResult = await svcResponse.Content.ReadFromJsonAsync<ApiResponse<ServiceDto>>(JsonOptions);
        Assert.NotNull(svcResult?.Data);
        var serviceId = svcResult.Data.Id;
        Assert.Equal(60000000m, svcResult.Data.BasePrice);

        // 3. Update Service Base Price to trigger price history
        var updateResponse = await _client.PutAsJsonAsync($"/api/v1/services/{serviceId}", new UpdateServiceRequest(
            svcResult.Data.Name,
            svcResult.Data.Description,
            svcResult.Data.Unit,
            65000000m,
            38000000m,
            categoryId,
            true,
            "2026 Annual Rate Revision"
        ));
        Assert.Equal(HttpStatusCode.OK, updateResponse.StatusCode);

        // 4. Verify Price History
        var historyResponse = await _client.GetAsync($"/api/v1/services/{serviceId}/price-history");
        Assert.Equal(HttpStatusCode.OK, historyResponse.StatusCode);
        var historyResult = await historyResponse.Content.ReadFromJsonAsync<ApiResponse<IReadOnlyList<ServicePriceHistoryDto>>>(JsonOptions);
        Assert.NotNull(historyResult?.Data);
        Assert.True(historyResult.Data.Count >= 2, "Price history should have at least initial and update entries.");
        Assert.Equal(65000000m, historyResult.Data[0].NewPrice);

        // 5. Create Price List with Tiered Items
        var plCode = $"PL-TEST-{Guid.NewGuid():N}"[..12].ToUpper();
        var plResponse = await _client.PostAsJsonAsync("/api/v1/price-lists", new CreatePriceListRequest(
            plCode,
            "Strategic Enterprise Rate Card",
            "Tiered volume rates",
            "VND",
            DateTime.UtcNow.AddDays(-1),
            DateTime.UtcNow.AddDays(30),
            false,
            true,
            new List<PriceListItemRequest>
            {
                new(serviceId, 58000000m, 1, 10m),
                new(serviceId, 52000000m, 3, 15m)
            }
        ));
        Assert.Equal(HttpStatusCode.OK, plResponse.StatusCode);
        var plResult = await plResponse.Content.ReadFromJsonAsync<ApiResponse<PriceListDto>>(JsonOptions);
        Assert.NotNull(plResult?.Data);
        var priceListId = plResult.Data.Id;

        // 6. Test Effective Price
        var singlePriceResp = await _client.GetAsync($"/api/v1/services/{serviceId}/effective-price?priceListId={priceListId}&quantity=1");
        var singlePrice = await singlePriceResp.Content.ReadFromJsonAsync<ApiResponse<decimal?>>(JsonOptions);
        Assert.Equal(58000000m, singlePrice?.Data);

        var bulkPriceResp = await _client.GetAsync($"/api/v1/services/{serviceId}/effective-price?priceListId={priceListId}&quantity=5");
        var bulkPrice = await bulkPriceResp.Content.ReadFromJsonAsync<ApiResponse<decimal?>>(JsonOptions);
        Assert.Equal(52000000m, bulkPrice?.Data);
    }

    [Fact]
    public async Task Quotation_FullLifecycle_And_PdfGeneration()
    {
        var token = await GetAdminTokenAsync();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // 1. Create a dedicated test account
        var accResponse = await _client.PostAsJsonAsync("/api/v1/accounts", new CreateAccountRequest(
            $"Phase4 Quote Corp {Guid.NewGuid():N}"[..24],
            "0319887766",
            "Software & Cloud",
            "https://quotecorp.com",
            $"+1555{Random.Shared.Next(1000000, 9999999)}",
            "quote.procurement@quotecorp.com",
            "88 Technology Boulevard, Hanoi",
            "VIP",
            "Active",
            "Enterprise account for Phase 4 tests",
            null
        ));
        Assert.Equal(HttpStatusCode.Created, accResponse.StatusCode);
        var account = (await accResponse.Content.ReadFromJsonAsync<ApiResponse<AccountDto>>(JsonOptions))!.Data!;

        // 2. Create Services for quotation
        var svc1Resp = await _client.PostAsJsonAsync("/api/v1/services", new CreateServiceRequest(
            $"SVC-K8S-{Guid.NewGuid():N}"[..12].ToUpper(), "Kubernetes Production Setup", "Cluster setup", "Cluster", 40000000m, 20000000m, null
        ));
        var svc1 = (await svc1Resp.Content.ReadFromJsonAsync<ApiResponse<ServiceDto>>(JsonOptions))!.Data!;

        var svc2Resp = await _client.PostAsJsonAsync("/api/v1/services", new CreateServiceRequest(
            $"SVC-DEV-{Guid.NewGuid():N}"[..12].ToUpper(), "Custom Backend API Engineering", "Dev work", "Month", 50000000m, 25000000m, null
        ));
        var svc2 = (await svc2Resp.Content.ReadFromJsonAsync<ApiResponse<ServiceDto>>(JsonOptions))!.Data!;

        // 3. Create Quotation with 2 items
        var quoteRequest = new CreateQuotationRequest(
            null,
            account.Id,
            null,
            null,
            DateTime.UtcNow.AddDays(30),
            "Payment within 30 days of deliverable acceptance.",
            "Phase 4 Integration Test Quote",
            new List<QuotationItemInput>
            {
                new(svc1.Id, svc1.Code, svc1.Name, svc1.Unit, 1, 40000000m, 10m, 10m, "10% Partner discount"),
                new(svc2.Id, svc2.Code, svc2.Name, svc2.Unit, 2, 50000000m, 5m, 10m, "5% Multi-month discount")
            }
        );

        var quoteResp = await _client.PostAsJsonAsync("/api/v1/quotations", quoteRequest);
        Assert.Equal(HttpStatusCode.OK, quoteResp.StatusCode);
        var quoteResult = await quoteResp.Content.ReadFromJsonAsync<ApiResponse<QuotationDto>>(JsonOptions);
        Assert.NotNull(quoteResult?.Data);
        var quoteId = quoteResult.Data.Id;

        // Item 1: 40M * 1 = 40M, disc 10% = 4M, net = 36M, VAT 10% = 3.6M, lineTotal = 39.6M
        // Item 2: 50M * 2 = 100M, disc 5% = 5M, net = 95M, VAT 10% = 9.5M, lineTotal = 104.5M
        // Subtotal: 140M, Discount: 9M, Net: 131M, VAT: 13.1M, Total: 144.1M
        Assert.Equal(140000000m, quoteResult.Data.Subtotal);
        Assert.Equal(9000000m, quoteResult.Data.DiscountAmount);
        Assert.Equal(13100000m, quoteResult.Data.VatAmount);
        Assert.Equal(144100000m, quoteResult.Data.TotalAmount);
        Assert.Equal("Draft", quoteResult.Data.Status);
        Assert.Equal(1, quoteResult.Data.Version);
        Assert.Equal("REV-01", quoteResult.Data.RevisionNumber);

        // 3. Submit for approval
        var submitResp = await _client.PostAsJsonAsync($"/api/v1/quotations/{quoteId}/submit", new SubmitQuotationApprovalRequest("Submitted for BOD review"));
        Assert.Equal(HttpStatusCode.OK, submitResp.StatusCode);
        var submitResult = await submitResp.Content.ReadFromJsonAsync<ApiResponse<QuotationDto>>(JsonOptions);
        Assert.Equal("Submitted", submitResult?.Data?.Status);

        // 4. Approve quotation
        var approveResp = await _client.PostAsJsonAsync($"/api/v1/quotations/{quoteId}/approve", new ApproveQuotationRequest("Approved by General Director"));
        Assert.Equal(HttpStatusCode.OK, approveResp.StatusCode);
        var approveResult = await approveResp.Content.ReadFromJsonAsync<ApiResponse<QuotationDto>>(JsonOptions);
        Assert.Equal("Approved", approveResult?.Data?.Status);

        // 5. Download Quotation PDF
        var pdfResp = await _client.GetAsync($"/api/v1/quotations/{quoteId}/pdf");
        Assert.Equal(HttpStatusCode.OK, pdfResp.StatusCode);
        Assert.Equal("application/pdf", pdfResp.Content.Headers.ContentType?.MediaType);
        var pdfBytes = await pdfResp.Content.ReadAsByteArrayAsync();
        Assert.True(pdfBytes.Length > 1000, "PDF binary must be valid and non-empty.");

        // 6. Create Revision
        var reviseResp = await _client.PostAsJsonAsync($"/api/v1/quotations/{quoteId}/revise", new CreateRevisionRequest("Customer requested revised scope"));
        var reviseBody = await reviseResp.Content.ReadAsStringAsync();
        _output.WriteLine($"Revise Response ({reviseResp.StatusCode}): {reviseBody}");
        Assert.Equal(HttpStatusCode.OK, reviseResp.StatusCode);
        var reviseResult = await reviseResp.Content.ReadFromJsonAsync<ApiResponse<QuotationDto>>(JsonOptions);
        Assert.NotNull(reviseResult?.Data);
        Assert.Equal(2, reviseResult.Data.Version);
        Assert.Equal("REV-02", reviseResult.Data.RevisionNumber);
        Assert.Equal("Draft", reviseResult.Data.Status);

        // Verify versions list
        var versionsResp = await _client.GetAsync($"/api/v1/quotations/{quoteId}/versions");
        Assert.Equal(HttpStatusCode.OK, versionsResp.StatusCode);
        var versionsResult = await versionsResp.Content.ReadFromJsonAsync<ApiResponse<IReadOnlyList<QuotationDto>>>(JsonOptions);
        Assert.NotNull(versionsResult?.Data);
        Assert.True(versionsResult.Data.Count >= 2);
    }

    [Fact]
    public async Task Contract_Generation_Appendices_Renewal_And_ESign()
    {
        var token = await GetAdminTokenAsync();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // 1. Create a dedicated test account
        var accResponse = await _client.PostAsJsonAsync("/api/v1/accounts", new CreateAccountRequest(
            $"Phase4 Contract Corp {Guid.NewGuid():N}"[..24],
            "0319888899",
            "Banking & Finance",
            "https://contractcorp.com",
            $"+1555{Random.Shared.Next(1000000, 9999999)}",
            "legal.contract@contractcorp.com",
            "100 Financial Street, HCMC",
            "VIP",
            "Active",
            "Enterprise account for Contract tests",
            null
        ));
        Assert.Equal(HttpStatusCode.Created, accResponse.StatusCode);
        var account = (await accResponse.Content.ReadFromJsonAsync<ApiResponse<AccountDto>>(JsonOptions))!.Data!;

        // 2. Create Service and approve a quotation
        var svcSecResp = await _client.PostAsJsonAsync("/api/v1/services", new CreateServiceRequest(
            $"SVC-SEC-{Guid.NewGuid():N}"[..12].ToUpper(), "Continuous Threat Monitoring", "SOC work", "Month", 25000000m, 15000000m, null
        ));
        var svcSec = (await svcSecResp.Content.ReadFromJsonAsync<ApiResponse<ServiceDto>>(JsonOptions))!.Data!;

        var quoteResp = await _client.PostAsJsonAsync("/api/v1/quotations", new CreateQuotationRequest(
            null,
            account.Id,
            null,
            null,
            DateTime.UtcNow.AddDays(30),
            "100% on delivery",
            "Contract Base",
            new List<QuotationItemInput>
            {
                new(svcSec.Id, svcSec.Code, svcSec.Name, svcSec.Unit, 3, 25000000m, 0m, 10m, "SOC Monitoring")
            }
        ));
        var quoteResult = await quoteResp.Content.ReadFromJsonAsync<ApiResponse<QuotationDto>>(JsonOptions);
        var quoteId = quoteResult!.Data!.Id;
        await _client.PostAsJsonAsync($"/api/v1/quotations/{quoteId}/approve", new ApproveQuotationRequest("Approved"));

        // 3. Ensure a Contract Template exists
        var tmplCode = $"TMPL-{Guid.NewGuid():N}"[..10].ToUpper();
        var tmplResp = await _client.PostAsJsonAsync("/api/v1/contract-templates", new CreateContractTemplateRequest(
            tmplCode,
            "Managed Cloud Services Agreement",
            "SOW",
            "Cloud management agreement",
            "Client {{CustomerName}} agreed to contract {{ContractNumber}} for {{TotalValue}}. Start: {{StartDate}}, End: {{EndDate}}.",
            false,
            true
        ));
        var tmplResult = await tmplResp.Content.ReadFromJsonAsync<ApiResponse<ContractTemplateDto>>(JsonOptions);
        var templateId = tmplResult!.Data!.Id;

        // 4. Generate Contract from Quotation
        var contractResp = await _client.PostAsJsonAsync("/api/v1/contracts/from-quotation", new GenerateContractFromQuotationRequest(
            quoteId,
            templateId,
            $"Managed Security Agreement - {account.Name}",
            DateTime.UtcNow,
            DateTime.UtcNow.AddMonths(12),
            "Quarterly advance payments",
            "Strict 99.9% uptime SLA"
        ));
        var contractBody = await contractResp.Content.ReadAsStringAsync();
        _output.WriteLine($"Contract From Quotation Response ({contractResp.StatusCode}): {contractBody}");
        Assert.Equal(HttpStatusCode.OK, contractResp.StatusCode);
        var contractResult = await contractResp.Content.ReadFromJsonAsync<ApiResponse<ContractDto>>(JsonOptions);
        Assert.NotNull(contractResult?.Data);
        var contractId = contractResult.Data.Id;
        Assert.Equal(ContractStatus.Draft, contractResult.Data.Status);
        Assert.Equal(quoteResult.Data.TotalAmount, contractResult.Data.TotalValue);

        // 5. Verify Rendered Content
        var contentResp = await _client.GetAsync($"/api/v1/contracts/{contractId}/content");
        Assert.Equal(HttpStatusCode.OK, contentResp.StatusCode);
        var contentResult = await contentResp.Content.ReadFromJsonAsync<ApiResponse<string>>(JsonOptions);
        Assert.Contains(account.Name, contentResult?.Data ?? "");

        // 6. Download Contract PDF
        var contractPdfResp = await _client.GetAsync($"/api/v1/contracts/{contractId}/pdf");
        Assert.Equal(HttpStatusCode.OK, contractPdfResp.StatusCode);
        Assert.Equal("application/pdf", contractPdfResp.Content.Headers.ContentType?.MediaType);
        var contractPdfBytes = await contractPdfResp.Content.ReadAsByteArrayAsync();
        Assert.True(contractPdfBytes.Length > 1000);

        // 7. Add Contract Appendix
        var appendixResp = await _client.PostAsJsonAsync($"/api/v1/contracts/{contractId}/appendices", new CreateContractAppendixRequest(
            "Appendix 1: Extended Night Shift SOC Coverage",
            "24/7 dedicated security monitoring analyst",
            20000000m,
            DateTime.UtcNow
        ));
        Assert.Equal(HttpStatusCode.OK, appendixResp.StatusCode);
        var appendixResult = await appendixResp.Content.ReadFromJsonAsync<ApiResponse<ContractAppendixDto>>(JsonOptions);
        Assert.Equal("APP-01", appendixResult?.Data?.AppendixNumber);

        // Verify contract total value adjusted
        var updatedContractResp = await _client.GetAsync($"/api/v1/contracts/{contractId}");
        var updatedContract = await updatedContractResp.Content.ReadFromJsonAsync<ApiResponse<ContractDto>>(JsonOptions);
        Assert.Equal(contractResult.Data.TotalValue + 20000000m, updatedContract?.Data?.TotalValue);

        // 8. E-Signature Initiation
        var esignSendResp = await _client.PostAsJsonAsync("/api/v1/e-signature/send", new CreateSignatureSessionRequest(
            contractId,
            "Tran Thi Bich",
            "bich.tran@partnercorp.vn"
        ));
        Assert.Equal(HttpStatusCode.OK, esignSendResp.StatusCode);
        var esignResult = await esignSendResp.Content.ReadFromJsonAsync<ApiResponse<SignatureSessionResult>>(JsonOptions);
        Assert.NotNull(esignResult?.Data);
        Assert.NotEmpty(esignResult.Data.ExternalEnvelopeId);
        Assert.NotEmpty(esignResult.Data.SigningUrl);

        // 9. E-Signature Webhook Callback
        var webhookResp = await _client.PostAsJsonAsync("/api/v1/e-signature/webhook", new WebhookEventDto(
            esignResult.Data.Provider,
            "envelope-completed",
            esignResult.Data.ExternalEnvelopeId,
            "Signed",
            "bich.tran@partnercorp.vn",
            DateTime.UtcNow,
            "14.232.208.188",
            "{\"status\":\"signed\",\"completed\":true}"
        ));
        Assert.Equal(HttpStatusCode.OK, webhookResp.StatusCode);

        // Verify contract is now digitally Signed
        var signedContractResp = await _client.GetAsync($"/api/v1/contracts/{contractId}");
        var signedContract = await signedContractResp.Content.ReadFromJsonAsync<ApiResponse<ContractDto>>(JsonOptions);
        Assert.Equal(ContractStatus.Signed, signedContract?.Data?.Status);
        Assert.Equal("Tran Thi Bich", signedContract?.Data?.SignedByClientName);
        Assert.NotNull(signedContract?.Data?.SignedAt);

        // 10. Renew Contract
        var renewResp = await _client.PostAsJsonAsync($"/api/v1/contracts/{contractId}/renew", new RenewContractRequest(
            DateTime.UtcNow.AddMonths(12),
            DateTime.UtcNow.AddMonths(24),
            signedContract!.Data!.TotalValue * 1.1m,
            "Renewed for another 12 months with 10% indexation."
        ));
        Assert.Equal(HttpStatusCode.OK, renewResp.StatusCode);
        var renewResult = await renewResp.Content.ReadFromJsonAsync<ApiResponse<ContractDto>>(JsonOptions);
        Assert.NotNull(renewResult?.Data);
        Assert.Equal(1, renewResult.Data.RenewalCount);
        Assert.Equal(contractId, renewResult.Data.ParentContractId);

        // Verify original contract status is Renewed
        var origRenewedResp = await _client.GetAsync($"/api/v1/contracts/{contractId}");
        var origRenewed = await origRenewedResp.Content.ReadFromJsonAsync<ApiResponse<ContractDto>>(JsonOptions);
        Assert.Equal(ContractStatus.Renewed, origRenewed?.Data?.Status);
    }
}
