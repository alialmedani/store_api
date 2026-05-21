using Microsoft.EntityFrameworkCore;
using Store.Data;
using Store.Dtos;
using Store.Models;

namespace Store.Services;

public class ProductVariantService : IProductVariantService
{
	private readonly AppDbContext _context;

	public ProductVariantService(AppDbContext context)
	{
		_context = context;
	}

	public async Task<PagedResultDto<ProductVariantDto>> GetByProductIdAsync(int productId, PagedRequestDto input)
	{
		var query = _context.ProductVariants
			.Where(variant => variant.ProductId == productId);

		if (!string.IsNullOrWhiteSpace(input.SearchTerm))
		{
			var searchTerm = input.SearchTerm.Trim();

			query = query.Where(variant =>
				variant.Color.Contains(searchTerm) ||
				variant.Size.Contains(searchTerm) ||
				variant.Product.Name.Contains(searchTerm));
		}

		query = ApplySorting(query, input.Sorting);

		var totalCount = await query.CountAsync();

		var pagedQuery = ApplyPaging(query, input);

		var items = await pagedQuery
			.Select(variant => new ProductVariantDto
			{
				Id = variant.Id,
				ProductId = variant.ProductId,
				ProductName = variant.Product.Name,
				Color = variant.Color,
				Size = variant.Size,
				StockQuantity = variant.StockQuantity,
				CreatedAt = variant.CreatedAt,
				UpdatedAt = variant.UpdatedAt
			})
			.ToListAsync();

		return new PagedResultDto<ProductVariantDto>
		{
			Items = items,
			TotalCount = totalCount
		};
	}

	public async Task<ProductVariantDto?> GetByIdAsync(int id)
	{
		var variant = await _context.ProductVariants
			.Where(variant => variant.Id == id)
			.Select(variant => new ProductVariantDto
			{
				Id = variant.Id,
				ProductId = variant.ProductId,
				ProductName = variant.Product.Name,
				Color = variant.Color,
				Size = variant.Size,
				StockQuantity = variant.StockQuantity,
				CreatedAt = variant.CreatedAt,
				UpdatedAt = variant.UpdatedAt
			})
			.FirstOrDefaultAsync();

		return variant;
	}

	public async Task<ProductVariantDto?> CreateAsync(CreateProductVariantDto dto)
	{
		var productExists = await _context.Products
			.AnyAsync(product => product.Id == dto.ProductId);

		if (!productExists)
		{
			return null;
		}

		var duplicateExists = await _context.ProductVariants
			.IgnoreQueryFilters()
			.AnyAsync(variant =>
				variant.ProductId == dto.ProductId &&
				variant.Color == dto.Color &&
				variant.Size == dto.Size);

		if (duplicateExists)
		{
			return null;
		}

		var variant = new ProductVariant
		{
			ProductId = dto.ProductId,
			Color = dto.Color,
			Size = dto.Size,
			StockQuantity = dto.StockQuantity
		};

		_context.ProductVariants.Add(variant);

		await _context.SaveChangesAsync();

		return await GetByIdAsync(variant.Id);
	}

	public async Task<ProductVariantDto?> UpdateAsync(int id, UpdateProductVariantDto dto)
	{
		var variant = await _context.ProductVariants
			.FirstOrDefaultAsync(variant => variant.Id == id);

		if (variant == null)
		{
			return null;
		}

		var duplicateExists = await _context.ProductVariants
			.IgnoreQueryFilters()
			.AnyAsync(otherVariant =>
				otherVariant.Id != id &&
				otherVariant.ProductId == variant.ProductId &&
				otherVariant.Color == dto.Color &&
				otherVariant.Size == dto.Size);

		if (duplicateExists)
		{
			return null;
		}

		variant.Color = dto.Color;
		variant.Size = dto.Size;
		variant.StockQuantity = dto.StockQuantity;
		variant.UpdatedAt = DateTime.UtcNow;

		await _context.SaveChangesAsync();

		return await GetByIdAsync(variant.Id);
	}

	public async Task<bool> DeleteAsync(int id)
	{
		var variant = await _context.ProductVariants
			.FirstOrDefaultAsync(variant => variant.Id == id);

		if (variant == null)
		{
			return false;
		}

		variant.IsDeleted = true;
		variant.DeletedAt = DateTime.UtcNow;
		variant.UpdatedAt = DateTime.UtcNow;

		await _context.SaveChangesAsync();

		return true;
	}

