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

	// GET: /api/ProductVariants/by-product/{productId}
	[HttpGet("by-product/{productId:int}")]
	public async Task<ActionResult<PagedResultDto<ProductVariantDto>>> GetByProductId(
		int productId,
		[FromQuery] PagedRequestDto input)
	{
		var variants = await _productVariantService.GetByProductIdAsync(productId, input);

		return Ok(variants);
	}

	// GET: /api/ProductVariants/{id}
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

	// POST: /api/ProductVariants
	[HttpPost]
	public async Task<ActionResult<ProductVariantDto>> Create([FromBody] CreateProductVariantDto dto)
	{
		var result = await _productVariantService.CreateAsync(dto);

		if (!result.IsSuccess)
		{
			return BadRequest(result.ErrorMessage);
		}

		return Ok(result.Data);
	}

	// POST: /api/ProductVariants/bulk
	[HttpPost("bulk")]
	public async Task<ActionResult<List<ProductVariantDto>>> BulkCreate([FromBody] CreateBulkProductVariantsDto dto)
	{
		var result = await _productVariantService.BulkCreateAsync(dto);

		if (!result.IsSuccess)
		{
			return BadRequest(result.ErrorMessage);
		}

		return Ok(result.Data);
	}

	// POST: /api/ProductVariants/generate
	[HttpPost("generate")]
	public async Task<ActionResult<List<ProductVariantDto>>> Generate([FromBody] GenerateProductVariantsDto dto)
	{
		var result = await _productVariantService.GenerateAsync(dto);

		if (!result.IsSuccess)
		{
			return BadRequest(result.ErrorMessage);
		}

		return Ok(result.Data);
	}

	// PUT: /api/ProductVariants/{id}
	[HttpPut("{id:int}")]
	public async Task<ActionResult<ProductVariantDto>> Update(int id, [FromBody] UpdateProductVariantDto dto)
	{
		var result = await _productVariantService.UpdateAsync(id, dto);

		if (!result.IsSuccess)
		{
			return BadRequest(result.ErrorMessage);
		}

		return Ok(result.Data);
	}

	// POST: /api/ProductVariants/{id}/adjust-stock
	[HttpPost("{id:int}/adjust-stock")]
	public async Task<ActionResult<ProductVariantDto>> AdjustStock(int id, [FromBody] AdjustProductVariantStockDto dto)
	{
		var variant = await _productVariantService.AdjustStockAsync(id, dto);

		if (variant == null)
		{
			return BadRequest("Variant does not exist, quantity change cannot be zero, or stock quantity cannot be negative.");
		}

		return Ok(variant);
	}

	// GET: /api/ProductVariants/{id}/stock-movements
	[HttpGet("{id:int}/stock-movements")]
	public async Task<ActionResult<PagedResultDto<StockMovementDto>>> GetStockMovements(
		int id,
		[FromQuery] PagedRequestDto input)
	{
		var movements = await _productVariantService.GetStockMovementsAsync(id, input);

		return Ok(movements);
	}

	// GET: /api/ProductVariants/available-colors/{productId}
	[HttpGet("available-colors/{productId:int}")]
	public async Task<ActionResult<List<string>>> GetAvailableColors(int productId)
	{
		var colors = await _productVariantService.GetAvailableColorsAsync(productId);

		return Ok(colors);
	}

	// GET: /api/ProductVariants/available-sizes/{productId}
	[HttpGet("available-sizes/{productId:int}")]
	public async Task<ActionResult<List<string>>> GetAvailableSizes(int productId)
	{
		var sizes = await _productVariantService.GetAvailableSizesAsync(productId);

		return Ok(sizes);
	}

	// GET: /api/ProductVariants/available-colors-by-size/{productId}/{size}
	[HttpGet("available-colors-by-size/{productId:int}/{size}")]
	public async Task<ActionResult<List<string>>> GetAvailableColorsBySize(int productId, string size)
	{
		var colors = await _productVariantService.GetAvailableColorsBySizeAsync(productId, size);

		return Ok(colors);
	}

	// GET: /api/ProductVariants/available-sizes-by-color/{productId}/{color}
	[HttpGet("available-sizes-by-color/{productId:int}/{color}")]
	public async Task<ActionResult<List<string>>> GetAvailableSizesByColor(int productId, string color)
	{
		var sizes = await _productVariantService.GetAvailableSizesByColorAsync(productId, color);

		return Ok(sizes);
	}

	// DELETE: /api/ProductVariants/{id}
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

	// GET: /api/ProductVariants/deleted
	[HttpGet("deleted")]
	public async Task<ActionResult<PagedResultDto<ProductVariantDto>>> GetDeleted([FromQuery] PagedRequestDto input)
	{
		var variants = await _productVariantService.GetDeletedAsync(input);

		return Ok(variants);
	}

	// POST: /api/ProductVariants/{id}/restore
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
}