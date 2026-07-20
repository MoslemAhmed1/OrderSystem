using System.ComponentModel.DataAnnotations;

public class CreateOrderRequest
{
    [Required]
    public int CustomerId { get; set; }

    [Required]
    [MinLength(1, ErrorMessage = "Order must contain at least one item.")]
    public List<CreateOrderItemRequest> Items { get; set; } = new();
}