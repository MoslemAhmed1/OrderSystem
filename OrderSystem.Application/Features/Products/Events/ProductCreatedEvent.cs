using MediatR;

namespace OrderSystem.Application.Features.Products.Events
{
    public record ProductCreatedEvent(
        int ProductId,
        string Name,
        decimal Price,
        int StockQuantity) : INotification;
}
