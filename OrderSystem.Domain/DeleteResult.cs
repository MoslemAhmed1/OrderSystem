namespace OrderSystem.Domain
{
    public enum DeleteResult
    {
        NotFound,
        Success,
        HasExistingOrders
    }
}