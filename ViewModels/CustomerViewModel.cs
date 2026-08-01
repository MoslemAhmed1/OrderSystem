namespace OrderSystem.ViewModels
{
    public class CustomerViewModel
    {
        public string FullName { get; set; } = string.Empty; // FirstName + LastName
        public string CustomerType { get; set; } = string.Empty;
        public bool IsVIP { get; set; }
    }
}
