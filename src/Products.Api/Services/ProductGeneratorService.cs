using System.Diagnostics;
using Bogus;
using Products.Api.Database;

namespace Products.Api.Services;

public class ProductGeneratorService(
    IServiceProvider serviceProvider, 
    ILogger<ProductGeneratorService> logger) : BackgroundService
{
    private static readonly ActivitySource ActivitySource = new ("Products.Api");
    
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            await GenerateProductsAsync();

            await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
        }
    }

    private async Task GenerateProductsAsync()
    {
        using var activity = ActivitySource.StartActivity();

        try
        {
            using var scope = serviceProvider.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            var faker = new Faker<Product>()
                .RuleFor(p => p.Name, f => f.Commerce.ProductName())
                .RuleFor(p => p.Description, f => f.Commerce.ProductDescription())
                .RuleFor(p => p.Price, f => f.Random.Decimal(1, 1000));

            var products = faker.Generate(10);

            await dbContext.Products.AddRangeAsync(products);
            var savedCount = await dbContext.SaveChangesAsync();

            activity?.SetTag("products.count", savedCount);
            logger.LogInformation("Generated and inserted {Count} new products", savedCount);
        }
        catch (Exception ex)
        {
            activity?.SetStatus(ActivityStatusCode.Error, ex.ToString());
            logger.LogError(ex, "Error generating products");
            throw;
        }
    }
}