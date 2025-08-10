namespace ShamDevs.EFatoraJo.Models
{
    public class Customer
    {
        public Customer(string name)
        {
            Name = name;
        }
        public string? IdentificationNumber { get; set; }
        public string? IdentificationType { get; set; }
        public string? PostalCode { get; set; }
        public string Name { get; set; }
        public string? PhoneNumber { get; set; }

    }
}