using Com.Danliris.Service.Inventory.Lib.Helpers;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Com.Bateeq.Service.Pos.Lib.ViewModels.SalesDoc
{
    public class SalesDocViewModel : BasicViewModel, IValidatableObject
    {
        public DateTimeOffset date { get; set; }
        public string code { get; set; }
        public double discount { get; set; }
        public double grandTotal { get; set; }
        public SalesDetail salesDetail { get; set; }
        public double sisaBayar { get; set; }
        public Store store { get; set; }
        public int shift { get; set; }
        public double subTotal { get; set; }
        public double total { get; set; }
        public double totalBayar { get; set; }
        public double totalDiscount { get; set; }
        public double totalProduct { get; set; }
        public string reference { get; set; }
        public string remark { get; set; }
        public bool isReturn { get; set; }
        public bool isVoid { get; set; }
        public List<SalesDocDetailViewModel> items { get; set; }
        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (store == null || store.Id == 0)
            {
                yield return new ValidationResult("store is required", new List<string> { "storeId" });
            }
            //else
            //{
            //    var dateFrom = new DateTime(DateTime.Now.Day + (DateTime.Now.Month + 1) + DateTime.Now.Year  + store.Shift.datefrom.Hour + store.Shift.datefrom.Minute + store.Shift.datefrom.Second);
            //    var dateTo = new DateTime(DateTime.Now.Day + (DateTime.Now.Month + 1) + DateTime.Now.Year + store.Shift.dateto.Hour + store.Shift.dateto.Minute + store.Shift.dateto.Second);
            //    if (dateFrom > dateTo)
            //    {
            //        dateFrom = new DateTime(dateFrom.Day - 1);
            //    }
            //    if (dateFrom < DateTime.Now && DateTime.Now < dateTo)
            //    {
            //        shift = store.Shift.shift;
            //    }
            //}


            if (salesDetail == null)
            {
                yield return new ValidationResult("salesDetail is required", new List<string> { "salesDetail" });
            }
            if (string.IsNullOrWhiteSpace(salesDetail.paymentType))
            {
                yield return new ValidationResult("paymentType is required", new List<string> { "paymentType" });
            }

            if (salesDetail.paymentType == "Card" || salesDetail.paymentType == "Partial")
            {
                if (salesDetail.cardType == null || salesDetail.cardType._id == 0)
                {
                    yield return new ValidationResult("cardType is required", new List<string> { "cardType" });
                }

                if (salesDetail.bank == null || salesDetail.bank._id == 0)
                {
                    yield return new ValidationResult("bank is required", new List<string> { "bank" });
                }

                if (salesDetail.bankCard == null || salesDetail.bankCard._id == 0)
                {
                    yield return new ValidationResult("bankCard is required", new List<string> { "bankCard" });
                }

                if (string.IsNullOrWhiteSpace(salesDetail.cardNumber))
                {
                    yield return new ValidationResult("cardNumber is required", new List<string> { "cardNumber" });
                }

                if (string.IsNullOrWhiteSpace(salesDetail.cardName))
                {
                    yield return new ValidationResult("cardName is required", new List<string> { "cardName" });
                }
            }

            if (date == null)
            {
                yield return new ValidationResult("date is not valid", new List<string> { "date" });
            }

            int itemErrorCount = 0;
            if (this.items == null || items.Count <= 0)
            {
                yield return new ValidationResult("item is required", new List<string> { "itemscount" });
            }
            else
            {
                string itemError = "[";

                foreach (var item in items)
                {
                    itemError += "{";

                    if (item.item.ArticleRealizationOrder == null || item.item.ArticleRealizationOrder == "")
                    {
                        itemErrorCount++;
                        itemError += "RO: 'No RO selected', ";
                    }

                    if (item.quantity == 0)
                    {
                        itemErrorCount++;
                        itemError += "there is no item left ";
                    }

                    itemError += "}, ";
                    
                }

                itemError += "]";

                if (itemErrorCount > 0)
                    yield return new ValidationResult(itemError, new List<string> { "items" });
            }
        }
    }

    public class SalesDetail
    {
        public Bank bank { get; set; }
        public Bank bankCard { get; set; }
        public Card cardType { get; set; }
        public string card { get; set; }
        public double cardAmount { get; set; }
        public double cashAmount { get; set; }
        public string cardNumber { get; set; }
        public string cardName { get; set; }
        public string paymentType { get; set; }
        public string refund { get; set; }
        public Voucher voucher { get; set; }
    }
    public class Card
    {
        public int _id { get; set; }
        public string code { get; set; }
        public string description { get; set; }
        public string name { get; set; }
    }
    public class Bank
    {
        public int _id { get; set; }
        public string code { get; set; }
        public string description { get; set; }
        public string name { get; set; }
    }
    public class Voucher
    {
        public double value { get; set; }
    }
    public class Store
    {
        public int Id { get; set; }
        public string Address { get; set; }
        public string City { get; set; }
        public DateTimeOffset ClosedDate { get; set; }
        public string Code { get; set; }
        public string Description { get; set; }
        public float MonthlyTotalCost { get; set; }
        public string Name { get; set; }
        public string OnlineOffline { get; set; }
        public DateTimeOffset OpenedDate { get; set; }
        public string Pic { get; set; }
        public string Phone { get; set; }
        public float SalesCapital { get; set; }
        public string SalesCategory { get; set; }
        public float SalesTarget { get; set; }
        public string Status { get; set; }
        public string StoreArea { get; set; }
        public string StoreCategory { get; set; }
        public string StoreWide { get; set; }
        public StorageViewModel Storage { get; set; }
        public Shift Shift { get; set; }
        
    }
    public class Shift
    {
        public int shift { get; set; }
        public DateTimeOffset datefrom { get; set; }
        public DateTimeOffset dateto { get; set; }
    }
    public class StorageViewModel
    {
        public int _id { get; set; }
        public string address { get; set; }
        public string code { get; set; }
        public string description { get; set; }
        public bool isCentral { get; set; }
        public string name { get; set; }
        public string phone { get; set; }
    }
}
