using Store.Models;

namespace Store.Dtos;

public class ProductDto
{
	public int Id { get; set; }

	public string Name { get; set; } = string.Empty;

	public string? Description { get; set; }

	public decimal Price { get; set; }

	public int StockQuantity { get; set; }

	public bool IsActive { get; set; }

	public ProductTargetAudience TargetAudience { get; set; }

	public int CategoryId { get; set; }

	public string CategoryName { get; set; } = string.Empty;

	public DateTime CreatedAt { get; set; }

	public DateTime? UpdatedAt { get; set; }
}