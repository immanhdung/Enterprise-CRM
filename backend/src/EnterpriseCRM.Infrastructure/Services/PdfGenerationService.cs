using System.Globalization;
using EnterpriseCRM.Application.Features.Quotations;
using EnterpriseCRM.Domain.Entities;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace EnterpriseCRM.Infrastructure.Services;

public class PdfGenerationService : IPdfGenerationService
{
    static PdfGenerationService()
    {
        QuestPDF.Settings.License = LicenseType.Community;
    }

    public Task<byte[]> GenerateQuotationPdfAsync(Quotation quotation, CancellationToken cancellationToken = default)
    {
        var doc = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(30);
                page.PageColor(Colors.White);
                page.DefaultTextStyle(x => x.FontSize(10).FontColor(Colors.Grey.Darken3));

                page.Header().Element(header => ComposeHeader(header, "OFFICIAL SERVICE QUOTATION", quotation.QuotationNumber, quotation.RevisionNumber));
                page.Content().Element(content => ComposeQuotationContent(content, quotation));
                page.Footer().Element(ComposeFooter);
            });
        });

        var bytes = doc.GeneratePdf();
        return Task.FromResult(bytes);
    }

    public Task<byte[]> GenerateContractPdfAsync(Contract contract, CancellationToken cancellationToken = default)
    {
        var doc = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(30);
                page.PageColor(Colors.White);
                page.DefaultTextStyle(x => x.FontSize(10).FontColor(Colors.Grey.Darken3));

                page.Header().Element(header => ComposeHeader(header, "SERVICE CONTRACT AGREEMENT", contract.ContractNumber, $"Rev: {contract.RenewalCount}"));
                page.Content().Element(content => ComposeContractContent(content, contract));
                page.Footer().Element(ComposeFooter);
            });
        });

        var bytes = doc.GeneratePdf();
        return Task.FromResult(bytes);
    }

    private void ComposeHeader(IContainer container, string title, string code, string revision)
    {
        container.Row(row =>
        {
            row.RelativeItem().Column(col =>
            {
                col.Item().Text("ENTERPRISE CRM SERVICES INC.").FontSize(16).Bold().FontColor(Colors.Blue.Darken2);
                col.Item().Text("123 Business Avenue, Suite 500, Tech City");
                col.Item().Text("Phone: (+84) 28 3822 9999 | Email: contact@enterprisecrm.com");
                col.Item().Text("Tax Code: 0109887766");
            });

            row.ConstantItem(180).Column(col =>
            {
                col.Item().AlignRight().Text(title).FontSize(13).Bold().FontColor(Colors.Grey.Darken4);
                col.Item().AlignRight().Text($"Doc #: {code}").FontSize(10).Bold();
                col.Item().AlignRight().Text($"Version: {revision}").FontSize(9).FontColor(Colors.Grey.Darken1);
            });
        });
    }

    private void ComposeQuotationContent(IContainer container, Quotation q)
    {
        container.PaddingVertical(10).Column(col =>
        {
            col.Item().LineHorizontal(1).LineColor(Colors.Grey.Lighten2);

            // Information Block
            col.Item().PaddingVertical(10).Row(row =>
            {
                row.RelativeItem().Column(c =>
                {
                    c.Item().Text("CLIENT INFORMATION").Bold().FontSize(10).FontColor(Colors.Blue.Darken2);
                    c.Item().Text($"Account: {q.Account?.Name ?? "N/A"}").Bold();
                    c.Item().Text($"Contact: {q.Contact?.FullName ?? "N/A"}");
                    c.Item().Text($"Email: {q.Contact?.Email ?? q.SentToEmail ?? "N/A"}");
                    c.Item().Text($"Phone: {q.Contact?.Phone ?? "N/A"}");
                });

                row.RelativeItem().Column(c =>
                {
                    c.Item().Text("QUOTATION DETAILS").Bold().FontSize(10).FontColor(Colors.Blue.Darken2);
                    c.Item().Text($"Issue Date: {q.CreatedAt:yyyy-MM-dd}");
                    c.Item().Text($"Valid Until: {q.ValidUntil:yyyy-MM-dd}").Bold();
                    c.Item().Text($"Status: {q.Status}");
                    c.Item().Text($"Price List: {q.PriceList?.Name ?? "Standard"}");
                });
            });

            // Table of items
            col.Item().PaddingTop(10).Table(table =>
            {
                table.ColumnsDefinition(columns =>
                {
                    columns.ConstantColumn(30);  // #
                    columns.RelativeColumn(3);   // Service
                    columns.ConstantColumn(45);  // Unit
                    columns.ConstantColumn(40);  // Qty
                    columns.RelativeColumn(1.5f); // Unit Price
                    columns.ConstantColumn(45);  // Disc %
                    columns.RelativeColumn(1.5f); // Total
                });

                // Header
                table.Header(header =>
                {
                    header.Cell().Background(Colors.Grey.Lighten3).Padding(4).Text("#").Bold();
                    header.Cell().Background(Colors.Grey.Lighten3).Padding(4).Text("Service / Description").Bold();
                    header.Cell().Background(Colors.Grey.Lighten3).Padding(4).Text("Unit").Bold();
                    header.Cell().Background(Colors.Grey.Lighten3).Padding(4).AlignRight().Text("Qty").Bold();
                    header.Cell().Background(Colors.Grey.Lighten3).Padding(4).AlignRight().Text("Unit Price").Bold();
                    header.Cell().Background(Colors.Grey.Lighten3).Padding(4).AlignRight().Text("Disc%").Bold();
                    header.Cell().Background(Colors.Grey.Lighten3).Padding(4).AlignRight().Text("Line Total").Bold();
                });

                var idx = 1;
                foreach (var item in q.Items)
                {
                    var bg = idx % 2 == 0 ? Colors.Grey.Lighten4 : Colors.White;
                    table.Cell().Background(bg).Padding(4).Text(idx.ToString());
                    table.Cell().Background(bg).Padding(4).Text($"{item.ServiceName} ({item.ServiceCode})");
                    table.Cell().Background(bg).Padding(4).Text(item.Unit);
                    table.Cell().Background(bg).Padding(4).AlignRight().Text(item.Quantity.ToString());
                    table.Cell().Background(bg).Padding(4).AlignRight().Text(item.UnitPrice.ToString("N0", CultureInfo.InvariantCulture));
                    table.Cell().Background(bg).Padding(4).AlignRight().Text($"{item.DiscountRate:0.#}%");
                    table.Cell().Background(bg).Padding(4).AlignRight().Text(item.LineTotal.ToString("N0", CultureInfo.InvariantCulture));
                    idx++;
                }
            });

            // Summary Totals
            col.Item().PaddingTop(10).AlignRight().Row(row =>
            {
                row.ConstantItem(260).Column(c =>
                {
                    c.Item().Row(r =>
                    {
                        r.RelativeItem().Text("Subtotal:").AlignRight();
                        r.RelativeItem().AlignRight().Text(q.Subtotal.ToString("N0", CultureInfo.InvariantCulture));
                    });
                    if (q.DiscountAmount > 0)
                    {
                        c.Item().Row(r =>
                        {
                            r.RelativeItem().Text("Total Discount:").AlignRight().FontColor(Colors.Red.Darken1);
                            r.RelativeItem().AlignRight().Text($"-{q.DiscountAmount.ToString("N0", CultureInfo.InvariantCulture)}").FontColor(Colors.Red.Darken1);
                        });
                    }
                    c.Item().Row(r =>
                    {
                        r.RelativeItem().Text("VAT Amount:").AlignRight();
                        r.RelativeItem().AlignRight().Text(q.VatAmount.ToString("N0", CultureInfo.InvariantCulture));
                    });
                    c.Item().LineHorizontal(1).LineColor(Colors.Grey.Medium);
                    c.Item().Row(r =>
                    {
                        r.RelativeItem().Text("Total Amount:").Bold().FontSize(12).AlignRight();
                        r.RelativeItem().AlignRight().Text($"{q.TotalAmount.ToString("N0", CultureInfo.InvariantCulture)} VND").Bold().FontSize(12).FontColor(Colors.Blue.Darken2);
                    });
                });
            });

            // Terms
            if (!string.IsNullOrWhiteSpace(q.TermsAndConditions))
            {
                col.Item().PaddingTop(15).Column(c =>
                {
                    c.Item().Text("TERMS & CONDITIONS").Bold().FontSize(9).FontColor(Colors.Grey.Darken3);
                    c.Item().Text(q.TermsAndConditions).FontSize(8.5f).FontColor(Colors.Grey.Darken2);
                });
            }

            // Signatures block
            col.Item().PaddingTop(30).Row(row =>
            {
                row.RelativeItem().Column(c =>
                {
                    c.Item().AlignCenter().Text("Prepared By").Bold();
                    c.Item().PaddingTop(40).AlignCenter().Text("_________________________");
                    c.Item().AlignCenter().Text("Account Manager").FontSize(8.5f);
                });
                row.RelativeItem().Column(c =>
                {
                    c.Item().AlignCenter().Text("Client Acceptance").Bold();
                    c.Item().PaddingTop(40).AlignCenter().Text("_________________________");
                    c.Item().AlignCenter().Text("Authorized Signature & Seal").FontSize(8.5f);
                });
            });
        });
    }

    private void ComposeContractContent(IContainer container, Contract c)
    {
        container.PaddingVertical(10).Column(col =>
        {
            col.Item().LineHorizontal(1).LineColor(Colors.Grey.Lighten2);

            // Information Block
            col.Item().PaddingVertical(10).Row(row =>
            {
                row.RelativeItem().Column(cl =>
                {
                    cl.Item().Text("PARTY A (SERVICE PROVIDER)").Bold().FontSize(10).FontColor(Colors.Blue.Darken2);
                    cl.Item().Text("ENTERPRISE CRM SERVICES INC.").Bold();
                    cl.Item().Text("Tax Code: 0109887766");
                    cl.Item().Text("Representative: General Director");
                });

                row.RelativeItem().Column(cl =>
                {
                    cl.Item().Text("PARTY B (CLIENT)").Bold().FontSize(10).FontColor(Colors.Blue.Darken2);
                    cl.Item().Text(c.Account?.Name ?? "Client Company").Bold();
                    cl.Item().Text($"Contact: {c.Contact?.FullName ?? "N/A"}");
                    cl.Item().Text($"Email: {c.Contact?.Email ?? "N/A"}");
                });
            });

            col.Item().PaddingVertical(5).Row(row =>
            {
                row.RelativeItem().Text($"Contract Title: {c.Title}").Bold().FontSize(11);
                row.ConstantItem(200).AlignRight().Text($"Period: {c.StartDate:yyyy-MM-dd} to {c.EndDate:yyyy-MM-dd}").Bold();
            });

            if (!string.IsNullOrWhiteSpace(c.ScopeOfWork))
            {
                col.Item().PaddingTop(10).Column(cl =>
                {
                    cl.Item().Text("ARTICLE 1: SCOPE OF SERVICES").Bold().FontSize(10).FontColor(Colors.Blue.Darken2);
                    cl.Item().Text(c.ScopeOfWork).FontSize(9);
                });
            }

            if (!string.IsNullOrWhiteSpace(c.PaymentTerms))
            {
                col.Item().PaddingTop(10).Column(cl =>
                {
                    cl.Item().Text("ARTICLE 2: CONTRACT VALUE & PAYMENT TERMS").Bold().FontSize(10).FontColor(Colors.Blue.Darken2);
                    cl.Item().Text($"Total Contract Value: {c.TotalValue.ToString("N0", CultureInfo.InvariantCulture)} VND").Bold();
                    cl.Item().Text(c.PaymentTerms).FontSize(9);
                });
            }

            if (!string.IsNullOrWhiteSpace(c.SpecialTerms))
            {
                col.Item().PaddingTop(10).Column(cl =>
                {
                    cl.Item().Text("ARTICLE 3: SPECIAL TERMS & CONDITIONS").Bold().FontSize(10).FontColor(Colors.Blue.Darken2);
                    cl.Item().Text(c.SpecialTerms).FontSize(9);
                });
            }

            // Signatures block
            col.Item().PaddingTop(40).Row(row =>
            {
                row.RelativeItem().Column(cl =>
                {
                    cl.Item().AlignCenter().Text("REPRESENTATIVE OF PARTY A").Bold();
                    cl.Item().PaddingTop(45).AlignCenter().Text("_________________________");
                    cl.Item().AlignCenter().Text("Authorized Signature").FontSize(8.5f);
                });
                row.RelativeItem().Column(cl =>
                {
                    cl.Item().AlignCenter().Text("REPRESENTATIVE OF PARTY B").Bold();
                    if (c.SignedAt.HasValue)
                    {
                        cl.Item().PaddingTop(15).AlignCenter().Text($"SIGNED DIGITALLY").Bold().FontColor(Colors.Green.Darken2);
                        cl.Item().AlignCenter().Text($"By: {c.SignedByClientName} ({c.SignedByClientEmail})").FontSize(8);
                        cl.Item().AlignCenter().Text($"Timestamp: {c.SignedAt:yyyy-MM-dd HH:mm:ss} UTC").FontSize(8);
                        cl.Item().AlignCenter().Text($"IP: {c.SignerIpAddress ?? "N/A"}").FontSize(8);
                    }
                    else
                    {
                        cl.Item().PaddingTop(45).AlignCenter().Text("_________________________");
                        cl.Item().AlignCenter().Text("Pending Signature").FontSize(8.5f);
                    }
                });
            });
        });
    }

    private void ComposeFooter(IContainer container)
    {
        container.Row(row =>
        {
            row.RelativeItem().Text("Confidential - Generated by Enterprise CRM").FontSize(8).FontColor(Colors.Grey.Medium);
            row.ConstantItem(100).AlignRight().Text(x =>
            {
                x.Span("Page ");
                x.CurrentPageNumber();
                x.Span(" of ");
                x.TotalPages();
            });
        });
    }
}
