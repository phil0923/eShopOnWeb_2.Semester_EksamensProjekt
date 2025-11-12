using Microsoft.AspNetCore.Mvc;
using ProductCatalog.Application.Abstractions;
using ProductCatalog.Application.DTOs;

namespace ProductCatalog.API.Controllers;

[ApiController]
[Route("api/catalog")]
public class CatalogController : ControllerBase
{
    private readonly ICatalogFacade _catalog;

    public CatalogController(ICatalogFacade catalog) => _catalog = catalog;

    [HttpGet("items")]
    public async Task<IActionResult> GetItems(int? catalogBrandId, int? catalogTypeId, int pageIndex = 0, int pageSize = 10, CancellationToken ct = default)
    {
        var result = await _catalog.GetProductsAsync(catalogBrandId, catalogTypeId, pageIndex, pageSize, ct);
        return Ok(result);
    }

    [HttpGet("items/{id:int}")]
    public async Task<IActionResult> GetItemById(int id, CancellationToken ct = default)
    {
        var item = await _catalog.GetProductByIdAsync(id, ct);
        return item is null ? NotFound() : Ok(item);
    }

    [HttpGet("brands")]
    public async Task<IActionResult> GetBrands(CancellationToken ct = default)
        => Ok(await _catalog.GetBrandsAsync(ct));

    [HttpGet("types")]
    public async Task<IActionResult> GetTypes(CancellationToken ct = default)
        => Ok(await _catalog.GetTypesAsync(ct));
}