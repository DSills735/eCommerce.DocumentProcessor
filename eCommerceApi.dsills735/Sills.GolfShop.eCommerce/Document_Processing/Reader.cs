using ExcelDataReader;
using Sills.GolfShop.eCommerceAPI.DTO;


namespace Sills.GolfShop.eCommerceAPI.Document_Processing;

public class Reader
{
    public static void ExcelReader()
    {
        //TODO - refactor filepath so not hardcoded

        using (var Stream = File.Open("C:\\Users\\sills\\Downloads\\Sills Golf Shop Inventory.xlsx", FileMode.Open, FileAccess.Read))
        {
            using (var reader = ExcelReaderFactory.CreateReader(Stream))
            {
                var categories = new List<SeedingCategoryDto>();
                var products = new List<SeedingProductDto>();

                var result = reader.AsDataSet();

                //read each individual table and send to dataset
                if (result.Tables.Contains("Categories"))
                {
                    var categoryTable = result.Tables["Categories"];
                    for (int i = 1; i < categoryTable.Rows.Count; i++)
                    {

                        //why cs7036 \/?
                        var category = new SeedingCategoryDto
                        {
                            Name = categoryTable.Rows[i][0].ToString(),
                            Description = categoryTable.Rows[i][1].ToString()
                        };
                        categories.Add(category);
                    }
                }


            }
        }
    }
}
