using Com.Danliris.Service.Inventory.Lib;
using Com.Danliris.Service.Inventory.Lib.Services;
using Com.Bateeq.Service.Pos.Lib.Models.SalesReturn;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using Com.Danliris.Service.Inventory.Lib.Helpers;
using Newtonsoft.Json;
using Com.Moonlay.NetCore.Lib;
using Com.Bateeq.Service.Pos.Lib.ViewModels.SalesDocReturn;
using Com.Bateeq.Service.Pos.Lib.Models.SalesDoc;
using Com.Bateeq.Service.Pos.Lib.ViewModels.SalesDoc;
using System.Threading.Tasks;
using Com.Moonlay.Models;
using Com.Bateeq.Service.Pos.Lib.ViewModels.NewIntegrationViewModel;
using Com.Bateeq.Service.Pos.Lib.Interfaces;
using System.Net.Http;
using MongoDB.Bson;
using HashidsNet; 

namespace Com.Bateeq.Service.Pos.Lib.Services.SalesDocReturnService
{
    public class SalesDocReturnService : ISalesDocReturnService
    {
        private const string UserAgent = "salesreturn-service";
        protected DbSet<SalesDocReturn> DbSet;
        protected DbSet<SalesDocDetailReturnItem> DbSetSales;
        protected DbSet<SalesDoc> DbSetSalesDoc;
        public IIdentityService IdentityService;
        public readonly IServiceProvider ServiceProvider;
        public PosDbContext DbContext;
        public SalesDocReturnService(IServiceProvider serviceProvider, PosDbContext dbContext)
        {
            DbContext = dbContext;
            ServiceProvider = serviceProvider;
            DbSet = dbContext.Set<SalesDocReturn>();
            DbSetSalesDoc = dbContext.Set<SalesDoc>();
            DbSetSales = DbContext.Set<SalesDocDetailReturnItem>();
            IdentityService = serviceProvider.GetService<IIdentityService>();
        }
        public Tuple<List<SalesDocReturn>, int, Dictionary<string, string>, List<string>> ReadModel(string storecode, int Page = 1, int Size = 25, string Order = "{}", string Keyword = null, string Filter = "{}", string Username = "")
        {
            IQueryable<SalesDocReturn> Query = this.DbContext.SalesDocReturns.Where(x => x._CreatedBy == Username && x.StoreCode == storecode);

            List<string> SearchAttributes = new List<string>()
            {
                "Code"
            };

            Query = QueryHelper<SalesDocReturn>.Search(Query, SearchAttributes, Keyword);

            List<string> SelectedFields = new List<string>()
            {
                "Id", "Date", "Code", "StoreName"
            };

            Dictionary<string, object> FilterDictionary = JsonConvert.DeserializeObject<Dictionary<string, object>>(Filter);
            Query = QueryHelper<SalesDocReturn>.Filter(Query, FilterDictionary);

            Dictionary<string, string> OrderDictionary = JsonConvert.DeserializeObject<Dictionary<string, string>>(Order);
            Query = QueryHelper<SalesDocReturn>.Order(Query, OrderDictionary);

            Pageable<SalesDocReturn> pageable = new Pageable<SalesDocReturn>(Query, Page - 1, Size);
            List<SalesDocReturn> Data = pageable.Data.ToList<SalesDocReturn>();
            int TotalData = pageable.TotalCount;

            return Tuple.Create(Data, TotalData, OrderDictionary, SelectedFields);
        }
        public SalesDocReturn MapToModel(SalesDocReturnViewModel viewModel)
        {
            SalesDocReturn model = new SalesDocReturn();
            PropertyCopier<SalesDocReturnViewModel, SalesDocReturn>.Copy(viewModel, model);
            model.Date = viewModel.date;
            model.IsVoid = viewModel.isVoid;
            model.StoreCode = viewModel.store.Code;
            model.StoreId = viewModel.store.Id;
            model.StoreName = viewModel.store.Name;
            model.StoreStorageCode = viewModel.store.Storage.code;
            model.StoreStorageId = viewModel.store.Storage._id;
            model.StoreStorageName = viewModel.store.Storage.name;
            model.SalesDocCode = viewModel.sales.code;
            model.SalesDocId = viewModel.sales.Id;
            model.SalesDocIsReturn = viewModel.sales.isReturn;
            model.Details = new List<SalesDocReturnDetail>();
            foreach (SalesDocReturnDetailViewModel i in viewModel.items)
            {
                SalesDocReturnDetail salesDocDetail = new SalesDocReturnDetail();
                PropertyCopier<SalesDocReturnDetailViewModel, SalesDocReturnDetail>.Copy(i, salesDocDetail);
                salesDocDetail.Discount1 = i.discount1;
                salesDocDetail.Discount2 = i.discount2;
                salesDocDetail.DiscountNominal = i.discountNominal;
                salesDocDetail.isReturn = false;
                salesDocDetail.ItemArticleRealizationOrder = i.item.item.ArticleRealizationOrder;
                salesDocDetail.ItemCode = i.item.item.code;
                salesDocDetail.ItemDomesticCOGS = i.item.item.DomesticCOGS;
                salesDocDetail.ItemDomesticRetail = i.item.item.DomesticRetail;
                salesDocDetail.ItemDomesticSale = i.item.item.DomesticSale;
                salesDocDetail.ItemDomesticWholeSale = i.item.item.DomesticWholeSale;
                salesDocDetail.ItemId = i.item.item._id;
                salesDocDetail.ItemName = i.item.item.name;
                salesDocDetail.ItemSize = i.item.item.Size;
                salesDocDetail.ItemUom = i.item.item.Uom;
                salesDocDetail.Margin = i.margin;
                salesDocDetail.Price = i.price;
                salesDocDetail.Quantity = i.quantity;
                salesDocDetail.Size = i.item.item.Size;
                salesDocDetail.SpesialDiscount = i.specialDiscount;
                salesDocDetail.Total = i.total;

                model.Details.Add(salesDocDetail);
            }

            return model;

        }

