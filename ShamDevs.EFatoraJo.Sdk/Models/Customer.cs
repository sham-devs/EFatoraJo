using ShamDevs.EFatoraJo.Enums;

namespace ShamDevs.EFatoraJo.Models
{
    public class Customer
    {
        public Customer(string name)
        {
            Name = name;
        }
        public string? IdentificationNumber { get; set; }
        public IdentificationType? IdentificationType { get; set; }
        public string? PostalCode { get; set; }
        public string Name { get; set; }
        public string? PhoneNumber { get; set; }
        public CountrySubentityCode? City { get; set; }

    }
}