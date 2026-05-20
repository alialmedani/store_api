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

	protected override void OnModelCreating(ModelBuilder modelBuilder)
	{
		base.OnModelCreating(modelBuilder);

		modelBuilder.Entity<Category>()
			.HasQueryFilter(category => !category.IsDeleted);

		modelBuilder.Entity<Product>()
			.HasQueryFilter(product => !product.IsDeleted);

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
	}
}