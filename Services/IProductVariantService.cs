using Store.Dtos;

namespace Store.Services;

public interface IProductVariantService
{
	Task<PagedResultDto<ProductVariantDto>> GetByProductIdAsync(int productId, PagedRequestDto input);

	Task<ProductVariantDto?> GetByIdAsync(int id);

	Task<ServiceResult<ProductVariantDto>> CreateAsync(CreateProductVariantDto dto);

	Task<ServiceResult<List<ProductVariantDto>>> BulkCreateAsync(CreateBulkProductVariantsDto dto);

	Task<ServiceResult<List<ProductVariantDto>>> GenerateAsync(GenerateProductVariantsDto dto);

	Task<ServiceResult<ProductVariantDto>> UpdateAsync(int id, UpdateProductVariantDto dto);

	Task<ServiceResult<ProductVariantDto>> AdjustStockAsync(int id, AdjustProductVariantStockDto dto);

	Task<PagedResultDto<StockMovementDto>> GetStockMovementsAsync(int variantId, PagedRequestDto input);

	Task<bool> DeleteAsync(int id);

	Task<ServiceResult<bool>> RestoreAsync(int id);

	Task<PagedResultDto<ProductVariantDto>> GetDeletedAsync(PagedRequestDto input);

	Task<List<string>> GetAvailableColorsAsync(int productId);

	Task<List<string>> GetAvailableSizesAsync(int productId);

	Task<List<string>> GetAvailableColorsBySizeAsync(int productId, string size);

	Task<List<string>> GetAvailableSizesByColorAsync(int productId, string color);
}