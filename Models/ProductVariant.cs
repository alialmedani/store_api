using System.ComponentModel.DataAnnotations;

namespace Store.Models;

public class ProductVariant : BaseEntity
{
	public int ProductId { get; set; }

	public Product Product { get; set; } = null!;
	public ICollection<StockMovement> StockMovements { get; set; } = new List<StockMovement>();
	[Required]
	[MaxLength(50)]
	public string Color { get; set; } = string.Empty;

	[Required]
	[MaxLength(20)]
	public string Size { get; set; } = string.Empty;

	public int StockQuantity { get; set; }
}