using Microsoft.EntityFrameworkCore;
using Store.Data;
using Store.Dtos;
using Store.Models;

namespace Store.Services;

public class ProductService : IProductService
{
	private readonly AppDbContext _context;

	public ProductService(AppDbContext context)
	{
		_context = context;
	}

	public async Task<PagedResultDto<ProductDto>> GetAllAsync(PagedRequestDto input)
	{
		var query = _context.Products.AsQueryable();

		if (!string.IsNullOrWhiteSpace(input.SearchTerm))
		{
			var searchTerm = input.SearchTerm.Trim();

			query = query.Where(product =>
				product.Name.Contains(searchTerm) ||
				(product.Description != null && product.Description.Contains(searchTerm)) ||
				product.Category.Name.Contains(searchTerm));
		}

		query = ApplySorting(query, input.Sorting);

		var totalCount = await query.CountAsync();

		var pagedQuery = ApplyPaging(query, input);

		var items = await pagedQuery
			.Select(product => new ProductDto
			{
				Id = product.Id,
				Name = product.Name,
				Description = product.Description,
				Price = product.Price,
				TotalStockQuantity = product.Variants
					.Where(variant => !variant.IsDeleted)
					.Sum(variant => variant.StockQuantity),
				IsActive = product.IsActive,

				Category = new LookupDto
				{
					Id = product.CategoryId,
					Name = product.Category.Name
				},

				TargetAudience = new LookupDto
				{
					Id = (int)product.TargetAudience,
					Name = product.TargetAudience.ToString()
				},

				CreatedAt = product.CreatedAt,
				UpdatedAt = product.UpdatedAt
			})
			.ToListAsync();

		return new PagedResultDto<ProductDto>
		{
			Items = items,
			TotalCount = totalCount
		};
	}
	public async Task<ProductDetailsDto?> GetByIdAsync(int id)
	{
		var product = await _context.Products
			.Where(product => product.Id == id)
			.Select(product => new ProductDetailsDto
			{
				Id = product.Id,
				Name = product.Name,
				Description = product.Description,
				Price = product.Price,
				TotalStockQuantity = product.Variants
					.Where(variant => !variant.IsDeleted)
					.Sum(variant => variant.StockQuantity),
				IsActive = product.IsActive,

				Category = new LookupDto
				{
					Id = product.CategoryId,
					Name = product.Category.Name
				},

				TargetAudience = new LookupDto
				{
					Id = (int)product.TargetAudience,
					Name = product.TargetAudience.ToString()
				},

				CreatedAt = product.CreatedAt,
				UpdatedAt = product.UpdatedAt,

				Variants = product.Variants
					.Where(variant => !variant.IsDeleted)
					.Select(variant => new ProductVariantSummaryDto
					{
						Id = variant.Id,
						Color = variant.Color,
						Size = variant.Size,
						StockQuantity = variant.StockQuantity
					})
					.ToList()
			})
			.FirstOrDefaultAsync();

		return product;
	}
	public async Task<ProductDto?> CreateAsync(CreateProductDto dto)
	{
		var categoryExists = await _context.Categories
			.AnyAsync(category => category.Id == dto.CategoryId);

		if (!categoryExists)
		{
			return null;
		}

		var product = new Product
		{
			Name = dto.Name,
			Description = dto.Description,
			Price = dto.Price,
			IsActive = dto.IsActive,
			TargetAudience = dto.TargetAudience,
			CategoryId = dto.CategoryId
		};

		_context.Products.Add(product);

		await _context.SaveChangesAsync();

		return await GetByIdAsync(product.Id);
	}

	public async Task<ProductDto?> UpdateAsync(int id, UpdateProductDto dto)
	{
		var product = await _context.Products
			.FirstOrDefaultAsync(product => product.Id == id);

		if (product == null)
		{
			return null;
		}

		var categoryExists = await _context.Categories
			.AnyAsync(category => category.Id == dto.CategoryId);

		if (!categoryExists)
		{
			return null;
		}

		product.Name = dto.Name;
		product.Description = dto.Description;
		product.Price = dto.Price;
		product.IsActive = dto.IsActive;
		product.TargetAudience = dto.TargetAudience;
		product.CategoryId = dto.CategoryId;
		product.UpdatedAt = DateTime.UtcNow;

		await _context.SaveChangesAsync();

		return await GetByIdAsync(product.Id);
	}

	public async Task<bool> DeleteAsync(int id)
	{
		var product = await _context.Products
			.FirstOrDefaultAsync(product => product.Id == id);

		if (product == null)
		{
			return false;
		}

		product.IsDeleted = true;
		product.DeletedAt = DateTime.UtcNow;
		product.UpdatedAt = DateTime.UtcNow;

		await _context.SaveChangesAsync();

		return true;
	}

	public async Task<bool> RestoreAsync(int id)
	{
		var product = await _context.Products
			.IgnoreQueryFilters()
			.FirstOrDefaultAsync(product => product.Id == id && product.IsDeleted);

		if (product == null)
		{
			return false;
		}

		product.IsDeleted = false;
		product.DeletedAt = null;
		product.UpdatedAt = DateTime.UtcNow;

		await _context.SaveChangesAsync();

		return true;
	}

	public async Task<PagedResultDto<ProductDto>> GetDeletedAsync(PagedRequestDto input)
	{
		var query = _context.Products
			.IgnoreQueryFilters()
			.Where(product => product.IsDeleted);

		if (!string.IsNullOrWhiteSpace(input.SearchTerm))
		{
			var searchTerm = input.SearchTerm.Trim();

			query = query.Where(product =>
				product.Name.Contains(searchTerm) ||
				(product.Description != null && product.Description.Contains(searchTerm)) ||
				product.Category.Name.Contains(searchTerm));
		}

		query = ApplySorting(query, input.Sorting);

		var totalCount = await query.CountAsync();

		var pagedQuery = ApplyPaging(query, input);

		var items = await pagedQuery
			.Select(product => new ProductDto
			{
				Id = product.Id,
				Name = product.Name,
				Description = product.Description,
				Price = product.Price,
				TotalStockQuantity = product.Variants
					.Where(variant => !variant.IsDeleted)
					.Sum(variant => variant.StockQuantity),
				IsActive = product.IsActive,

				Category = new LookupDto
				{
					Id = product.CategoryId,
					Name = product.Category.Name
				},

				TargetAudience = new LookupDto
				{
					Id = (int)product.TargetAudience,
					Name = product.TargetAudience.ToString()
				},

				CreatedAt = product.CreatedAt,
				UpdatedAt = product.UpdatedAt
			})
			.ToListAsync();

		return new PagedResultDto<ProductDto>
		{
			Items = items,
			TotalCount = totalCount
		};
	}
 


	private static IQueryable<Product> ApplyPaging(IQueryable<Product> query, PagedRequestDto input)
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

	private static IQueryable<Product> ApplySorting(IQueryable<Product> query, string? sorting)
	{
		if (string.IsNullOrWhiteSpace(sorting))
		{
			return query.OrderBy(product => product.CreatedAt);
		}

		return sorting.Trim().ToLower() switch
		{
			"name" or "name asc" => query.OrderBy(product => product.Name),
			"name desc" => query.OrderByDescending(product => product.Name),

			"price" or "price asc" => query.OrderBy(product => product.Price),
			"price desc" => query.OrderByDescending(product => product.Price),

			"createdat" or "createdat asc" => query.OrderBy(product => product.CreatedAt),
			"createdat desc" => query.OrderByDescending(product => product.CreatedAt),

			_ => query.OrderBy(product => product.CreatedAt)
		};
	}
}