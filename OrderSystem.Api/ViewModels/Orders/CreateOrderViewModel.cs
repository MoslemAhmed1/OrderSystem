namespace OrderSystem.ViewModels.Orders
{
    public record CreateOrderViewModel
    {
        public List<CreateOrderItemViewModel> Items { get; init; } = new List<CreateOrderItemViewModel>();
    }
}
