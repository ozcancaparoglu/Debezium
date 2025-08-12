using System.Diagnostics;
using Microsoft.EntityFrameworkCore;
using Products.Api.Database;

namespace Products.Api.Services;

public class ProductDeleterService(
    IServiceProvider serviceProvider, 
    ILogger<ProductDeleterService> logger) : BackgroundService
{
    private static readonly ActivitySource ActivitySource = new ("Products.Api");
    
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            await DeleteProductAsync();

            await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
        }
    }

    private async Task DeleteProductAsync()
    {
        using var activity = ActivitySource.StartActivity();
        
        try
        {
            using var scope = serviceProvider.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            // Fetch a random product to delete
            var product = await dbContext.Products
                .OrderBy(p => Guid.NewGuid())
                .FirstOrDefaultAsync();

            if (product == null)
            {
                logger.LogWarning("No products found to delete");
                return;
            }

            dbContext.Products.Remove(product);
            var savedCount = await dbContext.SaveChangesAsync();

            activity?.SetTag("products.deleted", savedCount);
            logger.LogInformation("Deleted {Count} product(s)", savedCount);
        }
        catch (Exception ex)
        {
            activity?.SetStatus(ActivityStatusCode.Error, ex.ToString());
            logger.LogError(ex, "Error deleting products");
            throw;
        }
    }
}