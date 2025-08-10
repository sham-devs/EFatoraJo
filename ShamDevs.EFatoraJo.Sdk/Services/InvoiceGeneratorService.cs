using ShamDevs.EFatoraJo.Enums;
using ShamDevs.EFatoraJo.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using UblSharp.CommonAggregateComponents;
using UblSharp.UnqualifiedDataTypes;

namespace ShamDevs.EFatoraJo.Services
{
    public static class InvoiceGeneratorService
    {
        /*  ----------  helper to decide whether an invoice type should contain VAT  ----------  */
        private static bool HasVat(InvoiceType type) => type != InvoiceType.Income;

        /*  ----------  helper to decide whether an invoice type should contain Special tax  ----------  */
        private static bool HasSpecialTax(InvoiceType type) => type == InvoiceType.SpecialSales;
        // Generate the full UBL 2.1 Invoice
        public static UblSharp.InvoiceType GenerateUBL21(Invoice invoice)
        {
            var ublInvoice = new UblSharp.InvoiceType();

            AddBasicDetails(ublInvoice, invoice);
            AddSupplierParty(ublInvoice, invoice.Supplier);
            AddCustomerParty(ublInvoice, invoice.Customer);
            AddSellerSupplierParty(ublInvoice, invoice.Supplier);
            AddMonetaryTotals(ublInvoice, invoice.InvoiceTotals, invoice.Type);
            AddInvoiceLines(ublInvoice, invoice.InvoiceDetails, invoice.Type);

            return ublInvoice;
        }

        private static void AddBasicDetails(UblSharp.InvoiceType ublInvoice, Invoice invoice)
        {
            ublInvoice.ID = new IdentifierType { Value = invoice.InvoiceNumber };
            ublInvoice.UUID = new IdentifierType { Value = invoice.UniqueSerialNumber };
            ublInvoice.IssueDate = new DateType { Value = DateTime.Parse(invoice.InvoiceDate) };
            ublInvoice.InvoiceTypeCode = new CodeType { Value = "388", name = invoice.InvoiceTypeCode };
            ublInvoice.Note = new List<TextType> { new TextType { Value = invoice.InvoiceNote } };
            ublInvoice.DocumentCurrencyCode = new CodeType { Value = invoice.CurrencyCode };
            ublInvoice.TaxCurrencyCode = new CodeType { Value = invoice.CurrencyCode };
            ublInvoice.AdditionalDocumentReference = new List<DocumentReferenceType>
            {
                new DocumentReferenceType
                {
                    ID = new IdentifierType { Value = "ICV" },
                    UUID = new IdentifierType { Value = Guid.NewGuid().ToString() }
                }
            };
        }

        private static void AddSupplierParty(UblSharp.InvoiceType ublInvoice, Supplier supplier)
        {
            ublInvoice.AccountingSupplierParty = new SupplierPartyType
            {
                Party = new PartyType
                {
                    PostalAddress = new AddressType
                    {
                        Country = new CountryType
                        {
                            IdentificationCode = new CodeType { Value = "JO" } // Country code for Jordan
                        }
                    },
                    PartyTaxScheme = new List<PartyTaxSchemeType>
                    {
                        new PartyTaxSchemeType
                        {
                            CompanyID = new IdentifierType { Value = supplier.TaxVATNumber },
                            TaxScheme = new TaxSchemeType
                            {
                                ID = new IdentifierType { Value = "VAT" }
                            }
                        }
                    },
                    PartyLegalEntity = new List<PartyLegalEntityType>
                    {
                        new PartyLegalEntityType
                        {
                            RegistrationName = new NameType { Value = supplier.RegisteredSupplierName }
                        }
                    }
                }
            };
        }

