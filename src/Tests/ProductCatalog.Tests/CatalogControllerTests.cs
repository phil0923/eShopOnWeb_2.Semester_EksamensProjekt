using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using ProductCatalog.API.Controllers;
using ProductCatalog.Application.Abstractions;
using ProductCatalog.Application.DTOs;
using Xunit;

namespace ProductCatalog.Tests;

public class CatalogControllerTests
{
    [Fact]
    public async Task GetItems_returns_200_and_payload()
    {
        // Arrange
        var page = new PagedResult<CatalogItemDto>
        {
            Items = new List<CatalogItemDto>
            {
                new CatalogItemDto
                {
                    Id = 1,
                    Name = "Test",
                    Description = "Desc",
                    Price = 10.0m,
                    PictureUri = "",
                    CatalogBrandId = 2,
                    CatalogTypeId = 3
                }
            },
            TotalItems = 1,
            PageIndex = 0,
            PageSize = 10
        };

        var facade = new Mock<ICatalogFacade>();
        facade.Setup(f => f.GetProductsAsync(
                /*brandId*/  null,
                /*typeId*/   null,
                /*pageIndex*/0,
                /*pageSize*/ 10,
                It.IsAny<CancellationToken>()))
              .ReturnsAsync(page);

        var controller = new CatalogController(facade.Object);

        // Act
        var result = await controller.GetItems(null, null, 0, 10, CancellationToken.None);

        // Assert
        var ok = result as OkObjectResult;
        ok.Should().NotBeNull();
        ok!.StatusCode.Should().Be(200);

        var payload = ok.Value as PagedResult<CatalogItemDto>;
        payload.Should().NotBeNull();
        payload!.Items.Should().HaveCount(1);
        payload.Items[0].Name.Should().Be("Test");

        facade.Verify(f => f.GetProductsAsync(null, null, 0, 10, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetItemById_returns_NotFound_when_null()
    {
        // Arrange
        var facade = new Mock<ICatalogFacade>();
        facade.Setup(f => f.GetProductByIdAsync(999, It.IsAny<CancellationToken>()))
              .ReturnsAsync((CatalogItemDto?)null);

        var controller = new CatalogController(facade.Object);

        // Act
        var result = await controller.GetItemById(999, CancellationToken.None);

        // Assert
        result.Should().BeOfType<NotFoundResult>();
        facade.Verify(f => f.GetProductByIdAsync(999, It.IsAny<CancellationToken>()), Times.Once);
    }
}
