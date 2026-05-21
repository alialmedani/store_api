using System.ComponentModel.DataAnnotations;

namespace Store.Dtos;

public class PagedRequestDto
{
	[Range(0, int.MaxValue)]
	public int? SkipCount { get; set; }

	[Range(1, int.MaxValue)]
	public int? MaxResultCount { get; set; }

	public string? SearchTerm { get; set; }

	public string? Sorting { get; set; }
}