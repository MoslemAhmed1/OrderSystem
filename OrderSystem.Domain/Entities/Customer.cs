using OrderSystem.Domain.Enums;

namespace OrderSystem.Domain.Entities
{
    public class Customer
    {
        public int Id { get; set; }

        public required string FirstName { get; set; }

        public required string LastName { get; set; }

        public CustomerType CustomerType { get; set; }

        public int? UserId { get; set; }
        public User? User { get; set; }
    }
}