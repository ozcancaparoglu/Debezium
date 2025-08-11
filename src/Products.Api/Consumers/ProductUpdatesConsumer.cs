using MassTransit;

namespace Products.Api.Consumers;

public class ProductUpdatesConsumer(ILogger<ProductUpdatesConsumer> logger) : IConsumer<ProductUpdate>
{
    public Task Consume(ConsumeContext<ProductUpdate> context)
    {
        var jsonMessage = context.Message;
        //logger.LogInformation("Received product change: {@JsonMessage}", jsonMessage);
        
        logger.LogInformation($"---- Id: {jsonMessage.Id.ToString()}, Name: {jsonMessage.Name}, Description: {jsonMessage.Description}, Price: {jsonMessage.Price} ----" );
        

        // You can add more processing logic here if needed

        return Task.CompletedTask;
    }
}

public class ProductUpdate
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Price { get; set; }
}