        public SalesDocReturnViewModel MapToViewModel(SalesDocReturn model)
        {
            SalesDocReturnViewModel viewModel = new SalesDocReturnViewModel();
            PropertyCopier<SalesDocReturn, SalesDocReturnViewModel>.Copy(model, viewModel);
            viewModel.code = model.Code;
            viewModel.date = model.Date;
            viewModel.Id = model.Id;
            viewModel.isVoid = model.IsVoid;
            var sales = DbContext.Set<SalesDoc>().Include(x => x.Details).Where(x => x.Id == model.SalesDocId).FirstOrDefault();
            var salesitems = new List<SalesDocDetailViewModel>();
            foreach (SalesDocDetail i in sales.Details)
            {
                    SalesDocDetailViewModel salesDocDetailViewModel = new SalesDocDetailViewModel();
                    PropertyCopier<SalesDocDetail, SalesDocDetailViewModel>.Copy(i, salesDocDetailViewModel);
                    salesDocDetailViewModel.Id = i.Id;
                    salesDocDetailViewModel.item = new Bateeq.Service.Pos.Lib.ViewModels.NewIntegrationViewModel.ItemViewModel
                    {
                        ArticleRealizationOrder = i.ItemArticleRealizationOrder,
                        code = i.ItemCode,
                        DomesticCOGS = i.ItemDomesticCOGS,
                        DomesticRetail = i.ItemDomesticRetail,
                        DomesticSale = i.ItemDomesticSale,
                        DomesticWholeSale = i.ItemDomesticWholeSale,
                        name = i.ItemName,
                        Size = i.ItemSize,
                        Uom = i.ItemUom,
                        _id = i.ItemId
                    };
                    salesDocDetailViewModel.discount1 = i.Discount1;
                    salesDocDetailViewModel.discount2 = i.Discount2;
                    salesDocDetailViewModel.discountNominal = i.DiscountNominal;
                    salesDocDetailViewModel.itemCode = i.ItemCode;
                    salesDocDetailViewModel.itemId = (int)i.ItemId;
                    salesDocDetailViewModel.margin = i.Margin;
                    salesDocDetailViewModel.price = i.Price;
                    salesDocDetailViewModel.quantity = i.Quantity;
                    salesDocDetailViewModel.specialDiscount = i.SpesialDiscount;
                    salesDocDetailViewModel.total = i.Total;

                    salesitems.Add(salesDocDetailViewModel);
            }
            viewModel.sales = new Bateeq.Service.Pos.Lib.ViewModels.SalesDoc.SalesDocViewModel
            {
                Active = sales.Active,
                date = sales.Date,
                discount = sales.Discount,
                grandTotal = sales.GrandTotal,
                Id = sales.Id,
                reference = sales.Reference,
                remark = sales.Remark,
                code = sales.Code,
                isReturn = sales.isReturn,
                isVoid = sales.isVoid,
                salesDetail = new SalesDetail
                {
                    bank = new Bank
                    {
                        code = sales.BankCode,
                        description = "",
                        name = sales.BankName,
                        _id = sales.BankId
                    },
                    bankCard = new Bank
                    {
                        code = sales.BankCardCode,
                        description = "",
                        name = sales.BankCardName,
                        _id = sales.BankCardId
                    },
                    cardAmount = sales.CardAmount,
                    cardName = sales.CardName,
                    cardNumber = sales.CardNumber,
                    cardType = new Card
                    {
                        code = sales.CardTypeCode,
                        description = "",
                        name = sales.CardTypeName,
                        _id = sales.CardTypeId
                    },
                    cashAmount = sales.CashAmount,
                    paymentType = sales.PaymentType,
                    voucher = new Voucher
                    {
                        value = sales.VoucherValue
                    },
                    card = sales.Card
                },
                store = new Store
                {
                    Code = sales.StoreCode,
                    Name = sales.StoreName,
                    Id = sales.StoreId,

                    Storage = new StorageViewModel
                    {
                        code = sales.StoreStorageCode,
                        name = model.StoreStorageName,
                        _id = model.StoreStorageId,

                    }
                },
                shift = sales.Shift,
                subTotal = sales.SubTotal,
                totalProduct = sales.TotalProduct,
                items = salesitems

            };
            var salesreturn = DbContext.Set<SalesDoc>().Include(x => x.Details).Where(x => x.Id == model.SalesDocReturnId).FirstOrDefault();
            var salesreturnitems = new List<SalesDocDetailViewModel>();
            foreach (SalesDocDetail i in salesreturn.Details)
            {
                SalesDocDetailViewModel salesDocDetailViewModel = new SalesDocDetailViewModel();
                PropertyCopier<SalesDocDetail, SalesDocDetailViewModel>.Copy(i, salesDocDetailViewModel);
                salesDocDetailViewModel.Id = i.Id;
                salesDocDetailViewModel.item = new Bateeq.Service.Pos.Lib.ViewModels.NewIntegrationViewModel.ItemViewModel
                {
                    ArticleRealizationOrder = i.ItemArticleRealizationOrder,
                    code = i.ItemCode,
                    DomesticCOGS = i.ItemDomesticCOGS,
                    DomesticRetail = i.ItemDomesticRetail,
                    DomesticSale = i.ItemDomesticSale,
                    DomesticWholeSale = i.ItemDomesticWholeSale,
                    name = i.ItemName,
                    Size = i.ItemSize,
                    Uom = i.ItemUom,
                    _id = i.ItemId
                };
                salesDocDetailViewModel.discount1 = i.Discount1;
                salesDocDetailViewModel.discount2 = i.Discount2;
                salesDocDetailViewModel.discountNominal = i.DiscountNominal;
                salesDocDetailViewModel.itemCode = i.ItemCode;
                salesDocDetailViewModel.itemId = (int)i.ItemId;
                salesDocDetailViewModel.margin = i.Margin;
                salesDocDetailViewModel.price = i.Price;
                salesDocDetailViewModel.quantity = i.Quantity;
                salesDocDetailViewModel.specialDiscount = i.SpesialDiscount;
                salesDocDetailViewModel.total = i.Total;
                salesDocDetailViewModel.isReturn = i.isReturn;
                salesDocDetailViewModel.returnItems = new List<SalesDocDetailViewModel>();
                if (i.isReturn)
                {
                    var a = DbContext.SalesDocDetailReturnItems.Where(x => x.SalesDocDetailId == i.Id).ToList();
                    foreach (var retur in a)
                    {
                        SalesDocDetailViewModel salesDocDetailreturnitemViewModel = new SalesDocDetailViewModel();
                        PropertyCopier<SalesDocDetail, SalesDocDetailViewModel>.Copy(i, salesDocDetailreturnitemViewModel);
                        salesDocDetailreturnitemViewModel.Id = retur.Id;
                        salesDocDetailreturnitemViewModel.item = new Bateeq.Service.Pos.Lib.ViewModels.NewIntegrationViewModel.ItemViewModel
                        {
                            ArticleRealizationOrder = retur.ItemArticleRealizationOrder,
                            code = retur.ItemCode,
                            DomesticCOGS = retur.ItemDomesticCOGS,
                            DomesticRetail = retur.ItemDomesticRetail,
                            DomesticSale = retur.ItemDomesticSale,
                            DomesticWholeSale = retur.ItemDomesticWholeSale,
                            name = retur.ItemName,
                            Size = retur.ItemSize,
                            Uom = retur.ItemUom,
                            _id = retur.ItemId
                        };
                        salesDocDetailreturnitemViewModel.discount1 = retur.Discount1;
                        salesDocDetailreturnitemViewModel.discount2 = retur.Discount2;
                        salesDocDetailreturnitemViewModel.discountNominal = retur.DiscountNominal;
                        salesDocDetailreturnitemViewModel.itemCode = retur.ItemCode;
                        salesDocDetailreturnitemViewModel.itemId = (int)retur.ItemId;
                        salesDocDetailreturnitemViewModel.margin = retur.Margin;
                        salesDocDetailreturnitemViewModel.price = retur.Price;
                        salesDocDetailreturnitemViewModel.quantity = retur.Quantity;
                        salesDocDetailreturnitemViewModel.specialDiscount = retur.SpesialDiscount;
                        salesDocDetailreturnitemViewModel.total = retur.Total;

                        salesDocDetailViewModel.returnItems.Add(salesDocDetailreturnitemViewModel);
                    }
                }
                salesreturnitems.Add(salesDocDetailViewModel);
            }
            viewModel.salesDocReturn = new SalesDocViewModel
            {
                Active = salesreturn.Active,
                date = salesreturn.Date,
                discount = salesreturn.Discount,
                grandTotal = salesreturn.GrandTotal,
                Id = salesreturn.Id,
                reference = salesreturn.Reference,
                remark = salesreturn.Remark,
                code = salesreturn.Code,
                isReturn = salesreturn.isReturn,
                isVoid = salesreturn.isVoid,
                salesDetail = new SalesDetail
                {
                    bank = new Bank
                    {
                        code = salesreturn.BankCode,
                        description = "",
                        name = salesreturn.BankName,
                        _id = salesreturn.BankId
                    },
                    bankCard = new Bank
                    {
                        code = salesreturn.BankCardCode,
                        description = "",
                        name = salesreturn.BankCardName,
                        _id = salesreturn.BankCardId
                    },
                    cardAmount = salesreturn.CardAmount,
                    cardName = salesreturn.CardName,
                    cardNumber = salesreturn.CardNumber,
                    cardType = new Card
                    {
                        code = salesreturn.CardTypeCode,
                        description = "",
                        name = salesreturn.CardTypeName,
                        _id = salesreturn.CardTypeId
                    },
                    cashAmount = salesreturn.CashAmount,
                    paymentType = salesreturn.PaymentType,
                    voucher = new Voucher
                    {
                        value = salesreturn.VoucherValue
                    },
                    card = salesreturn.Card
                },
                store = new Store
                {
                    Code = salesreturn.StoreCode,
                    Name = salesreturn.StoreName,
                    Id = salesreturn.StoreId,

                    Storage = new StorageViewModel
                    {
                        code = salesreturn.StoreStorageCode,
                        name = salesreturn.StoreStorageName,
                        _id = salesreturn.StoreStorageId,

                    }
                },
                shift = salesreturn.Shift,
                subTotal = salesreturn.SubTotal,
                totalProduct = salesreturn.TotalProduct,
                totalBayar = salesreturn.GrandTotal,
                totalDiscount = salesreturn.Discount,
                total = salesreturn.GrandTotal,
                items = salesreturnitems
            };
            viewModel._CreatedAgent = model._CreatedAgent;
            viewModel._CreatedBy = model._CreatedBy;
            viewModel._CreatedUtc = model._CreatedUtc;
            viewModel._IsDeleted = model._IsDeleted;
            viewModel._LastModifiedAgent = model._LastModifiedAgent;
            viewModel._LastModifiedUtc = model._LastModifiedUtc;
            viewModel._LastModifiedBy = model._LastModifiedBy;
            var store = GetStore(salesreturn.StoreCode);
            viewModel.store = new Store
            {
                Id = salesreturn.StoreId,
                Code = salesreturn.StoreCode,
                Name = salesreturn.StoreName,
                Address = store.address,
                Phone = store.phone
                
            };
            viewModel.items = new List<SalesDocReturnDetailViewModel>();
            foreach(var i in model.Details)
            {
                SalesDocReturnDetailViewModel salesDocDetailViewModel = new SalesDocReturnDetailViewModel();
                PropertyCopier<SalesDocReturnDetail, SalesDocReturnDetailViewModel>.Copy(i, salesDocDetailViewModel);
                salesDocDetailViewModel.Id = i.Id;
                salesDocDetailViewModel.Active = i.Active;
                salesDocDetailViewModel.discount1 = i.Discount1;
                salesDocDetailViewModel.discount2 = i.Discount2;
                salesDocDetailViewModel.discountNominal = i.DiscountNominal;
                salesDocDetailViewModel.isReturn = i.isReturn;
                salesDocDetailViewModel.itemCode = i.ItemCode;
                salesDocDetailViewModel.itemId = (int)i.ItemId;
                salesDocDetailViewModel.item = new SalesDocDetailViewModel
                {
                    discount1 = i.Discount1,
                    discount2 = i.Discount2,
                    discountNominal = i.DiscountNominal,
                    itemCode = i.ItemCode,
                    itemId = (int)i.ItemId,
                    margin = i.Margin,
                    price = i.Price,
                    quantity = i.Quantity,
                    specialDiscount = i.SpesialDiscount,
                    total = i.Total,
                    item = new Bateeq.Service.Pos.Lib.ViewModels.NewIntegrationViewModel.ItemViewModel
                    {
                        ArticleRealizationOrder = i.ItemArticleRealizationOrder,
                        code = i.ItemCode,
                        DomesticCOGS = i.ItemDomesticCOGS,
                        DomesticRetail = i.ItemDomesticRetail,
                        DomesticSale = i.ItemDomesticSale,
                        DomesticWholeSale = i.ItemDomesticWholeSale,
                        name = i.ItemName,
                        Size = i.ItemSize,
                        Uom = i.ItemUom,
                        _id = i.ItemId

                    }
                };
                salesDocDetailViewModel.margin = i.Margin;
                salesDocDetailViewModel.price = i.Price;
                salesDocDetailViewModel.quantity = i.Quantity;
                salesDocDetailViewModel.specialDiscount = i.SpesialDiscount;
                salesDocDetailViewModel.total = i.Total;
                salesDocDetailViewModel.returnItems = new List<SalesDocDetailViewModel>();



                foreach(var d in salesreturn.Details)
                {
                    SalesDocDetailViewModel salesDocDetailreturnViewModel = new SalesDocDetailViewModel();
                    PropertyCopier<SalesDocDetail, SalesDocDetailViewModel>.Copy(d, salesDocDetailreturnViewModel);

                    salesDocDetailreturnViewModel.Id = d.Id;
                    salesDocDetailreturnViewModel.item = new Bateeq.Service.Pos.Lib.ViewModels.NewIntegrationViewModel.ItemViewModel
                    {
                        ArticleRealizationOrder = d.ItemArticleRealizationOrder,
                        code = d.ItemCode,
                        DomesticCOGS = d.ItemDomesticCOGS,
                        DomesticRetail = d.ItemDomesticRetail,
                        DomesticSale = d.ItemDomesticSale,
                        DomesticWholeSale = d.ItemDomesticWholeSale,
                        name = d.ItemName,
                        Size = d.ItemSize,
                        Uom = d.ItemUom,
                        _id = d.ItemId
                    };
                    salesDocDetailreturnViewModel.discount1 = d.Discount1;
                    salesDocDetailreturnViewModel.discount2 = d.Discount2;
                    salesDocDetailreturnViewModel.discountNominal = d.DiscountNominal;
                    salesDocDetailreturnViewModel.itemCode = d.ItemCode;
                    salesDocDetailreturnViewModel.itemId = (int)d.ItemId;
                    salesDocDetailreturnViewModel.margin = d.Margin;
                    salesDocDetailreturnViewModel.price = d.Price;
                    salesDocDetailreturnViewModel.quantity = d.Quantity;
                    salesDocDetailreturnViewModel.specialDiscount = d.SpesialDiscount;
                    salesDocDetailreturnViewModel.total = d.Total;
                    

                    salesDocDetailViewModel.returnItems.Add(salesDocDetailreturnViewModel);
                }

                viewModel.items.Add(salesDocDetailViewModel);

            }
            return viewModel;

        }

