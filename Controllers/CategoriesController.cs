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

	[HttpGet]
	public async Task<ActionResult<List<CategoryDto>>> GetAll()
	{
		var categories = await _categoryService.GetAllAsync();

		return Ok(categories);
	}

	[HttpGet("deleted")]
	public async Task<ActionResult<List<CategoryDto>>> GetDeleted()
	{
		var categories = await _categoryService.GetDeletedAsync();

		return Ok(categories);
	}

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

	[HttpPost]
	public async Task<ActionResult<CategoryDto>> Create(CreateCategoryDto dto)
	{
		var category = await _categoryService.CreateAsync(dto);

		return Ok(category);
	}

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