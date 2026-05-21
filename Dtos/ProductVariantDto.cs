namespace Store.Dtos;

public class ProductVariantDto
{
	public int Id { get; set; }

	public int ProductId { get; set; }

	public string ProductName { get; set; } = string.Empty;

	public string Color { get; set; } = string.Empty;

	public string Size { get; set; } = string.Empty;

	public int StockQuantity { get; set; }

	public DateTime CreatedAt { get; set; }

	public DateTime? UpdatedAt { get; set; }
}