using Azure.Storage.Blobs.Models;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using Sills.GolfShop.eCommerceAPI.Models;

namespace Sills.GolfShop.eCommerceAPI.Document_Processing;

public class ReportGenerator
{
    public static void InventoryReportWriter()
    {
        QuestPDF.Settings.License = LicenseType.Community;

        Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(2, Unit.Centimetre);
                page.Header().Text("Inventory Report").SemiBold().FontSize(20).FontColor(Colors.Blue.Medium);
                page.DefaultTextStyle(x => x.FontSize(12).FontColor(Colors.Black));

                page.Header()
                    .Text($"Sills Golf Shop Inventory Report - {DateTime.Now:MMMM dd, yyyy}")
                    .Bold().FontSize(20).FontColor(Colors.Blue.Medium);

                //categories table
                page.Content()
                    .PaddingVertical(1, Unit.Centimetre)
                    .Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.ConstantColumn(50);
                            columns.RelativeColumn(2);

                        });
                        table.Header(header =>
                        {
                            header.Cell().Text("ID").SemiBold();
                            header.Cell().Text("Name").SemiBold();
                        });
                        var Categories = new List<Categories>
                        {
                            //get these from an api call i think?
                        };
                    });

                //products table
                page.Content()
                .PaddingVertical(1, Unit.Centimetre)
                .Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.ConstantColumn(50);
                            columns.RelativeColumn(2);   
                            columns.RelativeColumn(3);   
                            columns.RelativeColumn(1); 
                            columns.RelativeColumn(1);   
                            columns.RelativeColumn(1);   
                        });

                        table.Header(header =>
                        {
                            header.Cell().Text("ID").SemiBold();
                            header.Cell().Text("Name").SemiBold();
                            header.Cell().Text("Description").SemiBold();
                            header.Cell().Text("Price").SemiBold();
                            header.Cell().Text("Quantity").SemiBold();
                            header.Cell().Text("Category ID").SemiBold();
                        });

                        var products = new List<Product>
                        {
                            //get these from an api call ?
                        };
                        foreach (var product in products)
                        {
                            table.Cell().Text(product.Name);
                            table.Cell().Text(product.Description);
                            table.Cell().Text($"${product.Price:F2}");
                            table.Cell().Text(product.QuantityInStock.ToString());
                            table.Cell().Text(product.CategoryId.ToString());
                        }
                        static IContainer CellStyle(IContainer container)
                        {
                            return container.Border(1).Padding(5);
                        }



                    });

            });
            
        })
            .GeneratePdf("InventoryReport.pdf");
        
    }
}

