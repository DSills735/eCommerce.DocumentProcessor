namespace Sills.GolfShop.eCommerceAPI.DTO;

public sealed record SeedingProductDto(string Name, string Description, int QuantityInStock);

public sealed record SeedingCategoryDto(string Name, string Description);
