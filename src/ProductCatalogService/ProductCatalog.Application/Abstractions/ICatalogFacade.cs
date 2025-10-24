using ProductCatalog.Application.DTOs;

namespace ProductCatalog.Application.Abstractions
{
    public interface ICatalogFacade
    {
        Task<PagedResult<CatalogItemDto>> GetProductsAsync(int pageIndex, int pageSize, int? brandId, int? typeId, CancellationToken ct = default);
        Task<IReadOnlyList<CatalogBrandDto>> GetBrandsAsync(CancellationToken ct = default);
        Task<IReadOnlyList<CatalogTypeDto>> GetTypesAsync(CancellationToken ct = default);
        Task<CatalogItemDto> GetProductByIdAsync(int id, CancellationToken ct = default);
    }
}