namespace OrderSystem.ViewModels.Orders
{
    public record UpdateOrderItemsViewModel
    {
        public List<CreateOrderItemViewModel> Items { get; init; } = new List<CreateOrderItemViewModel>();
    }
}
