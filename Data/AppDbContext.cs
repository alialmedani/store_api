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
}