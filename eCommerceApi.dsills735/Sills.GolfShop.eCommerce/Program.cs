using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi;
using Sills.GolfShop.eCommerceAPI.Data;
using Sills.GolfShop.eCommerceAPI.Services;
using Sills.GolfShop.eCommerceAPI.Document_Processing;
using Sills.GolfShop.eCommerceAPI.Models;
using Spectre.Console;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddControllers();
builder.Services.AddDbContext<GolfShopDbContext>(options => options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<ICategoryService, CategoryService>();
builder.Services.AddScoped<IProductsService, ProductsService>();
builder.Services.AddScoped<ISalesService, SalesService>();
builder.Services.AddScoped<Reader>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    using (var scope = app.Services.CreateScope())
    {
        var dbContext = scope.ServiceProvider.GetRequiredService<GolfShopDbContext>();
        dbContext.Database.EnsureDeleted();
        dbContext.Database.EnsureCreated();

        await AutoSeedDatabaseAsync(dbContext, scope);
    }
}

app.MapControllers();

app.Run();

static async Task AutoSeedDatabaseAsync(GolfShopDbContext dbContext, IServiceScope scope)
{
    try
    {
        bool hasCategories = await dbContext.Categories.AnyAsync();
        bool hasProducts = await dbContext.Products.AnyAsync();

        if (hasCategories && hasProducts)
        {
            AnsiConsole.MarkupLine("[green]✓ Database already populated with data.[/]");
            return;
        }

        AnsiConsole.MarkupLine("[yellow]→ Database is empty. Attempting to seed from spreadsheet...[/]");

        ResourceDebugger.PrintSpreadsheetResources();

        var availableResources = Reader.GetAvailableResources();
        if (availableResources.Count == 0)
        {
            AnsiConsole.MarkupLine("[yellow]⚠ No spreadsheet files found for seeding. Add .xlsx, .xls, or .csv files to Resources folder as embedded resources.[/]");
            return;
        }

        string seedFile = availableResources.First();
        AnsiConsole.MarkupLine($"[cyan]→ Found seed file: {Path.GetFileName(seedFile)}[/]");

        var reader = scope.ServiceProvider.GetRequiredService<Reader>();
        var categoryService = scope.ServiceProvider.GetRequiredService<ICategoryService>();
        var productService = scope.ServiceProvider.GetRequiredService<IProductsService>();

        try
        {
            var payload = await reader.ReadAndSeedAsync("", seedFile);

            if (payload.Categories.Count == 0 && payload.Products.Count == 0)
            {
                AnsiConsole.MarkupLine("[yellow]⚠ Spreadsheet is empty or has no valid data.[/]");
                return;
            }

            AnsiConsole.MarkupLine($"[cyan]→ Seeding {payload.Categories.Count} categories and {payload.Products.Count} products...[/]");

            foreach (var categoryDto in payload.Categories)
            {
                var categoryEntity = new Sills.GolfShop.eCommerceAPI.Models.Categories
                {
                    Name = categoryDto.Name,
                    Description = categoryDto.Description
                };
                await categoryService.CreateCategoryAsync(categoryEntity);
            }

            foreach (var productDto in payload.Products)
            {
                var productEntity = new Sills.GolfShop.eCommerceAPI.Models.Product
                {
                    Name = productDto.Name,
                    Description = productDto.Description,
                    QuantityInStock = productDto.QuantityInStock
                };
                await productService.CreateProductAsync(productEntity);
            }

            await DisplaySeededDataAsync(dbContext);

            AnsiConsole.MarkupLine("[green]✓ Database seeding completed successfully![/]");
        }
        catch (Exception ex)
        {
            AnsiConsole.MarkupLine($"[red]✗ Seeding failed: {ex.Message}[/]");
        }
    }
    catch (Exception ex)
    {
        AnsiConsole.MarkupLine($"[red]✗ Auto-seeding error: {ex.Message}[/]");
    }
}

static async Task DisplaySeededDataAsync(GolfShopDbContext dbContext)
{
    try
    {
        var categories = await dbContext.Categories.ToListAsync();
        var products = await dbContext.Products.ToListAsync();

        if (categories.Count > 0)
        {
            AnsiConsole.MarkupLine("\n[bold cyan]📋 Categories[/]");
            var categoryTable = new Table()
                .AddColumn("ID")
                .AddColumn("Name")
                .AddColumn("Description");

            foreach (var category in categories)
            {
                categoryTable.AddRow(
                    category.Id.ToString(),
                    category.Name ?? "-",
                    category.Description ?? "-"
                );
            }
            AnsiConsole.Write(categoryTable);
        }

        if (products.Count > 0)
        {
            AnsiConsole.MarkupLine("\n[bold cyan]📦 Products[/]");
            var productTable = new Table()
                .AddColumn("ID")
                .AddColumn("Name")
                .AddColumn("Description")
                .AddColumn("Qty")
                .AddColumn("Category ID");

            foreach (var product in products)
            {
                productTable.AddRow(
                    product.Id.ToString(),
                    product.Name ?? "-",
                    product.Description ?? "-",
                    product.QuantityInStock.ToString(),
                    product.CategoryId.ToString()
                );
            }
            AnsiConsole.Write(productTable);
        }

        AnsiConsole.MarkupLine($"\n[green]✓ Loaded {categories.Count} categories and {products.Count} products[/]\n");
    }
    catch (Exception ex)
    {
        AnsiConsole.MarkupLine($"[yellow]⚠ Could not display data: {ex.Message}[/]");
    }
}
