using Microsoft.AspNetCore.Mvc;
using Store.Dtos;
using Store.Services;

namespace Store.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
	private readonly IProductService _productService;

	public ProductsController(IProductService productService)
	{
		_productService = productService;
	}

	// GET: /api/Products
	[HttpGet]
	public async Task<ActionResult<PagedResultDto<ProductDto>>> GetAll([FromQuery] PagedRequestDto input)
	{
		var products = await _productService.GetAllAsync(input);

		return Ok(products);
	}

	// GET: /api/Products/{id}
	[HttpGet("{id:int}")]
	public async Task<ActionResult<ProductDetailsDto>> GetById(int id)
	{
		var product = await _productService.GetByIdAsync(id);

		if (product == null)
		{
			return NotFound();
		}

		return Ok(product);
	}

	// POST: /api/Products
	[HttpPost]
	public async Task<ActionResult<ProductDto>> Create([FromBody] CreateProductDto dto)
	{
		var result = await _productService.CreateAsync(dto);

		if (!result.IsSuccess)
		{
			return BadRequest(result.ErrorMessage);
		}

		return Ok(result.Data);
	}

	// PUT: /api/Products/{id}
	[HttpPut("{id:int}")]
	public async Task<ActionResult<ProductDto>> Update(int id, [FromBody] UpdateProductDto dto)
	{
		var result = await _productService.UpdateAsync(id, dto);

		if (!result.IsSuccess)
		{
			if (result.ErrorMessage == "Product does not exist.")
			{
				return NotFound(result.ErrorMessage);
			}

			return BadRequest(result.ErrorMessage);
		}

		return Ok(result.Data);
	}

	// DELETE: /api/Products/{id}
	[HttpDelete("{id:int}")]
	public async Task<ActionResult> Delete(int id)
	{
		var deleted = await _productService.DeleteAsync(id);

		if (!deleted)
		{
			return NotFound();
		}

		return NoContent();
	}

	// GET: /api/Products/deleted
	[HttpGet("deleted")]
	public async Task<ActionResult<PagedResultDto<ProductDto>>> GetDeleted([FromQuery] PagedRequestDto input)
	{
		var products = await _productService.GetDeletedAsync(input);

		return Ok(products);
	}

	// POST: /api/Products/{id}/restore
	[HttpPost("{id:int}/restore")]
	public async Task<ActionResult> Restore(int id)
	{
		var restored = await _productService.RestoreAsync(id);

		if (!restored)
		{
			return NotFound();
		}

		return NoContent();
	}
}