using ShamDevs.EFatoraJo.Enums;
using ShamDevs.EFatoraJo.Models;
using ShamDevs.EFatoraJo.Utilities;
using System;
using System.Collections.Generic;
using System.Linq; // Required for LINQ operations
using UblSharp.CommonAggregateComponents;
using UblSharp.UnqualifiedDataTypes;

namespace ShamDevs.EFatoraJo.Services
{
    public static class ReturnInvoiceGeneratorService
    {
        #region helpers
        private static bool HasVat(InvoiceType type) => type != InvoiceType.Income;
        private static bool HasSpecialTax(InvoiceType type) => type == InvoiceType.SpecialSales;
        #endregion

        public static UblSharp.InvoiceType GenerateReturnUBL21(SalesReturnInvoice returnInvoice)
        {
            var originalInvoice = returnInvoice.ReturnedInvoice;
            var ublReturnInvoice = new UblSharp.InvoiceType();

            AddBasicReturnDetails(ublReturnInvoice, returnInvoice);
            AddBillingReference(ublReturnInvoice, originalInvoice);
            AddSupplierParty(ublReturnInvoice, originalInvoice.Supplier);
            AddCustomerParty(ublReturnInvoice, originalInvoice.Customer);
            AddSellerSupplierParty(ublReturnInvoice, originalInvoice.Supplier);
            AddMonetaryTotals(ublReturnInvoice, originalInvoice.InvoiceTotals, originalInvoice.Type, originalInvoice.Currency);
            AddInvoiceLines(ublReturnInvoice, originalInvoice.InvoiceDetails, originalInvoice.Type, originalInvoice.Currency);
            AddReturnReason(ublReturnInvoice, returnInvoice.ReturnReason);

            return ublReturnInvoice;
        }

        private static void AddBasicReturnDetails(UblSharp.InvoiceType ublInvoice, SalesReturnInvoice returnInvoice)
        {
            ublInvoice.ID = new IdentifierType { Value = returnInvoice.InvoiceNumber };
            ublInvoice.UUID = new IdentifierType { Value = returnInvoice.UniqueSerialNumber };
            ublInvoice.IssueDate = new DateType { Value = DateTime.Parse(returnInvoice.InvoiceDate) };
            ublInvoice.InvoiceTypeCode = new CodeType { Value = "381", name = returnInvoice.ReturnedInvoice.InvoiceTypeCode };
            ublInvoice.DocumentCurrencyCode = new CodeType { Value = returnInvoice.ReturnedInvoice.CurrencyCode };
            ublInvoice.TaxCurrencyCode = new CodeType { Value = returnInvoice.ReturnedInvoice.CurrencyCode };
            ublInvoice.AdditionalDocumentReference = new List<DocumentReferenceType>
            {
                new DocumentReferenceType
                {
                    ID = new IdentifierType { Value = "ICV" },
                    UUID = new IdentifierType { Value = Guid.NewGuid().ToString() }
                }
            };
        }

        private static void AddBillingReference(UblSharp.InvoiceType ublInvoice, Invoice originalInvoice)
        {
            ublInvoice.BillingReference = new List<BillingReferenceType>
            {
                new BillingReferenceType
                {
                    InvoiceDocumentReference = new DocumentReferenceType
                    {
                        ID = new IdentifierType { Value = originalInvoice.InvoiceNumber },
                        UUID = new IdentifierType { Value = originalInvoice.UniqueSerialNumber },
                        DocumentDescription = new List<TextType>
                        {
                            new TextType { Value = originalInvoice.InvoiceTotals.TotalInvoiceAmount.ToString("0.00") }
                        }
                    }
                }
            };
        }

        private static void AddReturnReason(UblSharp.InvoiceType ublInvoice, string returnReason)
        {
            ublInvoice.PaymentMeans = new List<PaymentMeansType>
            {
                new PaymentMeansType
                {
                    PaymentMeansCode = new CodeType { Value = "10", listID = "UN/ECE 4461" },
                    InstructionNote = new List<TextType>
                    {
                        new TextType { Value = returnReason }
                    }
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
                                              InvoiceType invoiceType,
                                              CurrencyCode currency)
        {
            // Discount block
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
                        Value = FormatMonetaryValue(totals.TotalDiscountAmount,currency)
                    }
                }
            };

            /* VAT / Special taxes */
            var taxTotals = new List<TaxTotalType>();

            if (HasVat(invoiceType))
            {
                taxTotals.Add(new TaxTotalType
                {
                    TaxAmount = new AmountType
                    {
                        currencyID = "JO",
                        Value = FormatMonetaryValue(totals.TotalVATAmount, currency)
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
                        Value = FormatMonetaryValue(totals.TotalSpecialTaxAmount, currency)
                    }
                });
            }

            ublInvoice.TaxTotal = taxTotals;

            /* Monetary totals */
            ublInvoice.LegalMonetaryTotal = new MonetaryTotalType
            {
                TaxExclusiveAmount = new AmountType
                {
                    currencyID = "JO",
                    Value = FormatMonetaryValue(totals.TotalBeforeDiscount, currency)
                },
                TaxInclusiveAmount = new AmountType
                {
                    currencyID = "JO",
                    Value = FormatMonetaryValue(totals.TotalInvoiceAmount, currency)
                },
                AllowanceTotalAmount = new AmountType
                {
                    currencyID = "JO",
                    Value = FormatMonetaryValue(totals.TotalDiscountAmount, currency)
                },
                PayableAmount = new AmountType
                {
                    currencyID = "JO",
                    Value = FormatMonetaryValue(totals.TotalInvoiceAmount, currency)
                }
            };
        }

