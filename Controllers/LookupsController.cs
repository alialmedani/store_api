using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Store.Data;
using Store.Dtos;
using Store.Models;

namespace Store.Controllers;

[ApiController]
[Route("api/[controller]")]
public class LookupsController : ControllerBase
{
	private readonly AppDbContext _context;

	public LookupsController(AppDbContext context)
	{
		_context = context;
	}

	// GET: /api/Lookups/categories
	[HttpGet("categories")]
	public async Task<ActionResult<List<CategoryLookupDto>>> GetCategories()
	{
		var items = await _context.Categories
			.Where(category => category.IsActive)
			.OrderBy(category => category.Name)
			.Select(category => new CategoryLookupDto
			{
				Id = category.Id,
				Name = category.Name,
				SizeType = new LookupDto
				{
					Id = (int)category.SizeType,
					Name = category.SizeType.ToString()
				}
			})
			.ToListAsync();

		return Ok(items);
	}

	// GET: /api/Lookups/category-size-types
	[HttpGet("category-size-types")]
	public ActionResult<List<LookupDto>> GetCategorySizeTypes()
	{
		var items = Enum.GetValues<CategorySizeType>()
			.Select(value => new LookupDto
			{
				Id = (int)value,
				Name = value.ToString()
			})
			.ToList();

		return Ok(items);
	}

	// GET: /api/Lookups/sizes-by-category-size-type/{sizeType}
	[HttpGet("sizes-by-category-size-type/{sizeType:int}")]
	public ActionResult<List<string>> GetSizesByCategorySizeType([FromRoute] CategorySizeType sizeType)
	{
		var sizes = sizeType switch
		{
			CategorySizeType.None => new List<string>(),

			CategorySizeType.ShoeNumeric => Enumerable
				.Range(36, 11) // 36 to 46
				.Select(size => size.ToString())
				.ToList(),

			CategorySizeType.ClothingLetter => new List<string>
			{
				"XS",
				"S",
				"M",
				"L",
				"XL",
				"XXL",
				"XXXL",
				"2XL",
				"3XL",
				"4XL"
			},

			CategorySizeType.KidsAge => new List<string>
			{
				"0-3M",
				"3-6M",
				"6-9M",
				"9-12M",
				"1Y",
				"2Y",
				"3Y",
				"4Y",
				"5Y",
				"6Y",
				"7Y",
				"8Y",
				"9Y",
				"10Y"
			},

			CategorySizeType.OneSize => new List<string>
			{
				"One Size"
			},

			CategorySizeType.Custom => new List<string>(),

			_ => new List<string>()
		};

		return Ok(sizes);
	}

	// GET: /api/Lookups/product-target-audiences
	[HttpGet("product-target-audiences")]
	public ActionResult<List<LookupDto>> GetProductTargetAudiences()
	{
		var items = Enum.GetValues<ProductTargetAudience>()
			.Select(value => new LookupDto
			{
				Id = (int)value,
				Name = value.ToString()
			})
			.ToList();

		return Ok(items);
	}

	// GET: /api/Lookups/stock-movement-types
	[HttpGet("stock-movement-types")]
	public ActionResult<List<LookupDto>> GetStockMovementTypes()
	{
		var items = Enum.GetValues<StockMovementType>()
			.Select(value => new LookupDto
			{
				Id = (int)value,
				Name = value.ToString()
			})
			.ToList();

		return Ok(items);
	}
}