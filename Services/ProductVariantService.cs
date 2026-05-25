using Microsoft.EntityFrameworkCore;
using Store.Data;
using Store.Dtos;
using Store.Models;

namespace Store.Services;

public class ProductVariantService : IProductVariantService
{
	private const string NoSizeLabel = "No Size";
	private const string OneSizeLabel = "One Size";

	private static readonly HashSet<string> ClothingSizes = new(StringComparer.OrdinalIgnoreCase)
	{
		"XS",
		"S",
		"M",
		"L",
		"XL",
		"XXL",
		"XXXL",
		"2XL",
		"3XL",
		"4XL"
	};

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

	public async Task<ServiceResult<ProductVariantDto>> CreateAsync(CreateProductVariantDto dto)
	{
		var productInfo = await _context.Products
			.Where(product => product.Id == dto.ProductId)
			.Select(product => new
			{
				product.Id,
				SizeType = product.Category.SizeType
			})
			.FirstOrDefaultAsync();

		if (productInfo == null)
		{
			return ServiceResult<ProductVariantDto>.Failure("Product does not exist.");
		}

		var color = NormalizeText(dto.Color);

		if (string.IsNullOrWhiteSpace(color))
		{
			return ServiceResult<ProductVariantDto>.Failure("Color is required.");
		}

		var size = NormalizeSizeByCategory(dto.Size, productInfo.SizeType);

		if (string.IsNullOrWhiteSpace(size))
		{
			return ServiceResult<ProductVariantDto>.Failure(GetInvalidSizeMessage(productInfo.SizeType));
		}

		var colorKey = NormalizeKey(color);
		var sizeKey = NormalizeKey(size);

		var duplicateExists = await _context.ProductVariants
			.IgnoreQueryFilters()
			.AnyAsync(variant =>
				variant.ProductId == dto.ProductId &&
				variant.Color.ToLower() == colorKey &&
				variant.Size.ToLower() == sizeKey);

		if (duplicateExists)
		{
			return ServiceResult<ProductVariantDto>.Failure(
				"Variant already exists. Use adjust-stock to increase or decrease quantity.");
		}

		var variant = new ProductVariant
		{
			ProductId = dto.ProductId,
			Color = color,
			Size = size,
			StockQuantity = dto.StockQuantity
		};

		_context.ProductVariants.Add(variant);

		await _context.SaveChangesAsync();

		var createdVariant = await GetByIdAsync(variant.Id);

		if (createdVariant == null)
		{
			return ServiceResult<ProductVariantDto>.Failure("Variant was created but could not be loaded.");
		}

		return ServiceResult<ProductVariantDto>.Success(createdVariant);
	}

