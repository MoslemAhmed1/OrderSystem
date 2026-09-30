using MediatR;
using Microsoft.Extensions.Logging;

namespace OrderSystem.Application.Features.Products.Events
{
    public class ProductCreatedEventHandler : INotificationHandler<ProductCreatedEvent>
    {
        private readonly ILogger<ProductCreatedEventHandler> _logger;

        public ProductCreatedEventHandler(ILogger<ProductCreatedEventHandler> logger)
        {
            _logger = logger;
        }

        public Task Handle(ProductCreatedEvent notification, CancellationToken cancellationToken)
        {
            _logger.LogInformation(
                "Domain event: Product created [Id={ProductId}, Name={Name}, Price={Price:C}, Stock={Stock}]",
                notification.ProductId,
                notification.Name,
                notification.Price,
                notification.StockQuantity);

            return Task.CompletedTask;
        }
    }
}
