using Com.Danliris.Service.Inventory.Lib.Helpers;
using System;
using System.Collections.Generic;
using System.Text;

namespace Com.Bateeq.Service.Pos.Lib.ViewModels.SalesDoc
{
    public class PaymentMethodReportViewModel : BasicViewModel
    {
        public string Code { get; set; }
        public DateTimeOffset Date { get; set; }
        public string BankName { get; set; }
		public string BankCard { get; set; }
		public string Card { get; set; }
        public double CardAmount { get; set; }
        public string CardTypeName { get; set; }
        public bool IsVoid { get; set; }
        public double VoucherValue { get; set; }
        public double GrandTotal { get; set; }
        public int Shift { get; set; }
        public double CashAmount { get; set; }
        public string PaymnetType { get; set; }
        public double SubTotal { get; set; }
    }
}