        public SalesDocViewModel MaptoViewModel(SalesDoc model)
        {
            SalesDocViewModel viewModel = new SalesDocViewModel();
            PropertyCopier<SalesDoc, SalesDocViewModel>.Copy(model, viewModel);
            var salesdoc = DbContext.Set<SalesDoc>().Where(x => x.Id == model.Id).FirstOrDefault();
            viewModel.Active = model.Active;
            viewModel.date = model.Date;
            viewModel.discount = model.Discount;
            viewModel.grandTotal = model.GrandTotal;
            viewModel.Id = model.Id;
            viewModel.reference = model.Reference;
            viewModel.remark = model.Remark;
            viewModel.code = model.Code;
            viewModel.isReturn = model.isReturn;
            viewModel.isVoid = model.isVoid;
            viewModel.salesDetail = new SalesDetail
            {
                bank = new Bank
                {
                    code = model.BankCode,
                    description = "",
                    name = model.BankName,
                    _id = model.BankId
                },
                bankCard = new Bank
                {
                    code = model.BankCardCode,
                    description = "",
                    name = model.BankCardName,
                    _id = model.BankCardId
                },
                cardAmount = model.CardAmount,
                cardName = model.CardName,
                cardNumber = model.CardNumber,
                cardType = new Card
                {
                    code = model.CardTypeCode,
                    description = "",
                    name = model.CardTypeName,
                    _id = model.CardTypeId
                },
                cashAmount = model.CashAmount,
                paymentType = model.PaymentType,
                voucher = new Voucher
                {
                    value = model.VoucherValue
                },
                card = model.Card
            };
            var Store = GetStore(salesdoc.StoreCode);
            viewModel.store = new Store
            {
                Code = model.StoreCode,
                Name = model.StoreName,
                Id = model.StoreId,
                Address = Store.address,
                Phone = Store.phone,
                StoreCategory = model.StoreCategory,

                Storage = new StorageViewModel
                {
                    code = model.StoreStorageCode,
                    name = model.StoreStorageName,
                    _id = model.StoreStorageId,

                }
            };
            viewModel.shift = model.Shift;
            viewModel.subTotal = model.SubTotal;
            viewModel.totalProduct = model.TotalProduct;
            viewModel.grandTotal = model.GrandTotal;
            viewModel.totalDiscount = model.Discount;

            viewModel.Id = model.Id;
            viewModel.items = new List<SalesDocDetailViewModel>();
            foreach (SalesDocDetail i in model.Details)
            {
                SalesDocDetailViewModel salesDocDetailViewModel = new SalesDocDetailViewModel();
                PropertyCopier<SalesDocDetail, SalesDocDetailViewModel>.Copy(i, salesDocDetailViewModel);
                salesDocDetailViewModel.Id = i.Id;
                salesDocDetailViewModel.item = new ViewModels.NewIntegrationViewModel.ItemViewModel
                {
                    ArticleRealizationOrder = i.ItemArticleRealizationOrder,
                    code = i.ItemCode,
                    DomesticCOGS = i.ItemDomesticCOGS,
                    DomesticRetail = i.ItemDomesticRetail,
                    DomesticSale = i.ItemDomesticSale,
                    DomesticWholeSale = i.ItemDomesticWholeSale,
                    name = i.ItemName,
                    Size = i.ItemSize,
                    Uom = i.ItemUom,
                    _id = i.ItemId
                };
                salesDocDetailViewModel.discount1 = i.Discount1;
                salesDocDetailViewModel.discount2 = i.Discount2;
                salesDocDetailViewModel.discountNominal = i.DiscountNominal;
                salesDocDetailViewModel.itemCode = i.ItemCode;
                salesDocDetailViewModel.itemId = (int)i.ItemId;
                salesDocDetailViewModel.margin = i.Margin;
                salesDocDetailViewModel.price = i.Price;
                salesDocDetailViewModel.quantity = i.Quantity;
                salesDocDetailViewModel.specialDiscount = i.SpesialDiscount;
                salesDocDetailViewModel.total = i.Total;


                viewModel.items.Add(salesDocDetailViewModel);
            }
            return viewModel;

        }

