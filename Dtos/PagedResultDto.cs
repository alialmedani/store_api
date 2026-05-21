namespace Store.Dtos;

public class PagedResultDto<T>
{
	public List<T> Items { get; set; } = new();

	public int TotalCount { get; set; }
}