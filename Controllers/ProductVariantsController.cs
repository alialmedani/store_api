using Microsoft.AspNetCore.Mvc;
using Store.Dtos;
using Store.Services;

namespace Store.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductVariantsController : ControllerBase
{
	private readonly IProductVariantService _productVariantService;

	public ProductVariantsController(IProductVariantService productVariantService)
	{
		_productVariantService = productVariantService;
	}

	[HttpGet("by-product/{productId:int}")]
	public async Task<ActionResult<PagedResultDto<ProductVariantDto>>> GetByProductId(
		int productId,
		[FromQuery] PagedRequestDto input)
	{
		var variants = await _productVariantService.GetByProductIdAsync(productId, input);

		return Ok(variants);
	}

	[HttpGet("deleted")]
	public async Task<ActionResult<PagedResultDto<ProductVariantDto>>> GetDeleted([FromQuery] PagedRequestDto input)
	{
		var variants = await _productVariantService.GetDeletedAsync(input);

		return Ok(variants);
	}

	[HttpGet("{id:int}")]
	public async Task<ActionResult<ProductVariantDto>> GetById(int id)
	{
		var variant = await _productVariantService.GetByIdAsync(id);

		if (variant == null)
		{
			return NotFound();
		}

		return Ok(variant);
	}

	[HttpPost]
	public async Task<ActionResult<ProductVariantDto>> Create(CreateProductVariantDto dto)
	{
		var variant = await _productVariantService.CreateAsync(dto);

		if (variant == null)
		{
			return BadRequest("Product does not exist or variant already exists.");
		}

		return Ok(variant);
	}

	[HttpPut("{id:int}")]
	public async Task<ActionResult<ProductVariantDto>> Update(int id, UpdateProductVariantDto dto)
	{
		var variant = await _productVariantService.UpdateAsync(id, dto);

		if (variant == null)
		{
			return BadRequest("Variant does not exist or duplicate variant exists.");
		}

		return Ok(variant);
	}

	[HttpDelete("{id:int}")]
	public async Task<ActionResult> Delete(int id)
	{
		var deleted = await _productVariantService.DeleteAsync(id);

		if (!deleted)
		{
			return NotFound();
		}

		return NoContent();
	}

	[HttpPost("{id:int}/restore")]
	public async Task<ActionResult> Restore(int id)
	{
		var restored = await _productVariantService.RestoreAsync(id);

		if (!restored)
		{
			return NotFound();
		}

		return NoContent();
	}

	[HttpGet("available-colors/{productId:int}")]
	public async Task<ActionResult<List<string>>> GetAvailableColors(int productId)
	{
		var colors = await _productVariantService.GetAvailableColorsAsync(productId);

		return Ok(colors);
	}

	[HttpGet("available-sizes/{productId:int}")]
	public async Task<ActionResult<List<string>>> GetAvailableSizes(int productId)
	{
		var sizes = await _productVariantService.GetAvailableSizesAsync(productId);

		return Ok(sizes);
	}

	[HttpGet("available-colors-by-size/{productId:int}/{size}")]
	public async Task<ActionResult<List<string>>> GetAvailableColorsBySize(int productId, string size)
	{
		var colors = await _productVariantService.GetAvailableColorsBySizeAsync(productId, size);

		return Ok(colors);
	}

	[HttpGet("available-sizes-by-color/{productId:int}/{color}")]
	public async Task<ActionResult<List<string>>> GetAvailableSizesByColor(int productId, string color)
	{
		var sizes = await _productVariantService.GetAvailableSizesByColorAsync(productId, color);

		return Ok(sizes);
	}
}