	public async Task<ServiceResult<List<ProductVariantDto>>> BulkCreateAsync(CreateBulkProductVariantsDto dto)
	{
		var productInfo = await _context.Products
			.Where(product => product.Id == dto.ProductId)
			.Select(product => new
			{
				product.Id,
				SizeType = product.Category.SizeType
			})
			.FirstOrDefaultAsync();

		if (productInfo == null)
		{
			return ServiceResult<List<ProductVariantDto>>.Failure("Product does not exist.");
		}

		var normalizedItems = dto.Variants
			.Select(variant =>
			{
				var color = NormalizeText(variant.Color);
				var size = NormalizeSizeByCategory(variant.Size, productInfo.SizeType);

				return new
				{
					Color = color,
					Size = size,
					ColorKey = NormalizeKey(color),
					SizeKey = size == null ? null : NormalizeKey(size),
					variant.StockQuantity
				};
			})
			.ToList();

		var hasInvalidColor = normalizedItems.Any(item => string.IsNullOrWhiteSpace(item.Color));

		if (hasInvalidColor)
		{
			return ServiceResult<List<ProductVariantDto>>.Failure("All variants must have a color.");
		}

		var hasInvalidSize = normalizedItems.Any(item => string.IsNullOrWhiteSpace(item.Size));

		if (hasInvalidSize)
		{
			return ServiceResult<List<ProductVariantDto>>.Failure(GetInvalidSizeMessage(productInfo.SizeType));
		}

		var hasDuplicateInRequest = normalizedItems
			.GroupBy(variant => new { variant.ColorKey, variant.SizeKey })
			.Any(group => group.Count() > 1);

		if (hasDuplicateInRequest)
		{
			return ServiceResult<List<ProductVariantDto>>.Failure(
				"Request contains duplicate variants. The same color and size cannot be repeated for the same product.");
		}

		var existingVariants = await _context.ProductVariants
			.IgnoreQueryFilters()
			.Where(variant => variant.ProductId == dto.ProductId)
			.Select(variant => new
			{
				ColorKey = variant.Color.ToLower(),
				SizeKey = variant.Size.ToLower()
			})
			.ToListAsync();

		var hasExistingDuplicate = normalizedItems.Any(item =>
			existingVariants.Any(existing =>
				existing.ColorKey == item.ColorKey &&
				existing.SizeKey == item.SizeKey));

		if (hasExistingDuplicate)
		{
			return ServiceResult<List<ProductVariantDto>>.Failure(
				"One or more variants already exist. Use adjust-stock to increase or decrease quantity.");
		}

		var variants = normalizedItems
			.Select(item => new ProductVariant
			{
				ProductId = dto.ProductId,
				Color = item.Color,
				Size = item.Size!,
				StockQuantity = item.StockQuantity
			})
			.ToList();

		_context.ProductVariants.AddRange(variants);

		await _context.SaveChangesAsync();

		var createdIds = variants
			.Select(variant => variant.Id)
			.ToList();

		var result = await _context.ProductVariants
			.Where(variant => createdIds.Contains(variant.Id))
			.OrderBy(variant => variant.Color)
			.ThenBy(variant => variant.Size)
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

		return ServiceResult<List<ProductVariantDto>>.Success(result);
	}
	public async Task<ServiceResult<List<ProductVariantDto>>> GenerateAsync(GenerateProductVariantsDto dto)
	{
		var productInfo = await _context.Products
			.Where(product => product.Id == dto.ProductId)
			.Select(product => new
			{
				product.Id,
				SizeType = product.Category.SizeType
			})
			.FirstOrDefaultAsync();

		if (productInfo == null)
		{
			return ServiceResult<List<ProductVariantDto>>.Failure("Product does not exist.");
		}

		var colors = dto.Colors
			.Select(NormalizeText)
			.Where(color => !string.IsNullOrWhiteSpace(color))
			.GroupBy(NormalizeKey)
			.Select(group => group.First())
			.ToList();

		if (colors.Count == 0)
		{
			return ServiceResult<List<ProductVariantDto>>.Failure("At least one color is required.");
		}

		var rawSizes = dto.Sizes?
			.Select(NormalizeText)
			.Where(size => !string.IsNullOrWhiteSpace(size))
			.ToList() ?? new List<string>();

		List<string> sizes;

		if (productInfo.SizeType == CategorySizeType.None)
		{
			if (rawSizes.Count > 0)
			{
				return ServiceResult<List<ProductVariantDto>>.Failure(GetInvalidSizeMessage(productInfo.SizeType));
			}

			sizes = new List<string> { NoSizeLabel };
		}
		else if (productInfo.SizeType == CategorySizeType.OneSize)
		{
			if (rawSizes.Count == 0)
			{
				sizes = new List<string> { OneSizeLabel };
			}
			else
			{
				sizes = rawSizes
					.Select(size => NormalizeSizeByCategory(size, productInfo.SizeType))
					.Where(size => !string.IsNullOrWhiteSpace(size))
					.Select(size => size!)
					.GroupBy(NormalizeKey)
					.Select(group => group.First())
					.ToList();

				if (sizes.Count == 0)
				{
					return ServiceResult<List<ProductVariantDto>>.Failure(GetInvalidSizeMessage(productInfo.SizeType));
				}
			}
		}
		else
		{
			if (rawSizes.Count == 0)
			{
				return ServiceResult<List<ProductVariantDto>>.Failure(GetInvalidSizeMessage(productInfo.SizeType));
			}

			var normalizedSizes = rawSizes
				.Select(size => NormalizeSizeByCategory(size, productInfo.SizeType))
				.ToList();

			if (normalizedSizes.Any(size => string.IsNullOrWhiteSpace(size)))
			{
				return ServiceResult<List<ProductVariantDto>>.Failure(GetInvalidSizeMessage(productInfo.SizeType));
			}

			sizes = normalizedSizes
				.Select(size => size!)
				.GroupBy(NormalizeKey)
				.Select(group => group.First())
				.ToList();
		}

		var generatedItems = colors
			.SelectMany(color => sizes.Select(size => new
			{
				Color = color,
				Size = size,
				ColorKey = NormalizeKey(color),
				SizeKey = NormalizeKey(size)
			}))
			.ToList();

		var hasDuplicateInGeneratedItems = generatedItems
			.GroupBy(item => new { item.ColorKey, item.SizeKey })
			.Any(group => group.Count() > 1);

		if (hasDuplicateInGeneratedItems)
		{
			return ServiceResult<List<ProductVariantDto>>.Failure(
				"Generated variants contain duplicates. Check colors and sizes.");
		}

		var existingVariants = await _context.ProductVariants
			.IgnoreQueryFilters()
			.Where(variant => variant.ProductId == dto.ProductId)
			.Select(variant => new
			{
				ColorKey = variant.Color.ToLower(),
				SizeKey = variant.Size.ToLower()
			})
			.ToListAsync();

		var hasExistingDuplicate = generatedItems.Any(item =>
			existingVariants.Any(existing =>
				existing.ColorKey == item.ColorKey &&
				existing.SizeKey == item.SizeKey));

		if (hasExistingDuplicate)
		{
			return ServiceResult<List<ProductVariantDto>>.Failure(
				"One or more generated variants already exist. Use adjust-stock to increase or decrease quantity.");
		}

		var variants = generatedItems
			.Select(item => new ProductVariant
			{
				ProductId = dto.ProductId,
				Color = item.Color,
				Size = item.Size,
				StockQuantity = dto.DefaultStockQuantity
			})
			.ToList();

		_context.ProductVariants.AddRange(variants);

		await _context.SaveChangesAsync();

		var createdIds = variants
			.Select(variant => variant.Id)
			.ToList();

		var result = await _context.ProductVariants
			.Where(variant => createdIds.Contains(variant.Id))
			.OrderBy(variant => variant.Color)
			.ThenBy(variant => variant.Size)
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

		return ServiceResult<List<ProductVariantDto>>.Success(result);
	}
	public async Task<ServiceResult<ProductVariantDto>> UpdateAsync(int id, UpdateProductVariantDto dto)
	{
		var variant = await _context.ProductVariants
			.Include(variant => variant.Product)
			.ThenInclude(product => product.Category)
			.FirstOrDefaultAsync(variant => variant.Id == id);

		if (variant == null)
		{
			return ServiceResult<ProductVariantDto>.Failure("Variant does not exist.");
		}

		var color = NormalizeText(dto.Color);

		if (string.IsNullOrWhiteSpace(color))
		{
			return ServiceResult<ProductVariantDto>.Failure("Color is required.");
		}

		var size = NormalizeSizeByCategory(dto.Size, variant.Product.Category.SizeType);

		if (string.IsNullOrWhiteSpace(size))
		{
			return ServiceResult<ProductVariantDto>.Failure(GetInvalidSizeMessage(variant.Product.Category.SizeType));
		}

		var colorKey = NormalizeKey(color);
		var sizeKey = NormalizeKey(size);

		var duplicateExists = await _context.ProductVariants
			.IgnoreQueryFilters()
			.AnyAsync(otherVariant =>
				otherVariant.Id != id &&
				otherVariant.ProductId == variant.ProductId &&
				otherVariant.Color.ToLower() == colorKey &&
				otherVariant.Size.ToLower() == sizeKey);

		if (duplicateExists)
		{
			return ServiceResult<ProductVariantDto>.Failure(
				"Another variant with the same color and size already exists. Use adjust-stock instead.");
		}

		variant.Color = color;
		variant.Size = size;
		variant.StockQuantity = dto.StockQuantity;
		variant.UpdatedAt = DateTime.UtcNow;

		await _context.SaveChangesAsync();

		var updatedVariant = await GetByIdAsync(variant.Id);

		if (updatedVariant == null)
		{
			return ServiceResult<ProductVariantDto>.Failure("Variant was updated but could not be loaded.");
		}

		return ServiceResult<ProductVariantDto>.Success(updatedVariant);
	}

