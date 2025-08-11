using MassTransit;

namespace Products.Api.Consumers;

public class ProductUpdatesConsumer(ILogger<ProductUpdatesConsumer> logger) : IConsumer<ProductUpdate>
{
    public Task Consume(ConsumeContext<ProductUpdate> context)
    {
        var jsonMessage = context.Message;
        //logger.LogInformation("Received product change: {@JsonMessage}", jsonMessage);
        
        logger.LogInformation("Received product change: Id={Id}, Name={Name}, Description={Description}, Price={Price}",
            jsonMessage.Id, jsonMessage.Name, jsonMessage.Description, jsonMessage.Price);

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