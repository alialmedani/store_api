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
				SizeType = new LookupDto
				{
					Id = (int)category.SizeType,
					Name = category.SizeType.ToString()
				},
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
				SizeType = new LookupDto
				{
					Id = (int)category.SizeType,
					Name = category.SizeType.ToString()
				},
				CreatedAt = category.CreatedAt,
				UpdatedAt = category.UpdatedAt
			})
			.FirstOrDefaultAsync();

		return category;
	}

	public async Task<ServiceResult<CategoryDto>> CreateAsync(CreateCategoryDto dto)
	{
		var name = dto.Name.Trim();
		var nameKey = name.ToLower();

		var duplicateExists = await _context.Categories
			.IgnoreQueryFilters()
			.AnyAsync(category =>
				!category.IsDeleted &&
				category.Name.ToLower() == nameKey);

		if (duplicateExists)
		{
			return ServiceResult<CategoryDto>.Failure("Category name already exists.");
		}

		var category = new Category
		{
			Name = name,
			Description = dto.Description,
			IsActive = dto.IsActive,
			SizeType = dto.SizeType
		};

		_context.Categories.Add(category);

		await _context.SaveChangesAsync();

		return ServiceResult<CategoryDto>.Success(MapToDto(category));
	}

	public async Task<ServiceResult<CategoryDto>> UpdateAsync(int id, UpdateCategoryDto dto)
	{
		var category = await _context.Categories
			.FirstOrDefaultAsync(category => category.Id == id);

		if (category == null)
		{
			return ServiceResult<CategoryDto>.Failure("Category does not exist.");
		}

		var name = dto.Name.Trim();
		var nameKey = name.ToLower();

		var duplicateExists = await _context.Categories
			.IgnoreQueryFilters()
			.AnyAsync(otherCategory =>
				otherCategory.Id != id &&
				!otherCategory.IsDeleted &&
				otherCategory.Name.ToLower() == nameKey);

		if (duplicateExists)
		{
			return ServiceResult<CategoryDto>.Failure("Category name already exists.");
		}

		if (category.SizeType != dto.SizeType)
		{
			var hasProducts = await _context.Products
				.IgnoreQueryFilters()
				.AnyAsync(product => product.CategoryId == category.Id);

			if (hasProducts)
			{
				return ServiceResult<CategoryDto>.Failure(
					"Cannot change category size type because this category is already used by products.");
			}
		}

		category.Name = name;
		category.Description = dto.Description;
		category.IsActive = dto.IsActive;
		category.SizeType = dto.SizeType;
		category.UpdatedAt = DateTime.UtcNow;

		await _context.SaveChangesAsync();

		return ServiceResult<CategoryDto>.Success(MapToDto(category));
	}
	public async Task<ServiceResult<bool>> DeleteAsync(int id)
	{
		var category = await _context.Categories
			.FirstOrDefaultAsync(category => category.Id == id);

		if (category == null)
		{
			return ServiceResult<bool>.Failure("Category does not exist.");
		}

		var hasProducts = await _context.Products
			.IgnoreQueryFilters()
			.AnyAsync(product => product.CategoryId == category.Id);

		if (hasProducts)
		{
			return ServiceResult<bool>.Failure(
				"Cannot delete category because it is already used by products.");
		}

		category.IsDeleted = true;
		category.DeletedAt = DateTime.UtcNow;
		category.UpdatedAt = DateTime.UtcNow;

		await _context.SaveChangesAsync();

		return ServiceResult<bool>.Success(true);
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
				SizeType = new LookupDto
				{
					Id = (int)category.SizeType,
					Name = category.SizeType.ToString()
				},
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
			SizeType = new LookupDto
			{
				Id = (int)category.SizeType,
				Name = category.SizeType.ToString()
			},
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

			"sizetype" or "sizetype asc" => query.OrderBy(category => category.SizeType),
			"sizetype desc" => query.OrderByDescending(category => category.SizeType),

			_ => query.OrderBy(category => category.CreatedAt)
		};
	}
}