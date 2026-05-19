using ExcelDataReader;
using Sills.GolfShop.eCommerceAPI.DTO;
using System.Data;
using System.Reflection;
using System.Text;


namespace Sills.GolfShop.eCommerceAPI.Document_Processing;

public class Reader
{
    private readonly HttpClient _httpClient;
    public Reader(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }
    //TODO -> https://your-server-domain/api/seed/bulk that is the endpoint url
 
    public async Task ExcelReader(string apiEndpointUrl)
    {
        //TODO - Still need to add filepath somewhere
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

        var assembly = Assembly.GetExecutingAssembly();
        var categories = new List<SeedingCategoryDto>();
        var products = new List<SeedingProductDto>();

        string resourceName = "YourNamespace.Resources.DataToSeed.xlsx";
        using (var Stream = assembly.GetManifestResourceStream(resourceName))
        {
            if(Stream == null)
            {
                throw new FileNotFoundException($"Embedded resource '{resourceName}' not found. Ensure the file is added to the project and marked as an embedded resource.");
            }

            using (var reader = ExcelReaderFactory.CreateReader(Stream))
            {
                
                var config = new ExcelDataSetConfiguration
                {
                    ConfigureDataTable = _ => new ExcelDataTableConfiguration
                    {
                        UseHeaderRow = true
                    }
                };
                var dataset = reader.AsDataSet(config);

                DataTable categoryTable = dataset.Tables["Categories"]!;
                if (categoryTable != null)
                {
                    foreach (DataRow row in categoryTable.Rows)
                    {
                        categories.Add(new SeedingCategoryDto(
                            Name: row["Name"]?.ToString() ?? string.Empty,
                            Description: row["Description"]?.ToString() ?? string.Empty
                        ));
                    }
                }

                DataTable productTable = dataset.Tables["Products"]!;
                if (productTable != null)
                {
                    foreach (DataRow row in productTable.Rows)
                    {
                        products.Add(new SeedingProductDto(
                            Name: row["Name"]?.ToString() ?? string.Empty,
                            Description: row["Description"]?.ToString() ?? string.Empty,
                            QuantityInStock: row["QuantityInStock"] != DBNull.Value
                                ? Convert.ToInt32(row["QuantityInStock"])
                                : 0
                        ));
                    }
                }
            }

            var payload = new BulkSeedPayloadDto(categories, products);

            Console.WriteLine($"Transmitting bulk seed payload containing {categories.Count} categories and {products.Count} products...");

            using var response = await _httpClient.PostAsJsonAsync(apiEndpointUrl, payload);

            if (response.IsSuccessStatusCode)
            {
                Console.WriteLine("Golf shop seed payload processed successfully by the API.");
            }
            else
            {
                string errorDetails = await response.Content.ReadAsStringAsync();
                throw new HttpRequestException($"API Seed Failure ({response.StatusCode}): {errorDetails}");
            }
        }
    }


}
