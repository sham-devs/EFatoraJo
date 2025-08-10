using ShamDevs.EFatoraJo.Enums;
using System;
using System.Reflection;

namespace ShamDevs.EFatoraJo.Models
{
    public static class EnumExtensions
    {
        public static string GetStringValue(this Enum value)
        {
            Type type = value.GetType();
            FieldInfo fieldInfo = type.GetField(value.ToString());
            StringValueAttribute[]? attribs = fieldInfo.GetCustomAttributes(
                typeof(StringValueAttribute), false) as StringValueAttribute[];

            return attribs?.Length > 0 ? attribs[0].Value : value.ToString();
        }

        public static decimal GetTaxPercent(this TaxCategoryCode category)
        {
            return category switch
            {
                TaxCategoryCode.S => 16,
                TaxCategoryCode.S1 => 1,
                TaxCategoryCode.S2 => 2,
                TaxCategoryCode.S3 => 3,
                TaxCategoryCode.S4 => 4,
                TaxCategoryCode.S5 => 5,
                TaxCategoryCode.S7 => 7,
                TaxCategoryCode.S8 => 8,
                TaxCategoryCode.S10 => 10,
                _ => 0 // O, Z
            };
        }
    }
}