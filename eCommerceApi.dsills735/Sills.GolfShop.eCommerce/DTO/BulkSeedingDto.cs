namespace Sills.GolfShop.eCommerceAPI.DTO
{
    public sealed record BulkSeedPayloadDto(
        List<SeedingCategoryDto> Categories,
        List<SeedingProductDto> Products
    );
}
