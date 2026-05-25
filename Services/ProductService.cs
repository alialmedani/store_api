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
		var query = _context.Products
			.AsNoTracking()
			.AsQueryable();

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
			.AsNoTracking()
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
				AvailableColors = product.Variants
					.Where(variant => !variant.IsDeleted && variant.StockQuantity > 0)
					.Select(variant => variant.Color)
					.Distinct()
					.OrderBy(color => color)
					.ToList(),
				AvailableSizes = product.Variants
					.Where(variant => !variant.IsDeleted && variant.StockQuantity > 0)
					.Select(variant => variant.Size)
					.Distinct()
					.OrderBy(size => size)
					.ToList(),
				Variants = product.Variants
					.Where(variant => !variant.IsDeleted)
					.OrderBy(variant => variant.Color)
					.ThenBy(variant => variant.Size)
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

	public async Task<ServiceResult<ProductDto>> CreateAsync(CreateProductDto dto)
	{
		var categoryExists = await _context.Categories
			.AnyAsync(category => category.Id == dto.CategoryId);

		if (!categoryExists)
		{
			return ServiceResult<ProductDto>.Failure("Category does not exist.");
		}

		var name = dto.Name.Trim();
		var nameKey = name.ToLower();

		var duplicateExists = await _context.Products
			.IgnoreQueryFilters()
			.AnyAsync(product =>
				!product.IsDeleted &&
				product.CategoryId == dto.CategoryId &&
				product.Name.ToLower() == nameKey);

		if (duplicateExists)
		{
			return ServiceResult<ProductDto>.Failure("Product name already exists in this category.");
		}

		var product = new Product
		{
			Name = name,
			Description = dto.Description,
			Price = dto.Price,
			IsActive = dto.IsActive,
			TargetAudience = dto.TargetAudience,
			CategoryId = dto.CategoryId
		};

		_context.Products.Add(product);

		await _context.SaveChangesAsync();

		var createdProduct = await GetDtoByIdAsync(product.Id);

		if (createdProduct == null)
		{
			return ServiceResult<ProductDto>.Failure("Product was created but could not be loaded.");
		}

		return ServiceResult<ProductDto>.Success(createdProduct);
	}

	public async Task<ServiceResult<ProductDto>> UpdateAsync(int id, UpdateProductDto dto)
	{
		var product = await _context.Products
			.FirstOrDefaultAsync(product => product.Id == id);

		if (product == null)
		{
			return ServiceResult<ProductDto>.Failure("Product does not exist.");
		}

		var categoryExists = await _context.Categories
			.AnyAsync(category => category.Id == dto.CategoryId);

		if (!categoryExists)
		{
			return ServiceResult<ProductDto>.Failure("Category does not exist.");
		}

		var name = dto.Name.Trim();
		var nameKey = name.ToLower();

		var duplicateExists = await _context.Products
			.IgnoreQueryFilters()
			.AnyAsync(otherProduct =>
				otherProduct.Id != id &&
				!otherProduct.IsDeleted &&
				otherProduct.CategoryId == dto.CategoryId &&
				otherProduct.Name.ToLower() == nameKey);

		if (duplicateExists)
		{
			return ServiceResult<ProductDto>.Failure("Product name already exists in this category.");
		}

		if (product.CategoryId != dto.CategoryId)
		{
			var hasVariants = await _context.ProductVariants
				.IgnoreQueryFilters()
				.AnyAsync(variant => variant.ProductId == product.Id);

			if (hasVariants)
			{
				return ServiceResult<ProductDto>.Failure(
					"Cannot change product category because this product already has variants.");
			}
		}

		product.Name = name;
		product.Description = dto.Description;
		product.Price = dto.Price;
		product.IsActive = dto.IsActive;
		product.TargetAudience = dto.TargetAudience;
		product.CategoryId = dto.CategoryId;
		product.UpdatedAt = DateTime.UtcNow;

		await _context.SaveChangesAsync();

		var updatedProduct = await GetDtoByIdAsync(product.Id);

		if (updatedProduct == null)
		{
			return ServiceResult<ProductDto>.Failure("Product was updated but could not be loaded.");
		}

		return ServiceResult<ProductDto>.Success(updatedProduct);
	}

	public async Task<ServiceResult<bool>> DeleteAsync(int id)
	{
		var product = await _context.Products
			.FirstOrDefaultAsync(product => product.Id == id);

		if (product == null)
		{
			return ServiceResult<bool>.Failure("Product does not exist.");
		}

		product.IsDeleted = true;
		product.DeletedAt = DateTime.UtcNow;
		product.UpdatedAt = DateTime.UtcNow;

		await _context.SaveChangesAsync();

		return ServiceResult<bool>.Success(true);
	}
	public async Task<ServiceResult<bool>> RestoreAsync(int id)
	{
		var product = await _context.Products
			.IgnoreQueryFilters()
			.FirstOrDefaultAsync(product => product.Id == id && product.IsDeleted);

		if (product == null)
		{
			return ServiceResult<bool>.Failure("Product does not exist or is not deleted.");
		}

		var categoryExists = await _context.Categories
			.AnyAsync(category => category.Id == product.CategoryId);

		if (!categoryExists)
		{
			return ServiceResult<bool>.Failure("Cannot restore product because its category does not exist or is deleted.");
		}

		var nameKey = product.Name.Trim().ToLower();

		var duplicateExists = await _context.Products
			.IgnoreQueryFilters()
			.AnyAsync(otherProduct =>
				otherProduct.Id != product.Id &&
				!otherProduct.IsDeleted &&
				otherProduct.CategoryId == product.CategoryId &&
				otherProduct.Name.ToLower() == nameKey);

		if (duplicateExists)
		{
			return ServiceResult<bool>.Failure(
				"Cannot restore product because another active product with the same name exists in this category.");
		}

		product.IsDeleted = false;
		product.DeletedAt = null;
		product.UpdatedAt = DateTime.UtcNow;

		await _context.SaveChangesAsync();

		return ServiceResult<bool>.Success(true);
	}

	public async Task<PagedResultDto<ProductDto>> GetDeletedAsync(PagedRequestDto input)
	{
		var query = _context.Products
			.IgnoreQueryFilters()
			.AsNoTracking()
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

	private async Task<ProductDto?> GetDtoByIdAsync(int id)
	{
		var product = await _context.Products
			.AsNoTracking()
			.Where(product => product.Id == id)
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
			.FirstOrDefaultAsync();

		return product;
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