namespace OrderSystem.ViewModels.Customers
{
    public record CustomerViewModel
    {
        public int Id { get; init; }
        public string FirstName { get; init; } = string.Empty;
        public string LastName { get; init; } = string.Empty;
        public string CustomerType { get; init; } = string.Empty;
    }
}
