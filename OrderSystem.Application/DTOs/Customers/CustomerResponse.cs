namespace OrderSystem.Application.DTOs.Customers
{
    public record CustomerResponse
    {
        public int Id { get; init; }
        public string FirstName { get; init; } = string.Empty;
        public string LastName { get; init; } = string.Empty;
        public string CustomerType { get; init; } = string.Empty;
    }
}