        private static void AddInvoiceLines(UblSharp.InvoiceType ublInvoice,
                                            List<InvoiceDetail> details,
                                            InvoiceType invoiceType,
                                            CurrencyCode currency)
        {
            ublInvoice.InvoiceLine = details
                .Select(d => CreateInvoiceLine(d, invoiceType, currency))
                .ToList();
        }

        private static InvoiceLineType CreateInvoiceLine(InvoiceDetail detail,
                                                         InvoiceType invoiceType,
                                                         CurrencyCode currency)
        {
            var line = new InvoiceLineType
            {
                ID = new IdentifierType { Value = detail.ID },
                InvoicedQuantity = CreateQuantity(detail.Quantity),
                LineExtensionAmount = CreateAmount(detail.TotalBeforeTax, currency),
                Item = CreateItem(detail.Description),
                Price = CreatePrice(detail, currency)
            };

            var taxTotals = new List<TaxTotalType>();

            /* VAT */
            if (HasVat(invoiceType))
            {
                taxTotals.Add(new TaxTotalType
                {
                    TaxAmount = CreateAmount(detail.TaxAmount, currency),
                    RoundingAmount = CreateAmount(detail.TotalIncludingTax, currency),
                    TaxSubtotal = new List<TaxSubtotalType>
                    {
                        new TaxSubtotalType
                        {
                            TaxAmount = CreateAmount(detail.TaxAmount, currency),
                            TaxCategory = CreateTaxCategory(detail.TaxCategory)
                        }
                    }
                });
            }

            /* Special Tax */
            if (HasSpecialTax(invoiceType) && detail.SpecialTaxAmount.HasValue)
            {
                taxTotals.Add(new TaxTotalType
                {
                    TaxAmount = CreateAmount(detail.SpecialTaxAmount.Value, currency),
                    TaxSubtotal = new List<TaxSubtotalType>
                    {
                        new TaxSubtotalType
                        {
                            TaxAmount = CreateAmount(detail.SpecialTaxAmount.Value, currency),
                            TaxCategory = CreateSpecialTaxCategory()
                        }
                    }
                });
            }

            line.TaxTotal = taxTotals;
            return line;
        }


        private static InvoiceLineType CreateInvoiceLine(InvoiceDetail detail, CurrencyCode currency)
        {
            return new InvoiceLineType
            {
                ID = new IdentifierType { Value = detail.ID },
                InvoicedQuantity = CreateQuantity(detail.Quantity),
                LineExtensionAmount = CreateAmount(detail.TotalBeforeTax, currency),
                TaxTotal = CreateTaxTotal(detail, currency),
                Item = CreateItem(detail.Description),
                Price = CreatePrice(detail, currency)
            };
        }

        private static QuantityType CreateQuantity(decimal quantity)
        {
            return new QuantityType
            {
                unitCode = "PCE",
                Value = quantity
            };
        }

        private static AmountType CreateAmount(decimal value, CurrencyCode currency, string currencyID = "JO")
        {
            return new AmountType
            {
                currencyID = currencyID,
                Value = FormatMonetaryValue(value, currency)
            };
        }

        private static List<TaxTotalType> CreateTaxTotal(InvoiceDetail detail, CurrencyCode currency)
        {
            return new List<TaxTotalType>
            {
                new TaxTotalType
                {
                    TaxAmount = CreateAmount(detail.TaxAmount,currency),
                    RoundingAmount = CreateAmount(detail.TotalIncludingTax, currency),
                    TaxSubtotal = CreateTaxSubtotal(detail, currency)
                }
            };
        }

        private static List<TaxSubtotalType> CreateTaxSubtotal(InvoiceDetail detail, CurrencyCode currency)
        {
            return new List<TaxSubtotalType>
            {
                new TaxSubtotalType
                {
                    TaxAmount = CreateAmount(detail.TaxAmount, currency),
                    TaxCategory = CreateTaxCategory(detail.TaxCategory)
                }
            };
        }

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
                Percent = new PercentType
                {
                    Value = taxCategory.GetTaxPercent()
                },
                TaxScheme = CreateTaxScheme()
            };
        }

        private static TaxCategoryType CreateSpecialTaxCategory()
        {
            return new TaxCategoryType
            {
                ID = new IdentifierType
                {
                    schemeAgencyID = "6",
                    schemeID = "UN/ECE 5305",
                    Value = "S"
                },
                Percent = new PercentType { Value = 0m },
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

        private static TaxSchemeType CreateTaxScheme()
        {
            return new TaxSchemeType
            {
                ID = new IdentifierType
                {
                    schemeAgencyID = "6",
                    schemeID = "UN/ECE 5153",
                    Value = "VAT"
                }
            };
        }

        private static ItemType CreateItem(string description)
        {
            return new ItemType
            {
                Name = new NameType { Value = description }
            };
        }

        private static PriceType CreatePrice(InvoiceDetail detail, CurrencyCode currency)
        {
            return new PriceType
            {
                PriceAmount = CreateAmount(detail.UnitPriceBeforeTax, currency),
                AllowanceCharge = CreateAllowanceCharge(detail.DiscountAmount, currency)
            };
        }

        private static List<AllowanceChargeType> CreateAllowanceCharge(decimal? discountAmount, CurrencyCode currency)
        {
            return new List<AllowanceChargeType>
            {
                new AllowanceChargeType
                {
                    ChargeIndicator = new IndicatorType { Value = false },
                    AllowanceChargeReason = new List<TextType>
                    {
                        new TextType { Value = "DISCOUNT" }
                    },
                    Amount = CreateAmount(discountAmount ?? 0.00m, currency)
                }
            };
        }

        // Helper method to format monetary values as #.00
        private static decimal FormatMonetaryValue(decimal value, CurrencyCode currency)
        {
            return CurrencyHelper.Round(value, currency);
        }
    }
}