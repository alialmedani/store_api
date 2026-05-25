using Microsoft.EntityFrameworkCore;
using Store.Models;

namespace Store.Data;

public class AppDbContext : DbContext
{
	public AppDbContext(DbContextOptions<AppDbContext> options)
		: base(options)
	{
	}

	public DbSet<Category> Categories => Set<Category>();

	public DbSet<Product> Products => Set<Product>();

	public DbSet<ProductVariant> ProductVariants => Set<ProductVariant>();

	public DbSet<StockMovement> StockMovements => Set<StockMovement>();

	protected override void OnModelCreating(ModelBuilder modelBuilder)
	{
		base.OnModelCreating(modelBuilder);

		modelBuilder.Entity<Category>()
			.HasQueryFilter(category => !category.IsDeleted);

		modelBuilder.Entity<Product>()
			.HasQueryFilter(product => !product.IsDeleted);

		modelBuilder.Entity<ProductVariant>()
			.HasQueryFilter(variant => !variant.IsDeleted);

		modelBuilder.Entity<StockMovement>()
			.HasQueryFilter(movement => !movement.IsDeleted);

		modelBuilder.Entity<Category>()
			.HasIndex(category => category.Name)
			.HasDatabaseName("UX_Categories_Name_Active")
			.IsUnique()
			.HasFilter("[IsDeleted] = 0");

		modelBuilder.Entity<Product>()
			.HasIndex(product => new { product.CategoryId, product.Name })
			.HasDatabaseName("UX_Products_CategoryId_Name_Active")
			.IsUnique()
			.HasFilter("[IsDeleted] = 0");

		modelBuilder.Entity<Product>()
			.Property(product => product.Price)
			.HasPrecision(18, 2);

		modelBuilder.Entity<Product>()
			.Property(product => product.TargetAudience)
			.HasDefaultValue(ProductTargetAudience.Unisex);

		modelBuilder.Entity<Product>()
			.HasOne(product => product.Category)
			.WithMany(category => category.Products)
			.HasForeignKey(product => product.CategoryId)
			.OnDelete(DeleteBehavior.Restrict);

		modelBuilder.Entity<ProductVariant>()
			.HasOne(variant => variant.Product)
			.WithMany(product => product.Variants)
			.HasForeignKey(variant => variant.ProductId)
			.OnDelete(DeleteBehavior.Cascade);

		modelBuilder.Entity<ProductVariant>()
			.HasIndex(variant => new { variant.ProductId, variant.Color, variant.Size })
			.HasDatabaseName("UX_ProductVariants_ProductId_Color_Size_Active")
			.IsUnique()
			.HasFilter("[IsDeleted] = 0");

		modelBuilder.Entity<StockMovement>()
			.HasOne(movement => movement.ProductVariant)
			.WithMany(variant => variant.StockMovements)
			.HasForeignKey(movement => movement.ProductVariantId)
			.OnDelete(DeleteBehavior.Cascade);
	}
}