using System.Diagnostics;
using Microsoft.EntityFrameworkCore;
using Products.Api.Database;

namespace Products.Api.Services;

public class ProductUpdaterService(
    IServiceProvider serviceProvider, 
    ILogger<ProductUpdaterService> logger) : BackgroundService
{
    private static readonly ActivitySource ActivitySource = new ("Products.Api");
    
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            await UpdateProductAsync();

            await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
        }
    }

    private async Task UpdateProductAsync()
    {
        using var activity = ActivitySource.StartActivity();
        
        try
        {
            using var scope = serviceProvider.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            // Fetch a random product to update
            var product = await dbContext.Products
                .OrderBy(p => Guid.NewGuid())
                .FirstOrDefaultAsync();

            if (product == null)
            {
                logger.LogWarning("No products found to update");
                return;
            }

            // Update product details
            product.Name = "Updated " + product.Name;
            product.Description = "Updated " + product.Description;
            product.Price += 10; // Increment price by 10

            dbContext.Products.Update(product);
            var savedCount = await dbContext.SaveChangesAsync();

            activity?.SetTag("products.updated", savedCount);
            logger.LogInformation("Updated {Count} product(s)", savedCount);
        }
        catch (Exception ex)
        {
            activity?.SetStatus(ActivityStatusCode.Error, ex.ToString());
            logger.LogError(ex, "Error updating products");
            throw;
        }
    }
}