	public async Task<ProductVariantDto?> AdjustStockAsync(int id, AdjustProductVariantStockDto dto)
	{
		var variant = await _context.ProductVariants
			.FirstOrDefaultAsync(variant => variant.Id == id);

		if (variant == null)
		{
			return null;
		}

		if (dto.QuantityChange == 0)
		{
			return null;
		}

		var oldQuantity = variant.StockQuantity;
		var newStockQuantity = oldQuantity + dto.QuantityChange;

		if (newStockQuantity < 0)
		{
			return null;
		}

		var movementType = dto.QuantityChange > 0
			? StockMovementType.Increase
			: StockMovementType.Decrease;

		var movement = new StockMovement
		{
			ProductVariantId = variant.Id,
			MovementType = movementType,
			QuantityChange = dto.QuantityChange,
			OldQuantity = oldQuantity,
			NewQuantity = newStockQuantity,
			Note = dto.Note
		};

		variant.StockQuantity = newStockQuantity;
		variant.UpdatedAt = DateTime.UtcNow;

		_context.StockMovements.Add(movement);

		await _context.SaveChangesAsync();

		return await GetByIdAsync(variant.Id);
	}

	public async Task<PagedResultDto<StockMovementDto>> GetStockMovementsAsync(int variantId, PagedRequestDto input)
	{
		var query = _context.StockMovements
			.Where(movement => movement.ProductVariantId == variantId);

		if (!string.IsNullOrWhiteSpace(input.SearchTerm))
		{
			var searchTerm = input.SearchTerm.Trim();

			query = query.Where(movement =>
				(movement.Note != null && movement.Note.Contains(searchTerm)) ||
				movement.ProductVariant.Color.Contains(searchTerm) ||
				movement.ProductVariant.Size.Contains(searchTerm) ||
				movement.ProductVariant.Product.Name.Contains(searchTerm));
		}

		query = ApplyStockMovementSorting(query, input.Sorting);

		var totalCount = await query.CountAsync();

		var pagedQuery = ApplyStockMovementPaging(query, input);

		var items = await pagedQuery
			.Select(movement => new StockMovementDto
			{
				Id = movement.Id,
				ProductVariantId = movement.ProductVariantId,
				ProductName = movement.ProductVariant.Product.Name,
				Color = movement.ProductVariant.Color,
				Size = movement.ProductVariant.Size,
				MovementType = new LookupDto
				{
					Id = (int)movement.MovementType,
					Name = movement.MovementType.ToString()
				},
				QuantityChange = movement.QuantityChange,
				OldQuantity = movement.OldQuantity,
				NewQuantity = movement.NewQuantity,
				Note = movement.Note,
				CreatedAt = movement.CreatedAt
			})
			.ToListAsync();

		return new PagedResultDto<StockMovementDto>
		{
			Items = items,
			TotalCount = totalCount
		};
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
			.OrderBy(color => color)
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
			.OrderBy(size => size)
			.ToListAsync();

		return sizes;
	}

