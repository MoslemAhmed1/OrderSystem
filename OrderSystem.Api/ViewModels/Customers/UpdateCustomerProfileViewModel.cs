namespace OrderSystem.ViewModels.Customers
{
    public record UpdateCustomerProfileViewModel
    {
        public string FirstName { get; init; } = string.Empty;
        public string LastName { get; init; } = string.Empty;
    }
}
