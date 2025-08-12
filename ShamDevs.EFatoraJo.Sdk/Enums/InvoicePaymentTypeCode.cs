using ShamDevs.EFatoraJo.Models;

namespace ShamDevs.EFatoraJo.Enums
{
    public enum InvoicePaymentTypeCode
    {
        // Income invoices (0xx)
        [StringValue("011")] LocalIncomeCash,
        [StringValue("021")] LocalIncomeCredit,
        [StringValue("111")] ExportIncomeCash,
        [StringValue("121")] ExportIncomeCredit,

        // General sales invoices (0xx)
        [StringValue("012")] LocalGeneralSalesCash,
        [StringValue("022")] LocalGeneralSalesCredit,
        [StringValue("112")] ExportGeneralSalesCash,
        [StringValue("122")] ExportGeneralSalesCredit,
        [StringValue("212")] DevelopmentAreaGeneralSalesCash,
        [StringValue("222")] DevelopmentAreaGeneralSalesCredit,

        // Special sales invoices (0xx)
        [StringValue("013")] LocalSpecialSalesCash,
        [StringValue("023")] LocalSpecialSalesCredit,
        [StringValue("113")] ExportSpecialSalesCash,
        [StringValue("123")] ExportSpecialSalesCredit,
        [StringValue("213")] DevelopmentAreaSpecialSalesCash,
        [StringValue("223")] DevelopmentAreaSpecialSalesCredit
    }
}
