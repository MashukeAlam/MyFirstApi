using CsvHelper;
using CsvHelper.Configuration;
using Microsoft.EntityFrameworkCore;
using MyFirstApi.Data;
using MyFirstApi.DTOs;
using MyFirstApi.Models;
using OfficeOpenXml;
using System.Globalization;

namespace MyFirstApi.Services;

public class ItemImportService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<ItemImportService> _logger;

    public ItemImportService(IServiceScopeFactory scopeFactory, ILogger<ItemImportService> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    public async Task ProcessFileAsync(string filePath, string fileType)
    {
        var rows = fileType == "csv"
            ? ParseCsv(filePath)
            : ParseExcel(filePath);

        using var scope = _scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        int inserted = 0;
        int skipped = 0;

        foreach (var row in rows)
        {
            // skip if code already exists
            var exists = await db.Items.AnyAsync(i => i.Code == row.Code);
            if (exists)
            {
                skipped++;
                continue;
            }

            db.Items.Add(new Item
            {
                Name = row.Name,
                Code = row.Code,
                Unit = row.Unit,
                ItemType = row.ItemType,
                VatRate = row.VatRate,
                SdRate = row.SdRate
            });

            inserted++;
        }

        await db.SaveChangesAsync();

        _logger.LogInformation(
            "Import complete. Inserted: {inserted}, Skipped: {skipped}",
            inserted, skipped);

        // cleanup temp file
        if (File.Exists(filePath))
            File.Delete(filePath);
    }

    private List<ItemImportRow> ParseCsv(string filePath)
    {
        var config = new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            HasHeaderRecord = true,
            MissingFieldFound = null
        };

        using var reader = new StreamReader(filePath);
        using var csv = new CsvReader(reader, config);

        return csv.GetRecords<ItemImportRow>().ToList();
    }

    private List<ItemImportRow> ParseExcel(string filePath)
    {
        ExcelPackage.License.SetNonCommercialPersonal("MyFirstApi");

        using var package = new ExcelPackage(new FileInfo(filePath));
        var sheet = package.Workbook.Worksheets[0];
        var rows = new List<ItemImportRow>();

        // row 1 is header, start from row 2
        for (int row = 2; row <= sheet.Dimension.End.Row; row++)
        {
            var name = sheet.Cells[row, 1].Text.Trim();
            if (string.IsNullOrEmpty(name)) continue;

            rows.Add(new ItemImportRow(
                Name: name,
                Code: sheet.Cells[row, 2].Text.Trim(),
                Unit: sheet.Cells[row, 3].Text.Trim(),
                ItemType: sheet.Cells[row, 4].Text.Trim(),
                VatRate: decimal.TryParse(sheet.Cells[row, 5].Text, out var vat) ? vat : 0,
                SdRate: decimal.TryParse(sheet.Cells[row, 6].Text, out var sd) ? sd : 0
            ));
        }

        return rows;
    }
}