namespace OrderSystem.Application.DTOs.Customers
{
    public record UpdateCustomerProfileRequest
    {
        public string FirstName { get; init; } = string.Empty;
        public string LastName { get; init; } = string.Empty;
    }
}
