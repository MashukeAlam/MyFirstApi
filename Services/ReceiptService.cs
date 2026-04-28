using MyFirstApi.DTOs;
using MyFirstApi.Models;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace MyFirstApi.Services;

public class ReceiptService
{
    public ReceiptResponseDto BuildReceipt(SalesOrder order, string companyName)
    {
        var lines = order.Lines.Select(line => new ReceiptLineDto(
            ItemName: line.Item.Name,
            Quantity: line.Quantity,
            UnitPrice: line.UnitPrice,
            VatAmount: line.VatAmount,
            SdAmount: line.SdAmount,
            LineTotal: line.Quantity * line.UnitPrice + line.VatAmount + line.SdAmount
        )).ToList();

        var subtotal = lines.Sum(l => l.Quantity * l.UnitPrice);
        var totalVat = lines.Sum(l => l.VatAmount);
        var totalSd = lines.Sum(l => l.SdAmount);
        var grandTotal = subtotal + totalVat + totalSd;

        return new ReceiptResponseDto(
            SalesOrderId: order.Id,
            CompanyName: companyName,
            Date: order.Date,
            ReceiptNumber: $"SO-{order.Id}",
            CustomerName: order.Customer.Name,
            CustomerAddress: order.Customer.Address,
            Lines: lines,
            Subtotal: subtotal,
            TotalVat: totalVat,
            TotalSd: totalSd,
            GrandTotal: grandTotal
        );
    }

    public byte[] GenerateReceiptPdf(ReceiptResponseDto receipt)
    {
        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Margin(30);
                page.Size(PageSizes.A4);

                page.Header().Column(col =>
                {
                    col.Item().Text(receipt.CompanyName).FontSize(18).SemiBold();
                    col.Item().Text($"Date: {receipt.Date:yyyy-MM-dd}");
                    col.Item().Text($"Receipt: {receipt.ReceiptNumber}");
                });

                page.Content().Column(col =>
                {
                    col.Item().PaddingVertical(10).Text($"Customer: {receipt.CustomerName}");
                    col.Item().Text($"Address: {receipt.CustomerAddress}");

                    col.Item().PaddingTop(10).Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.RelativeColumn(3);
                            columns.RelativeColumn(1);
                            columns.RelativeColumn(2);
                            columns.RelativeColumn(2);
                            columns.RelativeColumn(2);
                            columns.RelativeColumn(2);
                        });

                        table.Header(header =>
                        {
                            header.Cell().Element(CellStyle).Text("Item");
                            header.Cell().Element(CellStyle).Text("Qty");
                            header.Cell().Element(CellStyle).Text("Unit Price");
                            header.Cell().Element(CellStyle).Text("VAT");
                            header.Cell().Element(CellStyle).Text("SD");
                            header.Cell().Element(CellStyle).Text("Line Total");
                        });

                        foreach (var line in receipt.Lines)
                        {
                            table.Cell().Element(CellStyle).Text(line.ItemName);
                            table.Cell().Element(CellStyle).Text(line.Quantity.ToString("0.##"));
                            table.Cell().Element(CellStyle).Text(line.UnitPrice.ToString("0.00"));
                            table.Cell().Element(CellStyle).Text(line.VatAmount.ToString("0.00"));
                            table.Cell().Element(CellStyle).Text(line.SdAmount.ToString("0.00"));
                            table.Cell().Element(CellStyle).Text(line.LineTotal.ToString("0.00"));
                        }
                    });

                    col.Item().PaddingTop(10).AlignRight().Column(totalCol =>
                    {
                        totalCol.Item().Text($"Subtotal: {receipt.Subtotal:0.00}");
                        totalCol.Item().Text($"Total VAT: {receipt.TotalVat:0.00}");
                        totalCol.Item().Text($"Total SD: {receipt.TotalSd:0.00}");
                        totalCol.Item().Text($"Grand Total: {receipt.GrandTotal:0.00}").SemiBold();
                    });
                });

                page.Footer().AlignCenter().Text("Thank you for your business");
            });
        });

        return document.GeneratePdf();
    }

    private static IContainer CellStyle(IContainer container)
    {
        return container.BorderBottom(1).BorderColor(Colors.Grey.Lighten2).PaddingVertical(5);
    }
}
