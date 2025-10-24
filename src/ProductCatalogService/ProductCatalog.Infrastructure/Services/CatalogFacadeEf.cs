using Microsoft.EntityFrameworkCore;
using ProductCatalog.Infrastructure.Data;
using ProductCatalog.Application.Abstractions;
using ProductCatalog.Application.DTOs;

namespace ProductCatalog.Infrastructure.Services
{
    public class CatalogFacadeEf : ICatalogFacade
    {
        private readonly CatalogContext _db;

        public CatalogFacadeEf(CatalogContext db)
        {
            _db = db;
        }

        public async Task<PagedResult<CatalogItemDto>> GetProductsAsync(
            int pageIndex, int pageSize,
            int? brandId, int? typeId,
            CancellationToken ct = default)
        {

            var query = _db.CatalogItems.AsNoTracking().AsQueryable();

            if (brandId.HasValue) query = query.Where(i => i.CatalogBrandId == brandId.Value);
            if (typeId.HasValue) query = query.Where(i => i.CatalogTypeId == typeId.Value);

            var total = await query.CountAsync(ct);

            var items = await query
                .OrderBy(i => i.Id)
                .Skip(pageIndex * pageSize)
                .Take(pageSize)
                .Select(i => new CatalogItemDto
                {
                    Id = i.Id,
                    Name = i.Name,
                    Description = i.Description,
                    Price = i.Price,
                    PictureUri = i.PictureUri,
                    CatalogBrandId = i.CatalogBrandId,
                    CatalogTypeId = i.CatalogTypeId
                })
                .ToListAsync(ct);

            return new PagedResult<CatalogItemDto>
            {
                Items = items,
                TotalItems = total,
                PageIndex = pageIndex,
                PageSize = pageSize
            };
        }

        public async Task<CatalogItemDto?> GetProductByIdAsync(int id, CancellationToken ct = default)
        {
            return await _db.CatalogItems
                .AsNoTracking()
                .Where(i => i.Id == id)
                .Select(i => new CatalogItemDto
                {
                    Id = i.Id,
                    Name = i.Name,
                    Description = i.Description,
                    Price = i.Price,
                    PictureUri = i.PictureUri,
                    CatalogBrandId = i.CatalogBrandId,
                    CatalogTypeId = i.CatalogTypeId
                })
                .FirstOrDefaultAsync(ct);
        }

        public async Task<IReadOnlyList<CatalogBrandDto>> GetBrandsAsync(CancellationToken ct = default)
        {
            return await _db.CatalogBrands
                .AsNoTracking()
                .OrderBy(b => b.Brand)
                .Select(b => new CatalogBrandDto
                {
                    Id = b.Id,
                    Brand = b.Brand
                })
                .ToListAsync(ct);
        }

        public async Task<IReadOnlyList<CatalogTypeDto>> GetTypesAsync(CancellationToken ct = default)
        {
            return await _db.CatalogTypes
                .AsNoTracking()
                .OrderBy(t => t.Type)
                .Select(t => new CatalogTypeDto
                {
                    Id = t.Id,
                    Type = t.Type
                })
                .ToListAsync(ct);
        }
    }
}