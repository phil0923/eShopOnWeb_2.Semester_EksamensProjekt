using ProductCatalog.Application.DTOs;

namespace ProductCatalog.Application.Abstractions;
public interface ICatalogFacade
{
    Task<PagedResult<CatalogItemDto>> GetProductsAsync(
        int? brandId, int? typeId,
        int pageIndex, int pageSize,
        CancellationToken ct = default);

    Task<CatalogItemDto?> GetProductByIdAsync(int id, CancellationToken ct = default);
    Task<IReadOnlyList<CatalogBrandDto>> GetBrandsAsync(CancellationToken ct = default);
    Task<IReadOnlyList<CatalogTypeDto>> GetTypesAsync(CancellationToken ct = default);
}
