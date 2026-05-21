using Microsoft.AspNetCore.Mvc;
using Store.Dtos;
using Store.Services;

namespace Store.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CategoriesController : ControllerBase
{
	private readonly ICategoryService _categoryService;

	public CategoriesController(ICategoryService categoryService)
	{
		_categoryService = categoryService;
	}

	// GET: /api/Categories
	[HttpGet]
	public async Task<ActionResult<PagedResultDto<CategoryDto>>> GetAll([FromQuery] PagedRequestDto input)
	{
		var categories = await _categoryService.GetAllAsync(input);

		return Ok(categories);
	}

	// GET: /api/Categories/{id}
	[HttpGet("{id:int}")]
	public async Task<ActionResult<CategoryDto>> GetById(int id)
	{
		var category = await _categoryService.GetByIdAsync(id);

		if (category == null)
		{
			return NotFound();
		}

		return Ok(category);
	}

	// POST: /api/Categories
	[HttpPost]
	public async Task<ActionResult<CategoryDto>> Create(CreateCategoryDto dto)
	{
		var category = await _categoryService.CreateAsync(dto);

		return Ok(category);
	}

	// PUT: /api/Categories/{id}
	[HttpPut("{id:int}")]
	public async Task<ActionResult<CategoryDto>> Update(int id, UpdateCategoryDto dto)
	{
		var category = await _categoryService.UpdateAsync(id, dto);

		if (category == null)
		{
			return NotFound();
		}

		return Ok(category);
	}

	// DELETE: /api/Categories/{id}
	[HttpDelete("{id:int}")]
	public async Task<ActionResult> Delete(int id)
	{
		var deleted = await _categoryService.DeleteAsync(id);

		if (!deleted)
		{
			return NotFound();
		}

		return NoContent();
	}

	// GET: /api/Categories/deleted
	[HttpGet("deleted")]
	public async Task<ActionResult<PagedResultDto<CategoryDto>>> GetDeleted([FromQuery] PagedRequestDto input)
	{
		var categories = await _categoryService.GetDeletedAsync(input);

		return Ok(categories);
	}

	// POST: /api/Categories/{id}/restore
	[HttpPost("{id:int}/restore")]
	public async Task<ActionResult> Restore(int id)
	{
		var restored = await _categoryService.RestoreAsync(id);

		if (!restored)
		{
			return NotFound();
		}

		return NoContent();
	}
}