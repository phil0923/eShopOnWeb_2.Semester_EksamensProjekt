using Microsoft.AspNetCore.Mvc;
using ProductCatalog.Application.Abstractions;
using ProductCatalog.Application.DTOs;

namespace ProductCatalog.API.Controllers;

[ApiController]
[Route("api/[catalog]")]
public class CatalogController : ControllerBase
{
    private readonly ICatalogFacade _facade;

    public CatalogController(ICatalogFacade facade)
    {
        _facade = facade;
    }

    [HttpGet("items")]
    public async Task<ActionResult<PagedResult<CatalogItemDto>>> GetItems(
        [FromQuery] int pageIndex = 0,
        [FromQuery] int pageSize = 10,
        [FromQuery] int? brandId = null,
        [FromQuery] int? typeId = null,
        CancellationToken ct = default)
    {
        var result = await _facade.GetProductsAsync(pageIndex, pageSize, brandId, typeId, ct);
        return Ok(result);
    }

    [HttpGet("items/{id:int}")]
    public async Task<ActionResult<CatalogItemDto>> GetItemById(int id, CancellationToken ct = default)
    {
        var item = await _facade.GetProductByIdAsync(id, ct);
        if (item is null) return NotFound();
        return Ok(item);
    }

    [HttpGet("brands")]
    public async Task<ActionResult<IReadOnlyList<CatalogBrandDto>>> GetBrands(CancellationToken ct = default)
    {
        var brands = await _facade.GetBrandsAsync(ct);
        return Ok(brands);
    }

    [HttpGet("types")]
    public async Task<ActionResult<IReadOnlyList<CatalogTypeDto>>> GetTypes(CancellationToken ct = default)
    {
        var types = await _facade.GetTypesAsync(ct);
        return Ok(types);
    }
}