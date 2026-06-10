# Golf Shop eCommerce API

A .NET 10 REST API for managing a golf shop inventory with automatic database seeding from spreadsheets (CSV, XLSX, XLS).

## Features

- Auto-seed database on first run
- Support for CSV, Excel (.xlsx, .xls) formats
- RESTful API with Swagger documentation
- PDF report generation
- Entity Framework Core with SQL Server

## Prerequisites

- .NET 10 SDK
- SQL Server LocalDB
- Visual Studio 2022+ (optional)

## Setup

1. Update connection string in `appsettings.json`:
```json
"ConnectionStrings": {
  "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=GolfShopDb;Trusted_Connection=True;MultipleActiveResultSets=true"
}
```

2. Add seed data file to `Resources/DataToSeed.csv` (or .xlsx/.xls)
3. Mark the seed file as "Embedded Resource" in Visual Studio properties

## Running

```bash
cd eCommerceApi.dsills735/Sills.GolfShop.eCommerce
dotnet clean
dotnet build
dotnet run
```

The app will automatically seed the database on first run if empty.

## API Endpoints

- `GET /api/category` - Get all categories
- `GET /api/category/{id}` - Get category by ID
- `POST /api/category` - Create category
- `GET /api/product` - Get all products
- `GET /api/product/{id}` - Get product by ID
- `POST /api/product` - Create product
- `GET /api/sales/report` - Generate PDF report
- `POST /api/seed/bulk` - Manually seed database

## Swagger Documentation

Access Swagger UI at: `https://localhost:7070/swagger`

## Technologies

- .NET 10
- Entity Framework Core 10
- SQL Server
- ExcelDataReader - For Excel file parsing
- CsvHelper - For CSV parsing
- QuestPDF - For PDF report generation
- Spectre.Console - For console formatting
- Azure Storage Blobs - For cloud storage
