namespace Store.Dtos;

public class CategoryLookupDto
{
	public int Id { get; set; }

	public string Name { get; set; } = string.Empty;

	public LookupDto SizeType { get; set; } = new();
}