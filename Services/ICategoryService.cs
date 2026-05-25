using Store.Dtos;

namespace Store.Services;

public interface ICategoryService
{
	Task<PagedResultDto<CategoryDto>> GetAllAsync(PagedRequestDto input);

	Task<CategoryDto?> GetByIdAsync(int id);

	Task<ServiceResult<CategoryDto>> CreateAsync(CreateCategoryDto dto);

	Task<ServiceResult<CategoryDto>> UpdateAsync(int id, UpdateCategoryDto dto);

	Task<ServiceResult<bool>> DeleteAsync(int id);

	Task<ServiceResult<bool>> RestoreAsync(int id);

	Task<PagedResultDto<CategoryDto>> GetDeletedAsync(PagedRequestDto input);
}