        private static void AddCustomerParty(UblSharp.InvoiceType ublInvoice, Customer customer)
        {
            var party = new PartyType
            {
                PartyIdentification = new List<PartyIdentificationType>
                {
                    new PartyIdentificationType
                    {
                        ID = new IdentifierType
                        {
                            schemeID = customer.IdentificationType,
                            Value = customer.IdentificationNumber
                        }
                    }
                },
                PostalAddress = new AddressType
                {
                    PostalZone = new TextType { Value = customer.PostalCode },
                    CountrySubentityCode = new CodeType { Value = "JO-AM" },
                    Country = new CountryType
                    {
                        IdentificationCode = new CodeType { Value = "JO" } // Country code for Jordan
                    }
                },
                PartyTaxScheme = new List<PartyTaxSchemeType>
                {
                    new PartyTaxSchemeType
                    {
                        CompanyID = new IdentifierType { Value = "1" },
                        TaxScheme = new TaxSchemeType
                        {
                            ID = new IdentifierType { Value = "VAT" }
                        }
                    }
                },
                PartyLegalEntity = new List<PartyLegalEntityType>
                {
                    new PartyLegalEntityType
                    {
                        RegistrationName = new NameType { Value = customer.Name }
                    }
                },
                Contact = new ContactType
                {
                    Telephone = new TextType { Value = customer.PhoneNumber ?? "" }
                }
            };

            ublInvoice.AccountingCustomerParty = new CustomerPartyType
            {
                Party = party,
                AccountingContact = new ContactType
                {
                    Telephone = new TextType { Value = customer.PhoneNumber }
                }
            };
        }

        private static void AddSellerSupplierParty(UblSharp.InvoiceType ublInvoice, Supplier supplier)
        {
            ublInvoice.SellerSupplierParty = new SupplierPartyType
            {
                Party = new PartyType
                {
                    PartyIdentification = new List<PartyIdentificationType>
                    {
                        new PartyIdentificationType
                        {
                            ID = new IdentifierType { Value = supplier.IncomeSourceSequence }
                        }
                    }
                }
            };
        }

        private static void AddMonetaryTotals(UblSharp.InvoiceType ublInvoice,
                                               InvoiceTotals totals,
                                               InvoiceType invoiceType)
        {
            // Allowance / discount block (unchanged)
            ublInvoice.AllowanceCharge = new List<AllowanceChargeType>
            {
                new AllowanceChargeType
                {
                    ChargeIndicator = new IndicatorType { Value = false },
                    AllowanceChargeReason = new List<TextType>
                    {
                        new TextType { Value = "discount" }
                    },
                    Amount = new AmountType
                    {
                        currencyID = "JO",
                        Value = FormatMonetaryValue(totals.TotalDiscountAmount)
                    }
                }
            };

            /*  VAT / Special taxes  */
            var taxTotals = new List<TaxTotalType>();

            if (HasVat(invoiceType))
            {
                taxTotals.Add(new TaxTotalType
                {
                    TaxAmount = new AmountType
                    {
                        currencyID = "JO",
                        Value = FormatMonetaryValue(totals.TotalVATAmount)
                    }
                });
            }

            if (HasSpecialTax(invoiceType))
            {
                taxTotals.Add(new TaxTotalType
                {
                    TaxAmount = new AmountType
                    {
                        currencyID = "JO",
                        Value = FormatMonetaryValue(totals.TotalSpecialTaxAmount)
                    }
                });
            }

            ublInvoice.TaxTotal = taxTotals;

            /*  Monetary totals – same as before  */
            ublInvoice.LegalMonetaryTotal = new MonetaryTotalType
            {
                TaxExclusiveAmount = new AmountType
                {
                    currencyID = "JO",
                    Value = FormatMonetaryValue(totals.TotalBeforeDiscount)
                },
                TaxInclusiveAmount = new AmountType
                {
                    currencyID = "JO",
                    Value = FormatMonetaryValue(totals.TotalInvoiceAmount)
                },
                AllowanceTotalAmount = new AmountType
                {
                    currencyID = "JO",
                    Value = FormatMonetaryValue(totals.TotalDiscountAmount)
                },
                PayableAmount = new AmountType
                {
                    currencyID = "JO",
                    Value = FormatMonetaryValue(totals.TotalInvoiceAmount)
                }
            };
        }

        private static void AddInvoiceLines(UblSharp.InvoiceType ublInvoice,
                                            List<InvoiceDetail> details,
                                            InvoiceType invoiceType)
        {
            ublInvoice.InvoiceLine = details
                .Select(d => CreateInvoiceLine(d, invoiceType))
                .ToList();
        }