	public async Task<bool> RestoreAsync(int id)
	{
		var variant = await _context.ProductVariants
			.IgnoreQueryFilters()
			.FirstOrDefaultAsync(variant => variant.Id == id && variant.IsDeleted);

		if (variant == null)
		{
			return false;
		}

		variant.IsDeleted = false;
		variant.DeletedAt = null;
		variant.UpdatedAt = DateTime.UtcNow;

		await _context.SaveChangesAsync();

		return true;
	}

	public async Task<PagedResultDto<ProductVariantDto>> GetDeletedAsync(PagedRequestDto input)
	{
		var query = _context.ProductVariants
			.IgnoreQueryFilters()
			.Where(variant => variant.IsDeleted);

		if (!string.IsNullOrWhiteSpace(input.SearchTerm))
		{
			var searchTerm = input.SearchTerm.Trim();

			query = query.Where(variant =>
				variant.Color.Contains(searchTerm) ||
				variant.Size.Contains(searchTerm) ||
				variant.Product.Name.Contains(searchTerm));
		}

		query = ApplySorting(query, input.Sorting);

		var totalCount = await query.CountAsync();

		var pagedQuery = ApplyPaging(query, input);

		var items = await pagedQuery
			.Select(variant => new ProductVariantDto
			{
				Id = variant.Id,
				ProductId = variant.ProductId,
				ProductName = variant.Product.Name,
				Color = variant.Color,
				Size = variant.Size,
				StockQuantity = variant.StockQuantity,
				CreatedAt = variant.CreatedAt,
				UpdatedAt = variant.UpdatedAt
			})
			.ToListAsync();

		return new PagedResultDto<ProductVariantDto>
		{
			Items = items,
			TotalCount = totalCount
		};
	}

	public async Task<List<string>> GetAvailableColorsAsync(int productId)
	{
		var colors = await _context.ProductVariants
			.Where(variant =>
				variant.ProductId == productId &&
				variant.StockQuantity > 0)
			.Select(variant => variant.Color)
			.Distinct()
			.ToListAsync();

		return colors;
	}

	public async Task<List<string>> GetAvailableSizesAsync(int productId)
	{
		var sizes = await _context.ProductVariants
			.Where(variant =>
				variant.ProductId == productId &&
				variant.StockQuantity > 0)
			.Select(variant => variant.Size)
			.Distinct()
			.ToListAsync();

		return sizes;
	}

	public async Task<List<string>> GetAvailableColorsBySizeAsync(int productId, string size)
	{
		var colors = await _context.ProductVariants
			.Where(variant =>
				variant.ProductId == productId &&
				variant.Size == size &&
				variant.StockQuantity > 0)
			.Select(variant => variant.Color)
			.Distinct()
			.ToListAsync();

		return colors;
	}

	public async Task<List<string>> GetAvailableSizesByColorAsync(int productId, string color)
	{
		var sizes = await _context.ProductVariants
			.Where(variant =>
				variant.ProductId == productId &&
				variant.Color == color &&
				variant.StockQuantity > 0)
			.Select(variant => variant.Size)
			.Distinct()
			.ToListAsync();

		return sizes;
	}

	private static IQueryable<ProductVariant> ApplyPaging(IQueryable<ProductVariant> query, PagedRequestDto input)
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

	private static IQueryable<ProductVariant> ApplySorting(IQueryable<ProductVariant> query, string? sorting)
	{
		if (string.IsNullOrWhiteSpace(sorting))
		{
			return query.OrderBy(variant => variant.CreatedAt);
		}

		return sorting.Trim().ToLower() switch
		{
			"color" or "color asc" => query.OrderBy(variant => variant.Color),
			"color desc" => query.OrderByDescending(variant => variant.Color),

			"size" or "size asc" => query.OrderBy(variant => variant.Size),
			"size desc" => query.OrderByDescending(variant => variant.Size),

			"stockquantity" or "stockquantity asc" => query.OrderBy(variant => variant.StockQuantity),
			"stockquantity desc" => query.OrderByDescending(variant => variant.StockQuantity),

			"createdat" or "createdat asc" => query.OrderBy(variant => variant.CreatedAt),
			"createdat desc" => query.OrderByDescending(variant => variant.CreatedAt),

			_ => query.OrderBy(variant => variant.CreatedAt)
		};
	}
}