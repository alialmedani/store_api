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
	[ProducesResponseType(typeof(PagedResultDto<CategoryDto>), StatusCodes.Status200OK)]
	public async Task<ActionResult<PagedResultDto<CategoryDto>>> GetAll([FromQuery] PagedRequestDto input)
	{
		var categories = await _categoryService.GetAllAsync(input);

		return Ok(categories);
	}

	// GET: /api/Categories/{id}
	[HttpGet("{id:int}")]
	[ProducesResponseType(typeof(CategoryDto), StatusCodes.Status200OK)]
	[ProducesResponseType(StatusCodes.Status404NotFound)]
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
	[ProducesResponseType(typeof(CategoryDto), StatusCodes.Status201Created)]
	[ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
	public async Task<ActionResult<CategoryDto>> Create([FromBody] CreateCategoryDto dto)
	{
		var result = await _categoryService.CreateAsync(dto);

		if (!result.IsSuccess)
		{
			return BadRequest(result.ErrorMessage);
		}

		if (result.Data == null)
		{
			return BadRequest("Category was created but could not be loaded.");
		}

		return CreatedAtAction(
			nameof(GetById),
			new { id = result.Data.Id },
			result.Data);
	}

	// PUT: /api/Categories/{id}
	[HttpPut("{id:int}")]
	[ProducesResponseType(typeof(CategoryDto), StatusCodes.Status200OK)]
	[ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
	[ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
	public async Task<ActionResult<CategoryDto>> Update(int id, [FromBody] UpdateCategoryDto dto)
	{
		var result = await _categoryService.UpdateAsync(id, dto);

		if (!result.IsSuccess)
		{
			if (result.ErrorMessage == "Category does not exist.")
			{
				return NotFound(result.ErrorMessage);
			}

			return BadRequest(result.ErrorMessage);
		}

		return Ok(result.Data);
	}

	// DELETE: /api/Categories/{id}
	[HttpDelete("{id:int}")]
	[ProducesResponseType(StatusCodes.Status204NoContent)]
	[ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
	[ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
	public async Task<ActionResult> Delete(int id)
	{
		var result = await _categoryService.DeleteAsync(id);

		if (!result.IsSuccess)
		{
			if (result.ErrorMessage == "Category does not exist.")
			{
				return NotFound(result.ErrorMessage);
			}

			return BadRequest(result.ErrorMessage);
		}

		return NoContent();
	}

	// GET: /api/Categories/deleted
	[HttpGet("deleted")]
	[ProducesResponseType(typeof(PagedResultDto<CategoryDto>), StatusCodes.Status200OK)]
	public async Task<ActionResult<PagedResultDto<CategoryDto>>> GetDeleted([FromQuery] PagedRequestDto input)
	{
		var categories = await _categoryService.GetDeletedAsync(input);

		return Ok(categories);
	}

	// POST: /api/Categories/{id}/restore
	[HttpPost("{id:int}/restore")]
	[ProducesResponseType(StatusCodes.Status204NoContent)]
	[ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
	[ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
	public async Task<ActionResult> Restore(int id)
	{
		var result = await _categoryService.RestoreAsync(id);

		if (!result.IsSuccess)
		{
			if (result.ErrorMessage == "Category does not exist or is not deleted.")
			{
				return NotFound(result.ErrorMessage);
			}

			return BadRequest(result.ErrorMessage);
		}

		return NoContent();
	}
}