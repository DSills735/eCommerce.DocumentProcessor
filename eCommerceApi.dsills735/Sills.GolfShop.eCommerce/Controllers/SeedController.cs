using Microsoft.AspNetCore.Mvc;
using Sills.GolfShop.eCommerceAPI.DTO;
using Sills.GolfShop.eCommerceAPI.Models;
using Sills.GolfShop.eCommerceAPI.Services;
using System.Threading.Tasks;

namespace Sills.GolfShop.eCommerceAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SeedController(ICategoryService categoryService, IProductsService productService) : ControllerBase
{
    private readonly ICategoryService _categoryService = categoryService;
    private readonly IProductsService _productService = productService;

    [HttpPost("bulk")]
    public async Task<IActionResult> BulkSeed([FromBody] BulkSeedPayloadDto payload)
    {
        if (payload == null)
        {
            return BadRequest("Payload cannot be empty.");
        }

        foreach (var categoryDto in payload.Categories)
        {
            var categoryEntity = new Categories
            {
                Name = categoryDto.Name,
                Description = categoryDto.Description
            };

            await _categoryService.CreateCategoryAsync(categoryEntity);
        }


        foreach (var productDto in payload.Products)
        {
            var productEntity = new Product
            { 
                Name = productDto.Name,
                Description = productDto.Description,
                QuantityInStock = productDto.QuantityInStock
            };
            await _productService.CreateProductAsync(productEntity);
        }

        return Ok(new
        {
            Message = "Bulk seeding completed successfully.",
            CategoriesSeeded = payload.Categories.Count,
            ProductsSeeded = payload.Products.Count
        });
    }
}
