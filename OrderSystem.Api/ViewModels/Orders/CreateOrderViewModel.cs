namespace OrderSystem.ViewModels.Orders
{
    public class CreateOrderViewModel
    {
        public List<CreateOrderItemViewModel> Items { get; set; } = new List<CreateOrderItemViewModel>();
    }
}
