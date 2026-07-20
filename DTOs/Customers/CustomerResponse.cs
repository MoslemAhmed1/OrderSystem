namespace OrderSystem.DTOs.Customers
{
    public class CustomerResponse
    {
        public int Id { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string CustomerType { get; set; } = string.Empty;
    }
}