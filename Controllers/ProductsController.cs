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
	[ProducesResponseType(typeof(PagedResultDto<ProductDto>), StatusCodes.Status200OK)]
	public async Task<ActionResult<PagedResultDto<ProductDto>>> GetAll([FromQuery] PagedRequestDto input)
	{
		var products = await _productService.GetAllAsync(input);

		return Ok(products);
	}

	// GET: /api/Products/{id}
	[HttpGet("{id:int}")]
	[ProducesResponseType(typeof(ProductDetailsDto), StatusCodes.Status200OK)]
	[ProducesResponseType(StatusCodes.Status404NotFound)]
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
	[ProducesResponseType(typeof(ProductDto), StatusCodes.Status201Created)]
	[ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
	public async Task<ActionResult<ProductDto>> Create([FromBody] CreateProductDto dto)
	{
		var result = await _productService.CreateAsync(dto);

		if (!result.IsSuccess)
		{
			return BadRequest(result.ErrorMessage);
		}

		if (result.Data == null)
		{
			return BadRequest("Product was created but could not be loaded.");
		}

		return CreatedAtAction(
			nameof(GetById),
			new { id = result.Data.Id },
			result.Data);
	}

	// PUT: /api/Products/{id}
	[HttpPut("{id:int}")]
	[ProducesResponseType(typeof(ProductDto), StatusCodes.Status200OK)]
	[ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
	[ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
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
	[ProducesResponseType(StatusCodes.Status204NoContent)]
	[ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
	public async Task<ActionResult> Delete(int id)
	{
		var result = await _productService.DeleteAsync(id);

		if (!result.IsSuccess)
		{
			if (result.ErrorMessage == "Product does not exist.")
			{
				return NotFound(result.ErrorMessage);
			}

			return BadRequest(result.ErrorMessage);
		}

		return NoContent();
	}

	// GET: /api/Products/deleted
	[HttpGet("deleted")]
	[ProducesResponseType(typeof(PagedResultDto<ProductDto>), StatusCodes.Status200OK)]
	public async Task<ActionResult<PagedResultDto<ProductDto>>> GetDeleted([FromQuery] PagedRequestDto input)
	{
		var products = await _productService.GetDeletedAsync(input);

		return Ok(products);
	}

	// POST: /api/Products/{id}/restore
	[HttpPost("{id:int}/restore")]
	[ProducesResponseType(StatusCodes.Status204NoContent)]
	[ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
	[ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
	public async Task<ActionResult> Restore(int id)
	{
		var result = await _productService.RestoreAsync(id);

		if (!result.IsSuccess)
		{
			if (result.ErrorMessage == "Product does not exist or is not deleted.")
			{
				return NotFound(result.ErrorMessage);
			}

			return BadRequest(result.ErrorMessage);
		}

		return NoContent();
	}
}