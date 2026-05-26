using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.EntityFrameworkCore; 
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using Sills.GolfShop.eCommerceAPI.Models;
using Sills.GolfShop.eCommerceAPI.Services;

namespace Sills.GolfShop.eCommerceAPI.Document_Processing;

public class ReportGenerator
{
    public static async Task<string> CreateAndUploadInventoryReportAsync(
        ICategoryService categoryService,
        IProductsService productsService,
        BlobServiceClient blobServiceClient)
    {
        QuestPDF.Settings.License = LicenseType.Community;


        var categories = await categoryService.GetAllCategoriesQuery().ToListAsync();

        var products = await productsService.GetAllProductsAsync();

        using var pdfStream = new MemoryStream();

        Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(2, Unit.Centimetre);
                page.DefaultTextStyle(x => x.FontSize(12).FontColor(Colors.Black));

                page.Header().Column(column =>
                {
                    column.Item().Text("Inventory Report").SemiBold().FontSize(20).FontColor(Colors.Blue.Medium);
                    column.Item().Text($"Sills Golf Shop Inventory Report - {DateTime.Now:MMMM dd, yyyy}").Bold().FontSize(14).FontColor(Colors.Blue.Medium);
                });

                page.Content().PaddingVertical(1, Unit.Centimetre).Column(column =>
                {
                    
                    column.Item().Text("Categories").Bold().FontSize(16).Underline();
                    column.Item().PaddingBottom(0.5f, Unit.Centimetre).Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.ConstantColumn(50);
                            columns.RelativeColumn();
                        });

                        table.Header(header =>
                        {
                            header.Cell().Element(CellStyle).Text("ID").SemiBold();
                            header.Cell().Element(CellStyle).Text("Name").SemiBold();
                        });

                        foreach (var category in categories)
                        {
                            table.Cell().Element(CellStyle).Text(category.Id.ToString());
                            table.Cell().Element(CellStyle).Text(category.Name);
                        }
                    });

                   
                    column.Item().Text("Products").Bold().FontSize(16).Underline();
                    column.Item().Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.ConstantColumn(40);
                            columns.RelativeColumn(2);
                            columns.RelativeColumn(3);
                            columns.RelativeColumn(1);
                            columns.RelativeColumn(1);
                            columns.RelativeColumn(1);
                        });

                        table.Header(header =>
                        {
                            header.Cell().Element(CellStyle).Text("ID").SemiBold();
                            header.Cell().Element(CellStyle).Text("Name").SemiBold();
                            header.Cell().Element(CellStyle).Text("Description").SemiBold();
                            header.Cell().Element(CellStyle).Text("Price").SemiBold();
                            header.Cell().Element(CellStyle).Text("Qty").SemiBold();
                            header.Cell().Element(CellStyle).Text("Cat ID").SemiBold();
                        });

                        foreach (var product in products)
                        {
                            table.Cell().Element(CellStyle).Text(product.Id.ToString());
                            table.Cell().Element(CellStyle).Text(product.Name);
                            table.Cell().Element(CellStyle).Text(product.Description);
                            table.Cell().Element(CellStyle).Text($"${product.Price:F2}");
                            table.Cell().Element(CellStyle).Text(product.QuantityInStock.ToString());
                            table.Cell().Element(CellStyle).Text(product.CategoryId.ToString());
                        }
                    });
                });
            });
        })
        .GeneratePdf(pdfStream);

        pdfStream.Position = 0;

        var containerClient = blobServiceClient.GetBlobContainerClient("reports");
        await containerClient.CreateIfNotExistsAsync(PublicAccessType.None);

        string blobName = $"inventory-reports/InventoryReport_{DateTime.UtcNow:yyyyMMddHHmmss}.pdf";
        var blobClient = containerClient.GetBlobClient(blobName);

        await blobClient.UploadAsync(pdfStream, new BlobHttpHeaders { ContentType = "application/pdf" });

        return blobClient.Uri.ToString();
    }

    private static IContainer CellStyle(IContainer container)
    {
        return container.Border(1).BorderColor(Colors.Grey.Lighten2).Padding(5);
    }
}

