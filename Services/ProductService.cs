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

	public async Task<List<ProductDto>> GetAllAsync()
	{
		var products = await _context.Products
			.Include(product => product.Category)
			.Select(product => new ProductDto
			{
				Id = product.Id,
				Name = product.Name,
				Description = product.Description,
				Price = product.Price,
				StockQuantity = product.StockQuantity,
				IsActive = product.IsActive,
				TargetAudience = product.TargetAudience,
				CategoryId = product.CategoryId,
				CategoryName = product.Category.Name,
				CreatedAt = product.CreatedAt,
				UpdatedAt = product.UpdatedAt
			})
			.ToListAsync();

		return products;
	}

	public async Task<ProductDto?> GetByIdAsync(int id)
	{
		var product = await _context.Products
			.Include(product => product.Category)
			.Where(product => product.Id == id)
			.Select(product => new ProductDto
			{
				Id = product.Id,
				Name = product.Name,
				Description = product.Description,
				Price = product.Price,
				StockQuantity = product.StockQuantity,
				IsActive = product.IsActive,
				TargetAudience = product.TargetAudience,
				CategoryId = product.CategoryId,
				CategoryName = product.Category.Name,
				CreatedAt = product.CreatedAt,
				UpdatedAt = product.UpdatedAt
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
			StockQuantity = dto.StockQuantity,
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
		product.StockQuantity = dto.StockQuantity;
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

	public async Task<List<ProductDto>> GetDeletedAsync()
	{
		var products = await _context.Products
			.IgnoreQueryFilters()
			.Include(product => product.Category)
			.Where(product => product.IsDeleted)
			.Select(product => new ProductDto
			{
				Id = product.Id,
				Name = product.Name,
				Description = product.Description,
				Price = product.Price,
				StockQuantity = product.StockQuantity,
				IsActive = product.IsActive,
				TargetAudience = product.TargetAudience,
				CategoryId = product.CategoryId,
				CategoryName = product.Category.Name,
				CreatedAt = product.CreatedAt,
				UpdatedAt = product.UpdatedAt
			})
			.ToListAsync();

		return products;
	}
}