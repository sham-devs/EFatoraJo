using ShamDevs.EFatoraJo.Models;

namespace ShamDevs.EFatoraJo.Tests.Builders
{
    public class SupplierBuilder
    {
        private string _taxVATNumber = "JO123456789";
        private string _incomeSourceSequence = "SRC-001";
        private string _registeredSupplierName = "Test Supplier";

        public SupplierBuilder WithTaxVATNumber(string vatNumber)
        {
            _taxVATNumber = vatNumber;
            return this;
        }

        public SupplierBuilder WithIncomeSourceSequence(string sequence)
        {
            _incomeSourceSequence = sequence;
            return this;
        }

        public SupplierBuilder WithRegisteredSupplierName(string name)
        {
            _registeredSupplierName = name;
            return this;
        }

        public Supplier Build()
        {
            return new Supplier(
                _taxVATNumber,
                _incomeSourceSequence,
                _registeredSupplierName);
        }
    }

}