        private static InvoiceLineType CreateInvoiceLine(InvoiceDetail detail, InvoiceType invoiceType)
        {
            var line = new InvoiceLineType
            {
                ID = new IdentifierType { Value = detail.ID },
                InvoicedQuantity = CreateQuantity(detail.Quantity),
                LineExtensionAmount = CreateAmount(detail.TotalBeforeTax),
                Item = CreateItem(detail.Description),
                Price = CreatePrice(detail)
            };

            var taxTotals = new List<TaxTotalType>();

            // 1. VAT
            if (HasVat(invoiceType))
            {
                taxTotals.Add(new TaxTotalType
                {
                    TaxAmount = CreateAmount(detail.TaxAmount),
                    RoundingAmount = CreateAmount(detail.TotalIncludingTax), // Added this line
                    TaxSubtotal = new List<TaxSubtotalType>
            {
                new TaxSubtotalType
                {
                    TaxAmount = CreateAmount(detail.TaxAmount),
                    TaxCategory = CreateTaxCategory(detail.TaxCategory)
                }
            }
                });
            }

            // 2. Special tax
            if (HasSpecialTax(invoiceType) && detail.SpecialTaxAmount.HasValue)
            {
                taxTotals.Add(new TaxTotalType
                {
                    TaxAmount = CreateAmount(detail.SpecialTaxAmount.Value),
                    TaxSubtotal = new List<TaxSubtotalType>
            {
                new TaxSubtotalType
                {
                    TaxAmount = CreateAmount(detail.SpecialTaxAmount.Value),
                    TaxCategory = CreateSpecialTaxCategory()
                }
            }
                });
            }

            line.TaxTotal = taxTotals;
            return line;
        }


        /*  ----------  helper to build the VAT category (unchanged)  ----------  */
        private static TaxCategoryType CreateTaxCategory(TaxCategoryCode taxCategory)
        {
            string code;
            switch (taxCategory)
            {
                case TaxCategoryCode.S:
                case TaxCategoryCode.S1:
                case TaxCategoryCode.S2:
                case TaxCategoryCode.S3:
                case TaxCategoryCode.S4:
                case TaxCategoryCode.S5:
                case TaxCategoryCode.S7:
                case TaxCategoryCode.S8:
                case TaxCategoryCode.S10:
                    code = "S";
                    break;

                case TaxCategoryCode.O:
                    code = "O";
                    break;

                case TaxCategoryCode.Z:
                    code = "Z";
                    break;

                default:
                    code = "O";
                    break;
            }

            return new TaxCategoryType
            {
                ID = new IdentifierType
                {
                    schemeAgencyID = "6",
                    schemeID = "UN/ECE 5305",
                    Value = code
                },
                Percent = new PercentType { Value = taxCategory.GetTaxPercent() },
                TaxScheme = new TaxSchemeType
                {
                    ID = new IdentifierType
                    {
                        schemeAgencyID = "6",
                        schemeID = "UN/ECE 5153",
                        Value = "VAT"
                    }
                }
            };
        }

        /*  ----------  helper to build the Special-tax category  ----------  */
        private static TaxCategoryType CreateSpecialTaxCategory()
        {
            return new TaxCategoryType
            {
                ID = new IdentifierType
                {
                    schemeAgencyID = "6",
                    schemeID = "UN/ECE 5305",
                    Value = "S"          // Jordan spec uses “S” for special tax
                },
                Percent = new PercentType { Value = 0m },   // fixed amount
                TaxScheme = new TaxSchemeType
                {
                    ID = new IdentifierType
                    {
                        schemeAgencyID = "6",
                        schemeID = "UN/ECE 5153",
                        Value = "ST"
                    }
                }
            };
        }

        private static QuantityType CreateQuantity(decimal quantity)
        {
            return new QuantityType { unitCode = "PCE", Value = quantity };
        }

        private static AmountType CreateAmount(decimal value, string currencyID = "JO")
        {
            return new AmountType { currencyID = currencyID, Value = FormatMonetaryValue(value) };
        }

        private static ItemType CreateItem(string description)
        {
            return new ItemType { Name = new NameType { Value = description } };
        }

        private static PriceType CreatePrice(InvoiceDetail detail)
        {
            return new PriceType
            {
                PriceAmount = CreateAmount(detail.UnitPriceBeforeTax),
                AllowanceCharge = CreateAllowanceCharge(detail.DiscountAmount)
            };
        }

        private static List<AllowanceChargeType> CreateAllowanceCharge(decimal? discountAmount)
        {
            return new List<AllowanceChargeType>
            {
                new AllowanceChargeType
                {
                    ChargeIndicator = new IndicatorType { Value = false },
                    AllowanceChargeReason = new List<TextType> { new TextType { Value = "DISCOUNT" } },
                    Amount = CreateAmount(discountAmount ?? 0.00m)
                }
            };
        }

        private static decimal FormatMonetaryValue(decimal value)
        {
            return Math.Round(value, 3, MidpointRounding.AwayFromZero);
        }
    }
}