	public async Task<List<string>> GetAvailableColorsBySizeAsync(int productId, string size)
	{
		var normalizedSizeKey = NormalizeKey(size);

		var colors = await _context.ProductVariants
			.Where(variant =>
				variant.ProductId == productId &&
				variant.Size.ToLower() == normalizedSizeKey &&
				variant.StockQuantity > 0)
			.Select(variant => variant.Color)
			.Distinct()
			.OrderBy(color => color)
			.ToListAsync();

		return colors;
	}

	public async Task<List<string>> GetAvailableSizesByColorAsync(int productId, string color)
	{
		var normalizedColorKey = NormalizeKey(color);

		var sizes = await _context.ProductVariants
			.Where(variant =>
				variant.ProductId == productId &&
				variant.Color.ToLower() == normalizedColorKey &&
				variant.StockQuantity > 0)
			.Select(variant => variant.Size)
			.Distinct()
			.OrderBy(size => size)
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

	private static IQueryable<StockMovement> ApplyStockMovementPaging(IQueryable<StockMovement> query, PagedRequestDto input)
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

	private static IQueryable<StockMovement> ApplyStockMovementSorting(IQueryable<StockMovement> query, string? sorting)
	{
		if (string.IsNullOrWhiteSpace(sorting))
		{
			return query.OrderBy(movement => movement.CreatedAt);
		}

		return sorting.Trim().ToLower() switch
		{
			"createdat" or "createdat asc" => query.OrderBy(movement => movement.CreatedAt),
			"createdat desc" => query.OrderByDescending(movement => movement.CreatedAt),

			"quantitychange" or "quantitychange asc" => query.OrderBy(movement => movement.QuantityChange),
			"quantitychange desc" => query.OrderByDescending(movement => movement.QuantityChange),

			"oldquantity" or "oldquantity asc" => query.OrderBy(movement => movement.OldQuantity),
			"oldquantity desc" => query.OrderByDescending(movement => movement.OldQuantity),

			"newquantity" or "newquantity asc" => query.OrderBy(movement => movement.NewQuantity),
			"newquantity desc" => query.OrderByDescending(movement => movement.NewQuantity),

			_ => query.OrderBy(movement => movement.CreatedAt)
		};
	}

	private static string NormalizeText(string? value)
	{
		return value?.Trim() ?? string.Empty;
	}

	private static string NormalizeKey(string? value)
	{
		return NormalizeText(value).ToLower();
	}

	private static string? NormalizeSizeByCategory(string? size, CategorySizeType sizeType)
	{
		var normalizedSize = NormalizeText(size);

		return sizeType switch
		{
			CategorySizeType.None => string.IsNullOrWhiteSpace(normalizedSize)
				? NoSizeLabel
				: null,

			CategorySizeType.OneSize => string.IsNullOrWhiteSpace(normalizedSize) ||
				string.Equals(normalizedSize, OneSizeLabel, StringComparison.OrdinalIgnoreCase) ||
				string.Equals(normalizedSize, "OneSize", StringComparison.OrdinalIgnoreCase)
					? OneSizeLabel
					: null,

			CategorySizeType.ShoeNumeric => NormalizeShoeSize(normalizedSize),

			CategorySizeType.ClothingLetter => NormalizeClothingSize(normalizedSize),

			CategorySizeType.KidsAge => string.IsNullOrWhiteSpace(normalizedSize)
				? null
				: normalizedSize,

			CategorySizeType.Custom => string.IsNullOrWhiteSpace(normalizedSize)
				? null
				: normalizedSize,

			_ => null
		};
	}

	private static string? NormalizeShoeSize(string size)
	{
		if (!int.TryParse(size, out var numericSize))
		{
			return null;
		}

		if (numericSize <= 0 || numericSize > 60)
		{
			return null;
		}

		return numericSize.ToString();
	}

	private static string? NormalizeClothingSize(string size)
	{
		var normalizedSize = size.ToUpper();

		if (!ClothingSizes.Contains(normalizedSize))
		{
			return null;
		}

		return normalizedSize;
	}

	private static string GetInvalidSizeMessage(CategorySizeType sizeType)
	{
		return sizeType switch
		{
			CategorySizeType.None =>
				"This category does not support sizes. Send no size value.",

			CategorySizeType.OneSize =>
				"This category supports one size only. Send no size value or use 'One Size'.",

			CategorySizeType.ShoeNumeric =>
				"Shoes category accepts numeric sizes only, for example 40, 41, 42.",

			CategorySizeType.ClothingLetter =>
				"Clothes category accepts these sizes only: XS, S, M, L, XL, XXL, XXXL, 2XL, 3XL, 4XL.",

			CategorySizeType.KidsAge =>
				"Kids age size is required, for example 2Y, 3Y, 4Y.",

			CategorySizeType.Custom =>
				"Custom size is required.",

			_ =>
				"Size is not valid for this product category."
		};
	}
}