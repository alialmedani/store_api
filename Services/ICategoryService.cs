using Store.Dtos;

namespace Store.Services;

public interface ICategoryService
{
	Task<List<CategoryDto>> GetAllAsync();

	Task<CategoryDto?> GetByIdAsync(int id);

	Task<CategoryDto> CreateAsync(CreateCategoryDto dto);

	Task<CategoryDto?> UpdateAsync(int id, UpdateCategoryDto dto);

	Task<bool> DeleteAsync(int id);

	Task<bool> RestoreAsync(int id);
	Task<List<CategoryDto>> GetDeletedAsync();
}