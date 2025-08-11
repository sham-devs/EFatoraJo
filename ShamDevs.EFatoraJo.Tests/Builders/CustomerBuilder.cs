using ShamDevs.EFatoraJo.Enums;
using ShamDevs.EFatoraJo.Models;

namespace ShamDevs.EFatoraJo.Tests.Builders
{
    public class CustomerBuilder
    {
        private string _name = "Test Customer";
        private string _identificationNumber = "CUSTOMER-001";
        private IdentificationType? _identificationType = IdentificationType.NIN;
        private string _postalCode = "11110";
        private string _phoneNumber = "+962790000000";

        public CustomerBuilder WithName(string name)
        {
            _name = name;
            return this;
        }

        public CustomerBuilder WithIdentificationNumber(string id)
        {
            _identificationNumber = id;
            return this;
        }

        public CustomerBuilder WithIdentificationType(IdentificationType type)
        {
            _identificationType = type;
            return this;
        }

        public CustomerBuilder WithPostalCode(string code)
        {
            _postalCode = code;
            return this;
        }

        public CustomerBuilder WithPhoneNumber(string phone)
        {
            _phoneNumber = phone;
            return this;
        }

        public Customer Build()
        {
            return new Customer(_name)
            {
                IdentificationNumber = _identificationNumber,
                IdentificationType = _identificationType,
                PostalCode = _postalCode,
                PhoneNumber = _phoneNumber
            };
        }
    }



}