using Store.Dtos;

namespace Store.Services;

public interface IProductService
{
	Task<PagedResultDto<ProductDto>> GetAllAsync(PagedRequestDto input);

	Task<ProductDetailsDto?> GetByIdAsync(int id);

	Task<ServiceResult<ProductDto>> CreateAsync(CreateProductDto dto);

	Task<ServiceResult<ProductDto>> UpdateAsync(int id, UpdateProductDto dto);

	Task<ServiceResult<bool>> DeleteAsync(int id);

	Task<ServiceResult<bool>> RestoreAsync(int id);

	Task<PagedResultDto<ProductDto>> GetDeletedAsync(PagedRequestDto input);
}