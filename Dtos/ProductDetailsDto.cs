namespace Store.Dtos;

public class ProductDetailsDto
{
	public int Id { get; set; }

	public string Name { get; set; } = string.Empty;

	public string? Description { get; set; }

	public decimal Price { get; set; }

	public int TotalStockQuantity { get; set; }

	public bool IsActive { get; set; }

	public LookupDto Category { get; set; } = new();

	public LookupDto TargetAudience { get; set; } = new();

	public DateTime CreatedAt { get; set; }

	public DateTime? UpdatedAt { get; set; }

	public List<string> AvailableColors { get; set; } = new();

	public List<string> AvailableSizes { get; set; } = new();

	public List<ProductVariantSummaryDto> Variants { get; set; } = new();
}