        public string GenerateCode(string ModuleId)
        {
            var uid = ObjectId.GenerateNewId().ToString();
            var hashids = new Hashids(uid, 8, "ABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890");
            var now = DateTime.Now;
            var begin = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
            var diff = (now - begin).Milliseconds;
            string code = String.Format("{0}/{1}/{2}", hashids.Encode(diff), ModuleId, DateTime.Now.ToString("MM/yyyy"));
            return code;
        }

        public List<SalesDoc> SalesReturnReport(string storecode, DateTimeOffset dateFrom, DateTimeOffset dateTo, string shift)
        {
            var a = DbSetSalesDoc.Where(m => m.StoreCode == storecode && m.Date.Date >= dateFrom.Date && m.Date.Date <= dateTo.Date && m.Shift == (string.IsNullOrWhiteSpace(shift) ? m.Shift : Convert.ToInt32(shift)) && m.isReturn == true)
                    .Include(m => m.Details);

            return a.ToList();
        }

        //public List<SalesDocDetailReturnItem> ReadSalesReturnItem(int id)
        //{
        //    var b = DbSetSales.Where(m => m.SalesDocDetailId == id);
        //    return b.ToList();
        //}

        public async Task<int> Create(SalesDocReturn model, SalesDocReturnViewModel viewModel)
        {
            int Created = 0;
            using (var transaction = this.DbContext.Database.BeginTransaction())
            {
                try
                {
                    string code = GenerateCode("sales");
                    SalesDoc sales = new SalesDoc();
                    sales.Remark = viewModel.remark;
                    if(viewModel.salesDetail.bankCard != null)
                    {
                        sales.BankCardCode = viewModel.salesDetail.bankCard.code;
                        sales.BankCardId = viewModel.salesDetail.bankCard._id;
                        sales.BankCardName = viewModel.salesDetail.bankCard.name;
                    }
                    if (viewModel.salesDetail.bank != null)
                    {
                        sales.BankCode = viewModel.salesDetail.bank.code;
                        sales.BankId = viewModel.salesDetail.bank._id;
                        sales.BankName = viewModel.salesDetail.bank.name;
                    }
                    sales.CardAmount = viewModel.salesDetail.cardAmount;
                    sales.CardName = viewModel.salesDetail.cardName;
                    sales.CardNumber = viewModel.salesDetail.cardNumber;
                    if (viewModel.salesDetail.cardType != null)
                    {
                        sales.CardTypeCode = viewModel.salesDetail.cardType.code;
                        sales.CardTypeId = viewModel.salesDetail.cardType._id;
                        sales.CardTypeName = viewModel.salesDetail.cardType.name;
                    }
                    if (viewModel.salesDetail.voucher != null)
                    {
                        sales.VoucherValue = viewModel.salesDetail.voucher.value;
                    }
                    sales.CashAmount = viewModel.salesDetail.cashAmount;
                    sales.Date = viewModel.date;
                    sales.Discount = 0;
                    sales.Card = viewModel.salesDetail.card;
                    sales.PaymentType = viewModel.salesDetail.paymentType;
                    sales.isVoid = false;
                    sales.Shift = viewModel.shift;
                    sales.StoreCode = viewModel.store.Code;
                    sales.StoreId = viewModel.store.Id;
                    sales.StoreName = viewModel.store.Name;
                    sales.StoreStorageCode = viewModel.store.Storage.code;
                    sales.StoreStorageId = viewModel.store.Storage._id;
                    sales.StoreStorageName = viewModel.store.Storage.name;
                    sales.SubTotal = viewModel.subTotal;
                    sales.TotalProduct = viewModel.totalProduct;
                    sales.GrandTotal = viewModel.total;
                    sales.Discount = viewModel.totalDiscount;
                    


                    List<SalesDocDetail> docDetails = new List<SalesDocDetail>();
                    foreach(var i in viewModel.items)
                    {
                        docDetails.Add(new SalesDocDetail
                        {
                            Discount1 = i.discount1,
                            Discount2 = i.discount2,
                            DiscountNominal = i.discountNominal,
                            isReturn = true,
                            ItemArticleRealizationOrder = i.item.item.ArticleRealizationOrder,
                            ItemCode = i.item.item.code,
                            ItemDomesticCOGS = i.item.item.DomesticCOGS,
                            ItemDomesticRetail = i.item.item.DomesticRetail,
                            ItemDomesticSale = i.item.item.DomesticSale,
                            ItemDomesticWholeSale = i.item.item.DomesticWholeSale,
                            ItemId = i.item.item._id,
                            ItemName = i.item.item.name,
                            ItemSize = i.item.item.Size,
                            ItemUom = i.item.item.Uom,
                            Margin = i.margin,
                            Price = i.price,
                            Quantity = i.quantity,
                            Size = i.item.item.Size,
                            SpesialDiscount = i.specialDiscount,
                            Total = i.total
                            
                            
                        });
                        foreach(var retur in i.returnItems)
                        {
                            docDetails.Add(new SalesDocDetail
                            {
                                Discount1 = retur.discount1,
                                Discount2 = retur.discount2,
                                DiscountNominal = retur.discountNominal,
                                isReturn = false,
                                ItemArticleRealizationOrder = retur.item.ArticleRealizationOrder,
                                ItemCode = retur.item.code,
                                ItemDomesticCOGS = retur.item.DomesticCOGS,
                                ItemDomesticRetail = retur.item.DomesticRetail,
                                ItemDomesticSale = retur.item.DomesticSale,
                                ItemDomesticWholeSale = retur.item.DomesticWholeSale,
                                ItemId = retur.item._id,
                                ItemName = retur.item.name,
                                ItemSize = retur.item.Size,
                                ItemUom = retur.item.Uom,
                                Margin = retur.margin,
                                Price = retur.price,
                                Quantity = retur.quantity,
                                Size = retur.item.Size,
                                SpesialDiscount = retur.specialDiscount,
                                Total = retur.total
                            });
                        }
                    }
                    //foreach(var i in sales.Details)
                    //{
                    //    if(viewModel.items.Where(x=>x.item.code == i.ItemCode).Count() > 0)
                    //    {
                    //        if(viewModel.items.Single().returnItems.Count() > 0)
                    //        {
                    //            i.isReturn = true;
                    //        }
                    //    }
                    //}
                    sales.Details = docDetails;
                    sales.isReturn = true;
                    sales.Code = code;
                    sales.FlagForCreate(IdentityService.Username, UserAgent);
                    sales.FlagForUpdate(IdentityService.Username, UserAgent);

                    sales.isVoid = false;

                    var salesreturn = await AddSales(sales);

                    model.SalesDocReturnCode = salesreturn.Code;
                    model.SalesDocReturnId = salesreturn.Id;
                    model.SalesDocReturnIsReturn = sales.isReturn;
                    model.Code = sales.Code;
                    model.FlagForCreate(IdentityService.Username, UserAgent);
                    model.FlagForUpdate(IdentityService.Username, UserAgent);
                    foreach(var item in model.Details)
                    {
                        item.FlagForCreate(IdentityService.Username, UserAgent);
                        item.FlagForUpdate(IdentityService.Username, UserAgent);
                    }

                    DbSet.Add(model);
                    Created = await DbContext.SaveChangesAsync();

                    List<SalesDocDetailReturnItem> docDetail = new List<SalesDocDetailReturnItem>();
                    foreach (var i in viewModel.items)
                    {
                        var t = sales.Details.Where(x => x.ItemCode == i.item.item.code && x.ItemArticleRealizationOrder == i.item.item.ArticleRealizationOrder).Single();
                        foreach(var d in i.returnItems)
                        {
                            docDetail.Add(new SalesDocDetailReturnItem {
                                Discount1 = d.discount1,
                                Discount2 = d.discount2,
                                DiscountNominal = d.discountNominal,
                                isReturn = true,
                                ItemArticleRealizationOrder = d.item.ArticleRealizationOrder,
                                ItemCode = d.item.code,
                                ItemDomesticCOGS = d.item.DomesticCOGS,
                                ItemDomesticRetail = d.item.DomesticRetail,
                                ItemDomesticSale = d.item.DomesticSale,
                                ItemDomesticWholeSale = d.item.DomesticWholeSale,
                                ItemId = d.item._id,
                                ItemName = d.item.name,
                                ItemSize = d.item.Size,
                                ItemUom = d.item.Uom,
                                Margin = d.margin,
                                Price = d.price,
                                Quantity = d.quantity,
                                Size = d.item.Size,
                                SpesialDiscount = d.specialDiscount,
                                Total = d.total,
                                SalesDocDetailId = t.Id,
                                
                            });
                        }
                    }

                    Created += await AddReturnItems(docDetail);
                    transaction.Commit();
                }
                catch(Exception e)
                {
                    transaction.Rollback();
                    throw new Exception(e.Message);
                }

                
            }
            return model.Id;

        }



