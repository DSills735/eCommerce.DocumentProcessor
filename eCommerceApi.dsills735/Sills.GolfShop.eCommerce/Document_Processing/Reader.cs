using CsvHelper;
using ExcelDataReader;
using Sills.GolfShop.eCommerceAPI.DTO;
using System.Data;
using System.Globalization;
using System.Reflection;
using System.Text;

namespace Sills.GolfShop.eCommerceAPI.Document_Processing;

public class Reader
{
    public async Task<BulkSeedPayloadDto> ReadAndSeedAsync(string apiEndpointUrl, string resourceName)
    {
        try
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

            var assembly = Assembly.GetExecutingAssembly();
            var categories = new List<SeedingCategoryDto>();
            var products = new List<SeedingProductDto>();

            using (var stream = assembly.GetManifestResourceStream(resourceName))
            {
                if (stream == null)
                {
                    throw new FileNotFoundException($"Embedded resource '{resourceName}' not found. Ensure the file is added to the project and marked as an embedded resource.");
                }

                if (resourceName.EndsWith(".csv", StringComparison.OrdinalIgnoreCase))
                {
                    (categories, products) = await ReadCsvAsync(stream);
                }
                else if (resourceName.EndsWith(".xlsx", StringComparison.OrdinalIgnoreCase) || 
                         resourceName.EndsWith(".xls", StringComparison.OrdinalIgnoreCase))
                {
                    (categories, products) = await ReadExcelAsync(stream);
                }
                else
                {
                    throw new InvalidOperationException($"Unsupported file format: {Path.GetExtension(resourceName)}. Supported formats: .xlsx, .xls, .csv");
                }
            }

            var payload = new BulkSeedPayloadDto(categories, products);
            return await Task.FromResult(payload);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ERROR] Failed to read spreadsheet: {ex.Message}");
            throw;
        }
    }

    private async Task<(List<SeedingCategoryDto>, List<SeedingProductDto>)> ReadExcelAsync(Stream stream)
    {
        var categories = new List<SeedingCategoryDto>();
        var products = new List<SeedingProductDto>();

        try
        {
            using (var reader = ExcelReaderFactory.CreateReader(stream))
            {
                var config = new ExcelDataSetConfiguration
                {
                    ConfigureDataTable = _ => new ExcelDataTableConfiguration
                    {
                        UseHeaderRow = true
                    }
                };
                var dataset = reader.AsDataSet(config);

                if (dataset.Tables.Contains("Categories"))
                {
                    DataTable categoryTable = dataset.Tables["Categories"];
                    foreach (DataRow row in categoryTable.Rows)
                    {
                        try
                        {
                            categories.Add(new SeedingCategoryDto(
                                Name: row["Name"]?.ToString()?.Trim() ?? string.Empty,
                                Description: row["Description"]?.ToString()?.Trim() ?? string.Empty
                            ));
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"[WARNING] Skipped invalid category row: {ex.Message}");
                        }
                    }
                }

                if (dataset.Tables.Contains("Products"))
                {
                    DataTable productTable = dataset.Tables["Products"];
                    foreach (DataRow row in productTable.Rows)
                    {
                        try
                        {
                            int quantity = 0;
                            if (row["QuantityInStock"] != DBNull.Value && 
                                int.TryParse(row["QuantityInStock"].ToString(), out int qty))
                            {
                                quantity = qty;
                            }

                            products.Add(new SeedingProductDto(
                                Name: row["Name"]?.ToString()?.Trim() ?? string.Empty,
                                Description: row["Description"]?.ToString()?.Trim() ?? string.Empty,
                                QuantityInStock: quantity
                            ));
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"[WARNING] Skipped invalid product row: {ex.Message}");
                        }
                    }
                }
            }
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to parse Excel file: {ex.Message}", ex);
        }

        return (categories, products);
    }

    private async Task<(List<SeedingCategoryDto>, List<SeedingProductDto>)> ReadCsvAsync(Stream stream)
    {
        var categories = new List<SeedingCategoryDto>();
        var products = new List<SeedingProductDto>();

        try
        {
            using (var reader = new StreamReader(stream, Encoding.UTF8))
            using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
            {
                var records = csv.GetRecords<dynamic>().ToList();

                int separatorIndex = -1;
                for (int i = 0; i < records.Count; i++)
                {
                    var record = (IDictionary<string, object>)records[i];
                    if (record.Values.All(v => v == null || string.IsNullOrWhiteSpace(v.ToString())))
                    {
                        separatorIndex = i;
                        break;
                    }
                }

                if (separatorIndex == -1)
                {
                    separatorIndex = records.Count / 2;
                }

                for (int i = 0; i < separatorIndex; i++)
                {
                    try
                    {
                        var record = (IDictionary<string, object>)records[i];
                        if (record.TryGetValue("Name", out var nameObj) && nameObj != null)
                        {
                            categories.Add(new SeedingCategoryDto(
                                Name: nameObj.ToString()?.Trim() ?? string.Empty,
                                Description: record.TryGetValue("Description", out var descObj) 
                                    ? descObj?.ToString()?.Trim() ?? string.Empty 
                                    : string.Empty
                            ));
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"[WARNING] Skipped invalid category row: {ex.Message}");
                    }
                }

                for (int i = separatorIndex + 1; i < records.Count; i++)
                {
                    try
                    {
                        var record = (IDictionary<string, object>)records[i];
                        if (record.TryGetValue("Name", out var nameObj) && nameObj != null)
                        {
                            int quantity = 0;
                            if (record.TryGetValue("QuantityInStock", out var qtyObj) && qtyObj != null)
                            {
                                int.TryParse(qtyObj.ToString(), out quantity);
                            }

                            products.Add(new SeedingProductDto(
                                Name: nameObj.ToString()?.Trim() ?? string.Empty,
                                Description: record.TryGetValue("Description", out var descObj) 
                                    ? descObj?.ToString()?.Trim() ?? string.Empty 
                                    : string.Empty,
                                QuantityInStock: quantity
                            ));
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"[WARNING] Skipped invalid product row: {ex.Message}");
                    }
                }
            }
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to parse CSV file: {ex.Message}", ex);
        }

        return (categories, products);
    }

    public static List<string> GetAvailableResources()
    {
        var assembly = Assembly.GetExecutingAssembly();
        return assembly.GetManifestResourceNames()
            .Where(name => name.EndsWith(".xlsx", StringComparison.OrdinalIgnoreCase) ||
                          name.EndsWith(".xls", StringComparison.OrdinalIgnoreCase) ||
                          name.EndsWith(".csv", StringComparison.OrdinalIgnoreCase))
            .ToList();
    }
}
