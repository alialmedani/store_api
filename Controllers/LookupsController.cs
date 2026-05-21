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