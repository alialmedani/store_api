using Store.Dtos;

namespace Store.Services;

public interface IProductVariantService
{
	Task<PagedResultDto<ProductVariantDto>> GetByProductIdAsync(int productId, PagedRequestDto input);

	Task<ProductVariantDto?> GetByIdAsync(int id);

	Task<ProductVariantDto?> CreateAsync(CreateProductVariantDto dto);

	Task<ProductVariantDto?> UpdateAsync(int id, UpdateProductVariantDto dto);
	Task<ProductVariantDto?> AdjustStockAsync(int id, AdjustProductVariantStockDto dto);

	Task<bool> DeleteAsync(int id);

	Task<bool> RestoreAsync(int id);

	Task<PagedResultDto<ProductVariantDto>> GetDeletedAsync(PagedRequestDto input);

	Task<List<string>> GetAvailableColorsAsync(int productId);

	Task<List<string>> GetAvailableSizesAsync(int productId);

	Task<List<string>> GetAvailableColorsBySizeAsync(int productId, string size);

	Task<List<string>> GetAvailableSizesByColorAsync(int productId, string color);
	Task<List<ProductVariantDto>?> BulkCreateAsync(CreateBulkProductVariantsDto dto);
}