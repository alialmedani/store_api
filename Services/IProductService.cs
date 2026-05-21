using Store.Dtos;

namespace Store.Services;

public interface IProductService
{
	Task<PagedResultDto<ProductDto>> GetAllAsync(PagedRequestDto input);

	Task<ProductDto?> GetByIdAsync(int id);

	Task<ProductDto?> CreateAsync(CreateProductDto dto);

	Task<ProductDto?> UpdateAsync(int id, UpdateProductDto dto);

	Task<bool> DeleteAsync(int id);

	Task<bool> RestoreAsync(int id);

	Task<PagedResultDto<ProductDto>> GetDeletedAsync(PagedRequestDto input);
}