        public async Task<int> AddReturnItems(List<SalesDocDetailReturnItem> model)
        {
            int Created = 0;
            foreach(var m in model)
            {
                m.FlagForCreate(IdentityService.Username, UserAgent);
                m.FlagForUpdate(IdentityService.Username, UserAgent);

                DbSetSales.Add(m);
            }

             Created = await DbContext.SaveChangesAsync();
            return Created;
        }
        public async Task<SalesDoc> AddSales(SalesDoc model)
        {

            int Created = 0;
            TransferOutDocViewModel transferOutDocViewModel = new TransferOutDocViewModel();
            List<TransferOutDocItemViewModel> transferOutDocItemViewModels = new List<TransferOutDocItemViewModel>();
            TransferInDocViewModel transferInDocView = new TransferInDocViewModel();
            List<TransferInDocItemViewModel> transferInDocItemViews = new List<TransferInDocItemViewModel>();
            transferOutDocViewModel.code = model.Code;
            transferOutDocViewModel.reference = model.Code;
            transferOutDocViewModel.source = new SourceViewModel
            {
                _id = model.StoreStorageId,
                code = model.StoreStorageCode,
                name = model.StoreStorageName
            };
            transferOutDocViewModel.destination = new DestinationViewModel
            {
                _id = model.StoreStorageId,
                code = model.StoreStorageCode,
                name = model.StoreStorageName
            };
            transferOutDocViewModel.remark = model.Remark;
            bool isAnyTransferIn = false;
            transferInDocView.code = model.Code;
            transferInDocView.reference = model.Code;
            transferInDocView.source = new SourceViewModel
            {
                _id = model.StoreStorageId,
                code = model.StoreStorageCode,
                name = model.StoreStorageName
            };
            transferInDocView.destination = new DestinationViewModel
            {
                _id = model.StoreStorageId,
                code = model.StoreStorageCode,
                name = model.StoreStorageName
            };
            transferInDocView.remark = model.Remark;
            foreach (var item in model.Details)
            {
                if (!item.isReturn)
                {
                    transferOutDocItemViewModels.Add(new TransferOutDocItemViewModel
                    {
                        articleRealizationOrder = item.ItemArticleRealizationOrder,
                        remark = model.Remark,
                        quantity = item.Quantity,
                        item = new ViewModels.NewIntegrationViewModel.ItemViewModels
                        {
                            articleRealizationOrder = item.ItemArticleRealizationOrder,
                            code = item.ItemCode,
                            domesticCOGS = item.ItemDomesticCOGS,
                            domesticRetail = item.ItemDomesticRetail,
                            domesticSale = item.ItemDomesticSale,
                            domesticWholesale = item.ItemDomesticWholeSale,
                            name = item.ItemName,
                            size = item.ItemSize,
                            uom = item.ItemUom,
                            _id = item.ItemId,


                        }
                    });

                }
                else if (item.isReturn)
                {
                    transferInDocItemViews.Add(new TransferInDocItemViewModel
                    {
                        remark = model.Remark,
                        sendquantity = item.Quantity,
                        item = new ViewModels.NewIntegrationViewModel.ItemViewModels
                        {
                            articleRealizationOrder = item.ItemArticleRealizationOrder,
                            code = item.ItemCode,
                            domesticCOGS = item.ItemDomesticCOGS,
                            domesticRetail = item.ItemDomesticRetail,
                            domesticSale = item.ItemDomesticSale,
                            domesticWholesale = item.ItemDomesticWholeSale,
                            name = item.ItemName,
                            size = item.ItemSize,
                            uom = item.ItemUom,
                            _id = item.ItemId,


                        }
                    });
                    isAnyTransferIn = true;
                }
                item.FlagForCreate(IdentityService.Username, UserAgent);
                item.FlagForUpdate(IdentityService.Username, UserAgent);
            }
            transferOutDocViewModel.items = transferOutDocItemViewModels;
            transferInDocView.items = transferInDocItemViews;

            string warehouseUri = "transfer-out/pos";
            var httpClient = (IHttpClientService)ServiceProvider.GetService(typeof(IHttpClientService));
            var response = await httpClient.PostAsync($"{APIEndpoint.Warehouse}{warehouseUri}", new StringContent(JsonConvert.SerializeObject(transferOutDocViewModel).ToString(), Encoding.UTF8, General.JsonMediaType));

            response.EnsureSuccessStatusCode();

            if (isAnyTransferIn)
            {
                string warehouseUritransferin = "transfer-in/for-pos";
                var httpClienttarnsfer = (IHttpClientService)ServiceProvider.GetService(typeof(IHttpClientService));
                var responsetransfer = await httpClienttarnsfer.PostAsync($"{APIEndpoint.Warehouse}{warehouseUritransferin}", new StringContent(JsonConvert.SerializeObject(transferInDocView).ToString(), Encoding.UTF8, General.JsonMediaType));

                responsetransfer.EnsureSuccessStatusCode();
            }

            

            DbSetSalesDoc.Add(model);

            Created = await DbContext.SaveChangesAsync();
            return model;
        }
        public SalesDocReturn ReadModelById(int id)
        {
            var a = DbSet.Where(m => m.Id == id)
                    .Include(m => m.Details)
                    .FirstOrDefault();
            return a;
        }
        public StoreViewModels GetStore(string storeCode)
        {
            //string storeUri = "master/stores/code";
            IHttpClientService httpClient = (IHttpClientService)ServiceProvider.GetService(typeof(IHttpClientService));
            if (httpClient != null)
            {
                var storeUri = APIEndpoint.Core + $"master/stores/code";
                string queryUri = $"?code={storeCode}";
                string uri = storeUri + queryUri;
                var response = httpClient.GetAsync($"{uri}").Result.Content.ReadAsStringAsync();
                Dictionary<string, object> result = JsonConvert.DeserializeObject<Dictionary<string, object>>(response.Result);
                StoreViewModels viewModel = JsonConvert.DeserializeObject<StoreViewModels>(result.GetValueOrDefault("data").ToString());
                //viewModel = viewModel.Where(x => x.name == (string.IsNullOrWhiteSpace(storeCode) ? x.name : storeCode)).ToList();
                return viewModel;
            }
            else
            {
                StoreViewModels viewModel = null;
                return viewModel;
            }

        }
    }
}
