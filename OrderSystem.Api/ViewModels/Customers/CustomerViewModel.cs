namespace OrderSystem.ViewModels.Customers
{
    public class CustomerViewModel
    {
        public int Id { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string CustomerType { get; set; } = string.Empty;
    }
}