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

	public async Task<PagedResultDto<CategoryDto>> GetAllAsync(PagedRequestDto input)
	{
		var query = _context.Categories.AsQueryable();

		if (!string.IsNullOrWhiteSpace(input.SearchTerm))
		{
			var searchTerm = input.SearchTerm.Trim();

			query = query.Where(category =>
				category.Name.Contains(searchTerm) ||
				(category.Description != null && category.Description.Contains(searchTerm)));
		}

		query = ApplySorting(query, input.Sorting);

		var totalCount = await query.CountAsync();

		var pagedQuery = ApplyPaging(query, input);

		var items = await pagedQuery
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

		return new PagedResultDto<CategoryDto>
		{
			Items = items,
			TotalCount = totalCount
		};
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

	public async Task<PagedResultDto<CategoryDto>> GetDeletedAsync(PagedRequestDto input)
	{
		var query = _context.Categories
			.IgnoreQueryFilters()
			.Where(category => category.IsDeleted);

		if (!string.IsNullOrWhiteSpace(input.SearchTerm))
		{
			var searchTerm = input.SearchTerm.Trim();

			query = query.Where(category =>
				category.Name.Contains(searchTerm) ||
				(category.Description != null && category.Description.Contains(searchTerm)));
		}

		query = ApplySorting(query, input.Sorting);

		var totalCount = await query.CountAsync();

		var pagedQuery = ApplyPaging(query, input);

		var items = await pagedQuery
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

		return new PagedResultDto<CategoryDto>
		{
			Items = items,
			TotalCount = totalCount
		};
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

	private static IQueryable<Category> ApplyPaging(IQueryable<Category> query, PagedRequestDto input)
	{
		if (input.SkipCount.HasValue && input.SkipCount.Value > 0)
		{
			query = query.Skip(input.SkipCount.Value);
		}

		if (input.MaxResultCount.HasValue)
		{
			query = query.Take(input.MaxResultCount.Value);
		}

		return query;
	}

	private static IQueryable<Category> ApplySorting(IQueryable<Category> query, string? sorting)
	{
		if (string.IsNullOrWhiteSpace(sorting))
		{
			return query.OrderBy(category => category.CreatedAt);
		}

		return sorting.Trim().ToLower() switch
		{
			"name" or "name asc" => query.OrderBy(category => category.Name),
			"name desc" => query.OrderByDescending(category => category.Name),

			"createdat" or "createdat asc" => query.OrderBy(category => category.CreatedAt),
			"createdat desc" => query.OrderByDescending(category => category.CreatedAt),

			"isactive" or "isactive asc" => query.OrderBy(category => category.IsActive),
			"isactive desc" => query.OrderByDescending(category => category.IsActive),

			_ => query.OrderBy(category => category.CreatedAt)
		};
	}
}