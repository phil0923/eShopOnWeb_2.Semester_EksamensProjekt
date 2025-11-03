using CommonModels.MessageBroker.Contracts;
using LagerService.Database;
using MassTransit;
using MassTransit.Transports;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LagerService.Services
{
    public class CatalogItemsPublisher : BackgroundService
    {
        private readonly DBContext _dbContext;
        private readonly IPublishEndpoint _publishEndpoint;
        private readonly ILogger<CatalogItemsPublisher> _logger;

        public CatalogItemsPublisher(DBContext dbContext, IPublishEndpoint publishEndpoint, ILogger<CatalogItemsPublisher> logger)
        {
            _dbContext = dbContext;
            _publishEndpoint = publishEndpoint;
            _logger = logger;
        }
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("CatalogItemsPublisher started.");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    var items = _dbContext.Items.ToList();

                    var catalogItemsEvent = new CatalogItemsEvent
                    {
                        Items = items,
                    };


                    await _publishEndpoint.Publish(catalogItemsEvent, stoppingToken);

                    _logger.LogInformation("Published {Count} catalog items.", items.Count);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error while publishing catalog items.");
                }

                // Wait 5 minutes before next publish
                await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
            }
        }
    }
}
