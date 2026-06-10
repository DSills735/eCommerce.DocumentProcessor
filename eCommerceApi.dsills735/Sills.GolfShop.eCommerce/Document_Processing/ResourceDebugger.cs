using System.Reflection;

namespace Sills.GolfShop.eCommerceAPI.Document_Processing;

public static class ResourceDebugger
{
    public static void PrintAllResources()
    {
        var assembly = Assembly.GetExecutingAssembly();
        var resources = assembly.GetManifestResourceNames();

        Console.WriteLine("\n[DEBUG] Available Embedded Resources:");
        Console.WriteLine("=====================================");

        if (resources.Length == 0)
        {
            Console.WriteLine("  (No resources found)");
        }
        else
        {
            foreach (var resource in resources.OrderBy(r => r))
            {
                Console.WriteLine($"  • {resource}");
            }
        }

        Console.WriteLine("=====================================\n");
    }

    public static void PrintSpreadsheetResources()
    {
        var assembly = Assembly.GetExecutingAssembly();
        var spreadsheetResources = assembly.GetManifestResourceNames()
            .Where(name => name.EndsWith(".xlsx", StringComparison.OrdinalIgnoreCase) ||
                          name.EndsWith(".xls", StringComparison.OrdinalIgnoreCase) ||
                          name.EndsWith(".csv", StringComparison.OrdinalIgnoreCase))
            .ToList();

        Console.WriteLine("\n[DEBUG] Available Spreadsheet Resources:");
        Console.WriteLine("=====================================");

        if (spreadsheetResources.Count == 0)
        {
            Console.WriteLine("  (No spreadsheet resources found!)");
        }
        else
        {
            foreach (var resource in spreadsheetResources)
            {
                Console.WriteLine($"  • {resource}");
            }
        }

        Console.WriteLine("=====================================\n");
    }
}
