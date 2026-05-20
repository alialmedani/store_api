using Microsoft.EntityFrameworkCore;
using Store.Data;
using Store.Dtos;
using Store.Models;

namespace Store.Services;

public class CategoryService : ICategoryService
{
	private readonly AppDbContext _context;

	public CategoryService(AppDbContext context)
	{
		_context = context;
	}

	public async Task<List<CategoryDto>> GetAllAsync()
	{
		var categories = await _context.Categories
			.Select(category => new CategoryDto
			{
				Id = category.Id,
				Name = category.Name,
				Description = category.Description,
				IsActive = category.IsActive,
				CreatedAt = category.CreatedAt,
				UpdatedAt = category.UpdatedAt
			})
			.ToListAsync();

		return categories;
	}

	public async Task<CategoryDto?> GetByIdAsync(int id)
	{
		var category = await _context.Categories
			.Where(category => category.Id == id)
			.Select(category => new CategoryDto
			{
				Id = category.Id,
				Name = category.Name,
				Description = category.Description,
				IsActive = category.IsActive,
				CreatedAt = category.CreatedAt,
				UpdatedAt = category.UpdatedAt
			})
			.FirstOrDefaultAsync();

		return category;
	}

	public async Task<CategoryDto> CreateAsync(CreateCategoryDto dto)
	{
		var category = new Category
		{
			Name = dto.Name,
			Description = dto.Description,
			IsActive = dto.IsActive
		};

		_context.Categories.Add(category);

		await _context.SaveChangesAsync();

		return MapToDto(category);
	}

	public async Task<CategoryDto?> UpdateAsync(int id, UpdateCategoryDto dto)
	{
		var category = await _context.Categories
			.FirstOrDefaultAsync(category => category.Id == id);

		if (category == null)
		{
			return null;
		}

		category.Name = dto.Name;
		category.Description = dto.Description;
		category.IsActive = dto.IsActive;
		category.UpdatedAt = DateTime.UtcNow;

		await _context.SaveChangesAsync();

		return MapToDto(category);
	}

	public async Task<bool> DeleteAsync(int id)
	{
		var category = await _context.Categories
			.FirstOrDefaultAsync(category => category.Id == id);

		if (category == null)
		{
			return false;
		}

		category.IsDeleted = true;
		category.DeletedAt = DateTime.UtcNow;
		category.UpdatedAt = DateTime.UtcNow;

		await _context.SaveChangesAsync();

		return true;
	}

	public async Task<bool> RestoreAsync(int id)
	{
		var category = await _context.Categories
			.IgnoreQueryFilters()
			.FirstOrDefaultAsync(category => category.Id == id && category.IsDeleted);

		if (category == null)
		{
			return false;
		}

		category.IsDeleted = false;
		category.DeletedAt = null;
		category.UpdatedAt = DateTime.UtcNow;

		await _context.SaveChangesAsync();

		return true;
	}

	public async Task<List<CategoryDto>> GetDeletedAsync()
	{
		var categories = await _context.Categories
			.IgnoreQueryFilters()
			.Where(category => category.IsDeleted)
			.Select(category => new CategoryDto
			{
				Id = category.Id,
				Name = category.Name,
				Description = category.Description,
				IsActive = category.IsActive,
				CreatedAt = category.CreatedAt,
				UpdatedAt = category.UpdatedAt
			})
			.ToListAsync();

		return categories;
	}

	private static CategoryDto MapToDto(Category category)
	{
		return new CategoryDto
		{
			Id = category.Id,
			Name = category.Name,
			Description = category.Description,
			IsActive = category.IsActive,
			CreatedAt = category.CreatedAt,
			UpdatedAt = category.UpdatedAt
		};
	}
}