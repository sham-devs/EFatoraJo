using ShamDevs.EFatoraJo.Enums;
using System;
using System.Collections.Generic;
using System.Globalization;

namespace ShamDevs.EFatoraJo.Utilities
{
    public static class CurrencyHelper
    {
        private static readonly Dictionary<CurrencyCode, int> CurrencyPrecision = new Dictionary<CurrencyCode, int>
    {
        { CurrencyCode.JOD, 3 },  // Jordanian Dinar (3 decimal places)
        { CurrencyCode.USD, 2 },  // US Dollar (2 decimal places)
        { CurrencyCode.EUR, 2 },  // Euro (2 decimal places)
        { CurrencyCode.SAR, 2 },  // Saudi Riyal (2 decimal places)
        { CurrencyCode.AED, 2 },  // UAE Dirham (2 decimal places)
        { CurrencyCode.OMR, 3 },  // Omani Rial (3 decimal places)
        { CurrencyCode.GBP, 2 },  // British Pound (2 decimal places)
        { CurrencyCode.QAR, 2 },  // Qatari Riyal (2 decimal places)
        { CurrencyCode.KWD, 3 },  // Kuwaiti Dinar (3 decimal places)
        { CurrencyCode.BHD, 3 },  // Bahraini Dinar (3 decimal places)
        { CurrencyCode.AUD, 2 },  // Australian Dollar (2 decimal places)
        { CurrencyCode.CAD, 2 },  // Canadian Dollar (2 decimal places)
        { CurrencyCode.JPY, 0 },  // Japanese Yen (0 decimal places)
        { CurrencyCode.CHF, 2 },  // Swiss Franc (2 decimal places)
        { CurrencyCode.TRY, 2 },  // Turkish Lira (2 decimal places)
        { CurrencyCode.SYP, 2 },  // Syrian Pound (2 decimal places)
        { CurrencyCode.EGP, 2 }   // Egyptian Pound (2 decimal places)
    };

        public static int GetPrecision(CurrencyCode currency)
        {
            return CurrencyPrecision.TryGetValue(currency, out int precision)
                ? precision
                : 2; // Default to 2 decimal places if currency not found
        }

        public static decimal Round(decimal value, CurrencyCode currency)
        {
            int precision = GetPrecision(currency);

            // 1. Round to the required number of decimals
            decimal rounded = Math.Round(value, precision, MidpointRounding.AwayFromZero);

            // 2. Force the exact precision by re-parsing a formatted string
            string format = new string('0', precision) == string.Empty
                            ? "0"
                            : "0." + new string('0', precision);

            return decimal.Parse(rounded.ToString(format, CultureInfo.InvariantCulture),
                                 CultureInfo.InvariantCulture);
        }
    }
}
