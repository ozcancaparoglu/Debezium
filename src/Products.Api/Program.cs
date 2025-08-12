using System.Net.Mime;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Products.Api.Consumers;
using Products.Api.Database;
using Products.Api.Extensions;
using Products.Api.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("Postgres")));

builder.Services.AddMassTransit(x =>
{
    x.AddConsumer<ProductUpdatesConsumer>();
    
    x.UsingRabbitMq((context, cfg) =>
    {
        cfg.Host(builder.Configuration.GetConnectionString("RabbitMQ"));
        
        cfg.ReceiveEndpoint("product-updates", endpointConfigurator =>
        {
            endpointConfigurator.ConfigureConsumeTopology = false;

            endpointConfigurator.DefaultContentType = new ContentType("application/json");
            endpointConfigurator.UseRawJsonSerializer();

            endpointConfigurator.Consumer<ProductUpdatesConsumer>(context);
        });
    });
});

//builder.Services.AddHostedService<ProductGeneratorService>();
builder.Services.AddHostedService<ProductUpdaterService>();
//builder.Services.AddHostedService<ProductDeleterService>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();

    app.ApplyMigrations();
}

app.UseHttpsRedirection();

app.MapGet("/products", async (AppDbContext db) =>
    {
        return await db.Products.AsNoTracking().ToListAsync();
    })
    .WithName("GetProducts")
    .WithOpenApi();

app.Run();