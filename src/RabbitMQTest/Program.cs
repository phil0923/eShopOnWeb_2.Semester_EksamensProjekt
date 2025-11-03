using MassTransit;
using RabbitMQTest;

var builder = Host.CreateApplicationBuilder(args);

// MassTransit configuration
builder.Services.AddMassTransit(x =>
{
    x.UsingRabbitMq((ctx, cfg) =>
    {
        cfg.Host("localhost", "/", h =>
        {
            h.Username("guest");
            h.Password("guest");
        });
    });
});

// register the background worker
builder.Services.AddHostedService<Worker>();

var host = builder.Build();
host.Run();
