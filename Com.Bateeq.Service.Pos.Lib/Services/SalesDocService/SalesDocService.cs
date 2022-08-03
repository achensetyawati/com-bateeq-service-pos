using Com.Danliris.Service.Inventory.Lib;
using Com.Danliris.Service.Inventory.Lib.Helpers;
using Com.Danliris.Service.Inventory.Lib.Services;
using Com.Bateeq.Service.Pos.Lib.Services.SalesDocService;
using Com.Bateeq.Service.Pos.Lib.ViewModels.NewIntegrationViewModel;
using Com.Bateeq.Service.Pos.Lib.Interfaces;
using Com.Bateeq.Service.Pos.Lib.Models.SalesDoc;
using Com.Bateeq.Service.Pos.Lib.ViewModels.SalesDoc;
using Com.Moonlay.Models;
using Com.Moonlay.NetCore.Lib;
using HashidsNet;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Bson;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.IO;
using System.Globalization;
using Com.Bateeq.Service.Pos.Lib.ViewModels;

namespace Com.Bateeq.Service.Pos.Lib.Services.SalesDocService
{
    public class SalesDocService : ISalesDocService
    {
        private const string UserAgent = "sales-service";
        protected DbSet<SalesDoc> DbSet;
        public IIdentityService IdentityService;
        public readonly IServiceProvider ServiceProvider;
        public PosDbContext DbContext;
        public SalesDocService(IServiceProvider serviceProvider, PosDbContext dbContext)
        {
            DbContext = dbContext;
            ServiceProvider = serviceProvider;
            DbSet = dbContext.Set<SalesDoc>();
            IdentityService = serviceProvider.GetService<IIdentityService>();
        }
        public Tuple<List<SalesDoc>, int, Dictionary<string, string>, List<string>> ReadModel(string storecode,int Page = 1, int Size = 25, string Order = "{}", string Keyword = null, string Filter = "{}", string Username = "")
        {
            IQueryable<SalesDoc> Query = this.DbContext.SalesDocs.Where(x => x._CreatedBy == Username && x.StoreCode == storecode);

            List<string> SearchAttributes = new List<string>()
            {
                "Code"
            };

            Query = QueryHelper<SalesDoc>.Search(Query, SearchAttributes, Keyword);

            List<string> SelectedFields = new List<string>()
            {
                "Id", "Date", "Code", "StoreName"
            };

           

            Dictionary<string, object> FilterDictionary = JsonConvert.DeserializeObject<Dictionary<string, object>>(Filter);
            Query = QueryHelper<SalesDoc>.Filter(Query, FilterDictionary);

            Dictionary<string, string> OrderDictionary = JsonConvert.DeserializeObject<Dictionary<string, string>>(Order);
            Query = QueryHelper<SalesDoc>.Order(Query, OrderDictionary);

            Pageable<SalesDoc> pageable = new Pageable<SalesDoc>(Query, Page - 1, Size);
            List<SalesDoc> Data = pageable.Data.ToList<SalesDoc>();
            int TotalData = pageable.TotalCount;

            return Tuple.Create(Data, TotalData, OrderDictionary, SelectedFields);
        }
        public Tuple<List<SalesDoc>, int, Dictionary<string, string>, List<string>> ReadModelVoid(string storecode, int Page = 1, int Size = 25, string Order = "{}", string Keyword = null, string Filter = "{}", string Username = "")
        {
            //IQueryable<SalesDoc> Query = this.DbContext.SalesDocs.Where(x => x._CreatedBy == Username && x.isVoid == false && x.StoreCode == storecode);
            IQueryable<SalesDoc> Query = DbSet.Where(x => x._CreatedBy == Username && x.isVoid == false && x.StoreCode == storecode)
                .Include(x => x.Details);
            List<string> SearchAttributes = new List<string>()
            {
                "Code"
            };

            Query = QueryHelper<SalesDoc>.Search(Query, SearchAttributes, Keyword);

            List<string> SelectedFields = new List<string>()
            {
                "Id", "Date", "Code", "StoreName", "GrandTotal", "_CreatedBy"
            };

            Dictionary<string, object> FilterDictionary = JsonConvert.DeserializeObject<Dictionary<string, object>>(Filter);
            Query = QueryHelper<SalesDoc>.Filter(Query, FilterDictionary);

            Dictionary<string, string> OrderDictionary = JsonConvert.DeserializeObject<Dictionary<string, string>>(Order);
            Query = QueryHelper<SalesDoc>.Order(Query, OrderDictionary);

            Pageable<SalesDoc> pageable = new Pageable<SalesDoc>(Query, Page - 1, Size);
            List<SalesDoc> Data = pageable.Data.ToList<SalesDoc>();
            int TotalData = pageable.TotalCount;

            return Tuple.Create(Data, TotalData, OrderDictionary, SelectedFields);
        }
        public Tuple<List<SalesDoc>, int, Dictionary<string, string>, List<string>> ReadModelReturn(string storecode, int Page = 1, int Size = 25, string Order = "{}", string Keyword = null, string Filter = "{}", string Username = "")
        {
            //IQueryable<SalesDoc> Query = this.DbContext.SalesDocs.Where(x => x._CreatedBy == Username && x.isVoid == false && x.StoreCode == storecode);
            IQueryable<SalesDoc> Query = DbSet.Where(x => x._CreatedBy == Username && x.isReturn == false && x.StoreCode == storecode)
                .Include(x => x.Details);
            List<string> SearchAttributes = new List<string>()
            {
                "Code"
            };

            Query = QueryHelper<SalesDoc>.Search(Query, SearchAttributes, Keyword);

            List<string> SelectedFields = new List<string>()
            {
                "Id", "Date", "Code", "StoreName", "GrandTotal", "_CreatedBy"
            };

            Dictionary<string, object> FilterDictionary = JsonConvert.DeserializeObject<Dictionary<string, object>>(Filter);
            Query = QueryHelper<SalesDoc>.Filter(Query, FilterDictionary);

            Dictionary<string, string> OrderDictionary = JsonConvert.DeserializeObject<Dictionary<string, string>>(Order);
            Query = QueryHelper<SalesDoc>.Order(Query, OrderDictionary);

            Pageable<SalesDoc> pageable = new Pageable<SalesDoc>(Query, Page - 1, Size);
            List<SalesDoc> Data = pageable.Data.ToList<SalesDoc>();
            int TotalData = pageable.TotalCount;

            return Tuple.Create(Data, TotalData, OrderDictionary, SelectedFields);
        }
        public SalesDoc ReadModelById(int id)
        {
            var a = DbSet.Where(m => m.Id == id)
                    .Include(m => m.Details)
                    .FirstOrDefault();
            return a;
        }
        public List<SalesDoc> OmzetReport(string storecode, DateTimeOffset dateFrom, DateTimeOffset dateTo, string shift)
        {
            var a = DbSet.Where(m =>m.isVoid== false && m.StoreCode == storecode && m.Date.Date >= dateFrom.Date && m.Date.Date <= dateFrom.Date && m.Shift == (string.IsNullOrWhiteSpace(shift) ? m.Shift : Convert.ToInt32(shift)))
                    .Include(m => m.Details);

            return a.ToList();
        }
        public SalesDoc ReadModelByCode(string code, string storecode)
        {
            var a = DbSet.Where(m => m.Code == code && m.StoreStorageCode == storecode)
                    .Include(m => m.Details)
                    .FirstOrDefault();
            return a;
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
        public SalesDoc MapToModel(SalesDocViewModel viewModel)
        {
            SalesDoc model = new SalesDoc();
            PropertyCopier<SalesDocViewModel, SalesDoc>.Copy(viewModel, model);
            if(viewModel.salesDetail.bankCard != null)
            {
                model.BankCardCode = viewModel.salesDetail.bankCard.code;
                model.BankCardId = viewModel.salesDetail.bankCard._id;
                model.BankCardName = viewModel.salesDetail.bankCard.name;
            }
            if(viewModel.salesDetail.bank != null)
            {
                model.BankCode = viewModel.salesDetail.bank.code;
                model.BankId = viewModel.salesDetail.bank._id;
                model.BankName = viewModel.salesDetail.bank.name;
            }
            
            model.CardAmount = viewModel.salesDetail.cardAmount;
            model.CardName = viewModel.salesDetail.cardName;
            model.CardNumber = viewModel.salesDetail.cardNumber;
            if(viewModel.salesDetail.cardType != null)
            {
                model.CardTypeCode = viewModel.salesDetail.cardType.code;
                model.CardTypeId = viewModel.salesDetail.cardType._id;
                model.CardTypeName = viewModel.salesDetail.cardType.name;
            }
            model.CashAmount = viewModel.salesDetail.cashAmount;
            model.Card = viewModel.salesDetail.card;
            model.Date = viewModel.date;
            model.Discount = viewModel.discount;
            model.GrandTotal = viewModel.grandTotal;
            model.isReturn = false;
            model.isVoid = false;
            model.PaymentType = viewModel.salesDetail.paymentType;
            model.Reference = viewModel.reference;
            model.Remark = viewModel.remark;
            model.Shift = viewModel.shift;
            model.StoreCode = viewModel.store.Code;
            model.StoreId = viewModel.store.Id;
            model.StoreName = viewModel.store.Name;
            model.StoreCategory = viewModel.store.StoreCategory;
            model.StoreStorageCode = viewModel.store.Storage.code;
            model.StoreStorageId = viewModel.store.Storage._id;
            model.StoreStorageName = viewModel.store.Storage.name;
            model.SubTotal = viewModel.subTotal;
            model.TotalProduct = viewModel.totalProduct;
            model.VoucherValue = viewModel.salesDetail.voucher.value;

            model.Details = new List<SalesDocDetail>();

            foreach (SalesDocDetailViewModel i in viewModel.items)
            {
                SalesDocDetail salesDocDetail = new SalesDocDetail();
                PropertyCopier<SalesDocDetailViewModel, SalesDocDetail>.Copy(i, salesDocDetail);
                salesDocDetail.Discount1 = i.discount1;
                salesDocDetail.Discount2 = i.discount2;
                salesDocDetail.DiscountNominal = i.discountNominal;
                salesDocDetail.isReturn = false;
                salesDocDetail.ItemArticleRealizationOrder = i.item.ArticleRealizationOrder;
                salesDocDetail.ItemCode = i.item.code;
                salesDocDetail.ItemDomesticCOGS = i.item.DomesticCOGS;
                salesDocDetail.ItemDomesticRetail = i.item.DomesticRetail;
                salesDocDetail.ItemDomesticSale = i.item.DomesticSale;
                salesDocDetail.ItemDomesticWholeSale = i.item.DomesticWholeSale;
                salesDocDetail.ItemId = i.item._id;
                salesDocDetail.ItemName = i.item.name;
                salesDocDetail.ItemSize = i.item.Size;
                salesDocDetail.ItemUom = i.item.Uom;
                salesDocDetail.Margin = i.margin;
                salesDocDetail.Price = i.price;
                salesDocDetail.Quantity = i.quantity;
                salesDocDetail.Size = i.item.Size;
                salesDocDetail.SpesialDiscount = i.specialDiscount;
                salesDocDetail.Total = i.total;

                
                model.Details.Add(salesDocDetail);
            }
            return model;
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

        public async Task<SalesDoc> Create(SalesDoc model)
        {
            int Created = 0;

            using (var transaction = this.DbContext.Database.BeginTransaction())
            {
                try
                {
                    
                    string code = GenerateCode("sales");
                    model.Code = code;
                    model.FlagForCreate(IdentityService.Username, UserAgent);
                    model.FlagForUpdate(IdentityService.Username, UserAgent);
                    if(model.isReturn != true)
                    {
                        model.isReturn = false;
                    }
                    
                    model.isVoid = false;
                    
                    
                    TransferOutDocViewModel transferOutDocViewModel = new TransferOutDocViewModel();
                    List<TransferOutDocItemViewModel> transferOutDocItemViewModels = new List<TransferOutDocItemViewModel>();
                    TransferInDocViewModel transferInDocView = new TransferInDocViewModel();
                    List<TransferInDocItemViewModel> transferInDocItemViews = new List<TransferInDocItemViewModel>(); 
                    transferOutDocViewModel.code = code;
                    transferOutDocViewModel.reference = code;
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
                    transferInDocView.code = code;
                    transferInDocView.reference = code;
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
                                item = new ItemViewModels
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
                        else if(item.isReturn)
                        {
                            transferInDocItemViews.Add(new TransferInDocItemViewModel
                            {
                                remark = model.Remark,
                                sendquantity = item.Quantity,
                                item = new ItemViewModels
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

                    if (isAnyTransferIn)
                    {
                        string warehouseUritransferin = "transfer-in/for-pos";
                        var httpClienttarnsfer = (IHttpClientService)ServiceProvider.GetService(typeof(IHttpClientService));
                        var responsetransfer = await httpClienttarnsfer.PostAsync($"{APIEndpoint.Warehouse}{warehouseUritransferin}", new StringContent(JsonConvert.SerializeObject(transferInDocView).ToString(), Encoding.UTF8, General.JsonMediaType));

                        responsetransfer.EnsureSuccessStatusCode();
                    }

                    string warehouseUri = "transfer-out/pos";
                    var httpClient = (IHttpClientService)ServiceProvider.GetService(typeof(IHttpClientService));
                    var response = await httpClient.PostAsync($"{APIEndpoint.Warehouse}{warehouseUri}", new StringContent(JsonConvert.SerializeObject(transferOutDocViewModel).ToString(), Encoding.UTF8, General.JsonMediaType));

                    response.EnsureSuccessStatusCode();

                    model.FlagForCreate(IdentityService.Username, UserAgent);
                    model.FlagForUpdate(IdentityService.Username, UserAgent);

                    DbSet.Add(model);
                    Created = await DbContext.SaveChangesAsync();
                    Created = model.Id;
                    transaction.Commit();
                }
                catch (Exception e)
                {
                    transaction.Rollback();
                    throw new Exception(e.Message);
                }


            }

            return model;
        }

        public async Task<int> Void(int id, string user, int clientTimeZoneOffset = 7)
        {
            int Updated = 0;

            using (var transaction = this.DbContext.Database.BeginTransaction())
            {
                try
                {
                    var oldM = DbSet.Where(m => m.Id == id)
                    .Include(m => m.Details)
                    .FirstOrDefault();

                    if (oldM != null && oldM.Id == id)
                    {
                        oldM.isVoid = true;
                        oldM.FlagForUpdate(IdentityService.Username, UserAgent);

                        TransferInDocViewModel transferInDocView = new TransferInDocViewModel();
                        List<TransferInDocItemViewModel> transferInDocItems = new List<TransferInDocItemViewModel>();

                        foreach(var i in oldM.Details)
                        {
                            if (!i.isReturn)
                            {
                                transferInDocItems.Add(new TransferInDocItemViewModel
                                {
                                    item = new ItemViewModels
                                    {
                                        articleRealizationOrder = i.ItemArticleRealizationOrder,
                                        code = i.ItemCode,
                                        domesticCOGS = i.ItemDomesticCOGS,
                                        domesticRetail = i.ItemDomesticRetail,
                                        domesticSale = i.ItemDomesticSale,
                                        domesticWholesale = i.ItemDomesticWholeSale,
                                        name = i.ItemName,
                                        size = i.ItemSize,
                                        uom = i.ItemUom,
                                        _id = i.ItemId
                                    },
                                    remark = oldM.Remark,
                                    sendquantity = i.Quantity
                                });
                            }
                        }

                        transferInDocView.code = GenerateCode("voidsales");
                        transferInDocView.destination = new DestinationViewModel
                        {
                            code = oldM.StoreStorageCode,
                            name = oldM.StoreStorageName,
                            _id = oldM.StoreStorageId
                        };
                        transferInDocView.reference = oldM.Code;
                        transferInDocView.source = new SourceViewModel
                        {
                            code = oldM.StoreStorageCode,
                            name = oldM.StoreStorageName,
                            _id = oldM.StoreStorageId
                        };
                        transferInDocView.items = transferInDocItems;
                        string warehouseUri = "transfer-in/for-pos";
                        var httpClient = (IHttpClientService)ServiceProvider.GetService(typeof(IHttpClientService));
                        var response = await httpClient.PostAsync($"{APIEndpoint.Warehouse}{warehouseUri}", new StringContent(JsonConvert.SerializeObject(transferInDocView).ToString(), Encoding.UTF8, General.JsonMediaType));

                        response.EnsureSuccessStatusCode();

                        //if(oldM.isReturn == true)
                        //{

                        //}
                    }

                    Updated = await DbContext.SaveChangesAsync();
                    transaction.Commit();
                }
                catch (Exception e)
                {
                    transaction.Rollback();
                    throw new Exception(e.Message);
                }
            }
            return Updated;
        }

        public IQueryable<PaymentMethodReportViewModel> GetPaymentReportQuery(string storecode, DateTimeOffset dateFrom, DateTimeOffset dateTo, string shift)
        {
            var Query = (from c in DbContext.SalesDocs
                         where c._IsDeleted == false
                         //&& c.StorageId == (string.IsNullOrWhiteSpace(storageId) ? c.StorageId : storageId)
                         && c.Date.Date >= dateFrom.Date
                         && c.Date.Date <= dateTo.Date
                         && c.StoreCode == (string.IsNullOrWhiteSpace(storecode) ? c.StoreCode : storecode)
                         && c.Shift == (string.IsNullOrWhiteSpace(shift) ? c.Shift : Convert.ToInt32(shift))
                         //&& a.ItemName == (string.IsNullOrWhiteSpace(info) ? a.ItemName : info)

                         select new PaymentMethodReportViewModel
                         {
                             Code = c.Code == "" ? "-" : c.Code,
							 BankCard = c.BankCardName,
                             Date = c.Date,
                             BankName = c.BankName == "" ? "-" : c.BankName,
                             Card = c.Card == "" ? "-" : c.Card,
                             CardAmount = c.CardAmount,
                             CardTypeName = c.CardTypeName == "" ? "-" : c.CardTypeName,
                             IsVoid = c.isVoid,
                             VoucherValue = c.VoucherValue,
                             GrandTotal = c.GrandTotal,
                             Shift = c.Shift,
                             CashAmount = c.CashAmount,
                             PaymnetType = c.PaymentType == "" ? "-" : c.PaymentType,
                             SubTotal = c.SubTotal
                         });


            return Query;
        }

        public Tuple<List<PaymentMethodReportViewModel>, int> GetPaymentMethodReport(string storecode, DateTimeOffset dateFrom, DateTimeOffset dateTo, string shift, string info, int offset, string username, int page = 1, int size = 25, string Order = "{}")
        {
            var Query = GetPaymentReportQuery(storecode, dateFrom, dateTo, shift);

            Dictionary<string, string> OrderDictionary = JsonConvert.DeserializeObject<Dictionary<string, string>>(Order);


            // Pageable<InventoriesReportViewModel> pageable = new Pageable<InventoriesReportViewModel>(Query, page - 1, size);
            List<PaymentMethodReportViewModel> Data = Query.ToList<PaymentMethodReportViewModel>();
            // int TotalData = pageable.TotalCount;

            return Tuple.Create(Data, Data.Count());

        }

        #region SalesVoidReport
        public IQueryable<SalesVoidReportViewModel> GetSalesVoidReportQuery(string storeCode, DateTimeOffset dateFrom, DateTimeOffset dateTo, string shift)
        {
            var Query = (from a in DbContext.SalesDocs
                         join b in DbContext.SalesDocDetails on a.Id equals b.SalesDocId
                         where a._IsDeleted == false
                         && b._IsDeleted == false
                         && a.isVoid == true
                         //&& c.StorageId == (string.IsNullOrWhiteSpace(storageId) ? c.StorageId : storageId)
                         && a.Date.Date >= dateFrom.Date
                         && a.Date.Date <= dateTo.Date
                         && a.StoreCode == (string.IsNullOrWhiteSpace(storeCode) ? a.StoreCode : storeCode)
                         && a.Shift == (string.IsNullOrWhiteSpace(shift) ? a.Shift : Convert.ToInt32(shift))
                         //&& a.ItemName == (string.IsNullOrWhiteSpace(info) ? a.ItemName : info)

                         select new SalesVoidReportViewModel
                         {
                             _CreatedBy = a._CreatedBy,
                             _LastModifiedBy = a._LastModifiedBy,
                             _LastModifiedUtc = a._LastModifiedUtc,
                             date = a.Date,
                             code = a.Code,
                             grandTotal = a.GrandTotal,
                             storeId = a.StoreId,
                             storeCode = a.StoreCode,
                             storeName = a.StoreName,
                             ItemId = b.ItemId,
                             ItemCode = b.ItemCode,
                             ItemName = b.ItemName,
                             ItemSize = b.ItemSize,
                             Quantity = b.Quantity,
                             TotalPrice = b.Total,
                             shift = a.Shift,
                             reference = a.Reference,
                             remark = a.Remark,
                             isReturn = a.isReturn,
                             isVoid = a.isVoid
                         });

            return Query;
        }

        public Tuple<List<SalesVoidReportViewModel>, int> GetSalesVoidReport(string storeCode, DateTimeOffset dateFrom, DateTimeOffset dateTo, string shift, int offset, int page = 1, int size = 25, string Order = "{}")
        {
            var Query = GetSalesVoidReportQuery(storeCode, dateFrom, dateTo, shift);

            Dictionary<string, string> OrderDictionary = JsonConvert.DeserializeObject<Dictionary<string, string>>(Order);


            // Pageable<InventoriesReportViewModel> pageable = new Pageable<InventoriesReportViewModel>(Query, page - 1, size);
            List<SalesVoidReportViewModel> Data = Query.ToList<SalesVoidReportViewModel>();
            // int TotalData = pageable.TotalCount;
            return Tuple.Create(Data, Data.Count());

        }
        #endregion

        public StoreViewModels GetStore(string storeCode)
        {
            string storeUri = "master/stores/code";
            IHttpClientService httpClient = (IHttpClientService)ServiceProvider.GetService(typeof(IHttpClientService));
            if (httpClient != null)
            {
                var response = httpClient.GetAsync($"{APIEndpoint.Core}{storeUri}?code={storeCode}").Result.Content.ReadAsStringAsync();
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

        #region omzet report
        public IQueryable<OmzetReportViewModel> GetOmzetReportQuery(string storecode, DateTimeOffset dateFrom, DateTimeOffset dateTo, string shift)
        {
            var Query = (from c in DbContext.SalesDocs
                         join a in DbContext.SalesDocDetails on c.Id equals a.SalesDocId
                         where
						 c.isVoid == false &&
                         c._IsDeleted == false
                         && a._IsDeleted == false
                         //&& c.StorageId == (string.IsNullOrWhiteSpace(storageId) ? c.StorageId : storageId)
                         && c.Date.Date >= dateFrom.Date
                         && c.Date.Date <= dateTo.Date
                         && c.StoreCode == (string.IsNullOrWhiteSpace(storecode) ? c.StoreCode : storecode)
                         && c.Shift == (string.IsNullOrWhiteSpace(shift) ? c.Shift : Convert.ToInt32(shift))
                         //&& a.ItemName == (string.IsNullOrWhiteSpace(info) ? a.ItemName : info)

                         select new
                         {
                             ItemCode = a.ItemCode == "" ? "-" : a.ItemCode,
                             ItemName = a.ItemName == "" ? "-" : a.ItemName,
                             ItemSize = a.ItemSize == "" ? "-" : a.ItemSize,
                             Price = a.Price,
                             Quantity = a.Quantity,
                             Discount1 = a.Discount1,
                             Discount2 = a.Discount2,
                             DiscountNominal = a.DiscountNominal,
                             SpecialDiscount = a.SpesialDiscount,
                             Margin = a.Margin,
                             Code = c.Code == "" ? "-" : c.Code,
                             Date = c.Date,
                             IsVoid = c.isVoid,
                             IsReturn = c.isReturn,
                             Discount = c.Discount,
                             PaymentType = c.PaymentType == "" ? "-" : c.PaymentType,
                             Card = c.Card == "" ? "-" : c.Card,
                             CardTypeName = c.CardTypeName == "" ? "-" : c.CardTypeName,
                             CashAmount = c.CashAmount,
                             CardAmount = c.CardAmount,
                             BankName = c.BankName == "" ? "-" : c.BankName,
                             BankCardName = c.BankCardName == "" ? "-" : c.BankCardName,
                             SubTotal = c.SubTotal,
                             StoreName = c.StoreName,
                             Shift = c.Shift,
                             Voucher = c.VoucherValue
                         });

            var Query2 = (from data in Query

                          group data by new { data.Code } into groupData
                          select new OmzetReportViewModel
                          {
                              StoreName = groupData.FirstOrDefault().StoreName,
                              Shift = groupData.FirstOrDefault().Shift,
                              Date = groupData.FirstOrDefault().Date,
                              Code = groupData.FirstOrDefault().Code,
                              PaymentType = groupData.FirstOrDefault().PaymentType,
                              Card = groupData.FirstOrDefault().Card,
                              CardTypeName = groupData.FirstOrDefault().CardTypeName,
                              BankName = groupData.FirstOrDefault().BankName,
                              BankCardName = groupData.FirstOrDefault().BankCardName,
                              CashAmount = groupData.Sum(x => x.CashAmount),
                              CardAmount = groupData.Sum(x => x.CardAmount),
                              Voucher = groupData.FirstOrDefault().Voucher,
                              TotalOmzetBruto = groupData.Sum(x => x.SubTotal),
                              Discount = groupData.Sum(x => x.Discount),
                              Margin = groupData.Sum(x => x.Margin),
                              TotalOmzetNetto = groupData.Sum(x => x.SubTotal)
                          }

              );

            return Query2;
        }

        public Tuple<List<OmzetReportViewModel>, int> GetOmzetReport(string storecode, DateTimeOffset dateFrom, DateTimeOffset dateTo, string shift, string info, int offset, string username, int page = 1, int size = 25, string Order = "{}")
        {
            var Query = GetOmzetReportQuery(storecode, dateFrom, dateTo, shift);

            Dictionary<string, string> OrderDictionary = JsonConvert.DeserializeObject<Dictionary<string, string>>(Order);


            // Pageable<InventoriesReportViewModel> pageable = new Pageable<InventoriesReportViewModel>(Query, page - 1, size);
            List<OmzetReportViewModel> Data = Query.ToList<OmzetReportViewModel>();
            // int TotalData = pageable.TotalCount;

            return Tuple.Create(Data, Data.Count());

        }


        public MemoryStream GenerateExcelOmzet(string storecode, DateTimeOffset dateFrom, DateTimeOffset dateTo, string shift)
        {
            var Query = GetOmzetReportQuery(storecode, dateFrom, dateTo, shift);
            // Query = Query.OrderByDescending(a => a.ReceiptDate);
            DataTable result = new DataTable();
            //No	Unit	Budget	Kategori	Tanggal PR	Nomor PR	Kode Barang	Nama Barang	Jumlah	Satuan	Tanggal Diminta Datang	Status	Tanggal Diminta Datang Eksternal

            result.Columns.Add(new DataColumn() { ColumnName = "No", DataType = typeof(String) });
            result.Columns.Add(new DataColumn() { ColumnName = "Toko", DataType = typeof(String) });
            result.Columns.Add(new DataColumn() { ColumnName = "Shift", DataType = typeof(String) });
            result.Columns.Add(new DataColumn() { ColumnName = "Tanggal", DataType = typeof(String) });
            result.Columns.Add(new DataColumn() { ColumnName = "No Pembayaran", DataType = typeof(String) });
            result.Columns.Add(new DataColumn() { ColumnName = "Tipe Pembayaran", DataType = typeof(String) });
            result.Columns.Add(new DataColumn() { ColumnName = "Kartu", DataType = typeof(String) });
            result.Columns.Add(new DataColumn() { ColumnName = "Jenis Kartu", DataType = typeof(String) });
            result.Columns.Add(new DataColumn() { ColumnName = "Bank (EDC)", DataType = typeof(String) });
            result.Columns.Add(new DataColumn() { ColumnName = "Bank (Kartu)", DataType = typeof(String) });
            result.Columns.Add(new DataColumn() { ColumnName = "Cash Amount", DataType = typeof(double) });
            result.Columns.Add(new DataColumn() { ColumnName = "Card Amount", DataType = typeof(double) });
            result.Columns.Add(new DataColumn() { ColumnName = "Voucher", DataType = typeof(double) });
            result.Columns.Add(new DataColumn() { ColumnName = "Total Omset Bruto", DataType = typeof(double) });
            result.Columns.Add(new DataColumn() { ColumnName = "Total Diskon", DataType = typeof(double) });
            result.Columns.Add(new DataColumn() { ColumnName = "Total Margin", DataType = typeof(double) });
            result.Columns.Add(new DataColumn() { ColumnName = "Total Omzet Netto", DataType = typeof(double) });



            if (Query.ToArray().Count() == 0)
                result.Rows.Add("", "", "", "", "", "", "", "", "", 0, 0, 0, 0, 0, 0);
            // to allow column name to be generated properly for empty data as template
            else
            {
                double totCashAmount = 0;
                double totCardAmount = 0;
                double totVoucher = 0;
                double totTotalOmzetBruto = 0;
                double totDiscount = 0;
                double totMargin = 0;
                double totTotalOmzetNetto = 0;
                int index = 0;
                foreach (var item in Query)
                {
                    index++;
                    string date = item.Date == null ? "-" : item.Date.ToOffset(new TimeSpan(7, 0, 0)).ToString("dd MMM yyyy", new CultureInfo("id-ID"));
                    //string pr_date = item.PRDate == null ? "-" : item.PRDate.ToOffset(new TimeSpan(offset, 0, 0)).ToString("dd MMM yyyy", new CultureInfo("id-ID"));
                    //string do_date = item.DODate == null ? "-" : item.ReceiptDate.ToOffset(new TimeSpan(offset, 0, 0)).ToString("dd MMM yyyy", new CultureInfo("id-ID"));

                    result.Rows.Add(index, item.StoreName, item.Shift, date, item.Code, item.PaymentType, item.Card, item.CardTypeName, item.BankName, item.BankCardName, item.CashAmount, item.CardAmount, item.Voucher,
                        item.TotalOmzetBruto, item.Discount, item.Margin, item.TotalOmzetNetto);

                    totCashAmount = +item.CashAmount;
                    totCardAmount = +item.CardAmount;
                    totVoucher = +item.Voucher;
                    totTotalOmzetBruto = +item.TotalOmzetBruto;
                    totDiscount = +item.Discount;
                    totMargin = +item.Margin;
                    totTotalOmzetNetto = +item.TotalOmzetNetto;

                }

                result.Rows.Add("", "", "", "", "", "", "", "", "", "", totCashAmount, totCardAmount, totVoucher, totTotalOmzetBruto, totDiscount, totMargin, totTotalOmzetNetto);
            }

            return Excel.CreateExcel(new List<KeyValuePair<DataTable, string>>() { new KeyValuePair<DataTable, string>(result, "Territory") }, true);
        }
        #endregion



        #region OmzetDailyReport
        public TotalCategoryViewModel GetOmzetDayReportQuery(DateTimeOffset dateFrom, DateTimeOffset dateTo)
        {

            var Store = GetStoreData();

            var SalesDocs = (from c in DbContext.SalesDocs
                             where
                               c._IsDeleted == false
                               && c.Date.Date >= dateFrom.Date
                               && c.Date.Date <= dateTo.Date

                             select new
                             {
                                 StoreCode = c.StoreCode,
                                 SubTotal = c.SubTotal,
                                 TotalProduct = c.TotalProduct,
                                 Remark = c.Remark
                             }

                );

            var StandAlone = (from a in Store
                              join c in SalesDocs on a.Code equals c.StoreCode into d
                              from c in d.DefaultIfEmpty()
                              where
                              // c._IsDeleted == false
                               //&& c.Date.Date >= dateFrom.Date
                               //&& c.Date.Date <= dateTo.Date
                              a.SalesCategory == "STAND ALONE"
                               && a.OnlineOffline == "OFFLINE"
                              && a.Status == "OPEN"
                              select new
                              {
                                  storeName = a.Name,
                                  storeCategory = a.SalesCategory,
                                  offlineOnline = a.OnlineOffline,
                                  subTotal = c == null ? 0 : c.SubTotal,
                                  totalProduct = c == null ? 0 : c.TotalProduct,
                                  remark = c == null ? "" : c.Remark

                              });
            var Konsinyasi = (from a in Store
                              join c in SalesDocs on a.Code equals c.StoreCode into d
                              from c in d.DefaultIfEmpty()
                              where
                              a.SalesCategory == "KONSINYASI"
                              && a.Status == "OPEN"

                              select new
                              {
                                  storeName = a.Name,
                                  storeCategory = a.SalesCategory,
                                  offlineOnline = a.OnlineOffline,
                                  subTotal = c == null ? 0 : c.SubTotal,
                                  totalProduct = c == null ? 0 : c.TotalProduct,
                                  remark = c == null ? "" : c.Remark

                              });

            var Online = (from a in Store
                          join c in SalesDocs on a.Code equals c.StoreCode into d
                          from c in d.DefaultIfEmpty()
                          where
                          a.OnlineOffline == "ONLINE"
                          && a.Status == "OPEN"
                          select new
                          {
                              storeName = a.Name,
                              storeCategory = a.SalesCategory,
                              offlineOnline = a.OnlineOffline,
                              subTotal = c == null ? 0 : c.SubTotal,
                              totalProduct = c == null ? 0 : c.TotalProduct,
                              remark = c == null ? "" : c.Remark

                          });
            var WholeSale = (from a in Store
                             join c in SalesDocs on a.Code equals c.StoreCode into d
                             from c in d.DefaultIfEmpty()
                             where
                              a.SalesCategory == "PENJUALAN UMUM"
                             && a.Status == "OPEN"

                             select new
                             {
                                 storeName = a.Name,
                                 storeCategory = a.SalesCategory,
                                 offlineOnline = a.OnlineOffline,
                                 subTotal = c == null ? 0 : c.SubTotal,
                                 totalProduct = c == null ? 0 : c.TotalProduct,
                                 remark = c == null ? "" : c.Remark

                             });

            var Vvip = (from a in Store
                             join c in SalesDocs on a.Code equals c.StoreCode into d
                             from c in d.DefaultIfEmpty()
                             where
                              a.SalesCategory == "PENJUALAN VVIP"
                             && a.Status == "OPEN"

                             select new
                             {
                                 storeName =  a.Name,
                                 storeCategory = a.SalesCategory,
                                 offlineOnline = a.OnlineOffline,
                                 subTotal = c == null ? 0 : c.SubTotal,
                                 totalProduct = c == null ? 0 : c.TotalProduct,
                                 remark = c == null ? "" : c.Remark

                             });

            var CombineData = StandAlone.Union(Konsinyasi).Union(Online).Union(WholeSale).Union(Vvip).ToList();

            var Query = (from data in CombineData

                         group data by new { data.storeCategory, data.offlineOnline } into groupData
                         select new CategoryViewModel
                         {
                             CategoryName = groupData.FirstOrDefault().storeCategory,
                             OfflineOnline = groupData.FirstOrDefault().offlineOnline,
                             GrandTotal = groupData.Sum(x => x.subTotal),
                             Count = groupData.Sum(x => x.totalProduct),

                         }).ToList();

            CategoryCollectViewModel CategoryCollect = new CategoryCollectViewModel
            {
                StandAlone = Query.Find(x => x.CategoryName == "STAND ALONE"),
                Konsinyasi = Query.Find(x => x.CategoryName == "KONSINYASI"),
                Online = Query.Find(x => x.OfflineOnline == "ONLINE"),
                WholeSale = Query.Find(x => x.CategoryName == "PENJUALAN UMUM"),
                vvip = Query.Find(x => x.CategoryName == "PENJUALAN VVIP")



            };

            List<TotalViewModel> Stand_Alone = new List<TotalViewModel>();
            foreach (var ip in CombineData.Where(x => x.storeCategory == "STAND ALONE" && x.offlineOnline == "OFFLINE"))
            {
                Stand_Alone.Add(new TotalViewModel
                {
                    Count = ip.totalProduct,
                    GrandTotal = ip.subTotal,
                    Store = new StoreViewModel
                    {
                        Name = ip.storeName,
                        StoreCategory = ip.storeCategory

                    }

                });
            };

            List<TotalViewModel> Konsinyasi2 = new List<TotalViewModel>();
            foreach (var ip in CombineData.Where(x => x.storeCategory == "KONSINYASI"))
            {
                Konsinyasi2.Add(new TotalViewModel
                {
                    Count = ip.totalProduct,
                    GrandTotal = ip.subTotal,
                    Store = new StoreViewModel
                    {
                        Name = ip.storeName,
                        StoreCategory = ip.storeCategory

                    }

                });
            };

            List<TotalViewModel> Online2 = new List<TotalViewModel>();
            foreach (var ip in CombineData.Where(x => x.offlineOnline == "ONLINE"))
            {
                Online2.Add(new TotalViewModel
                {
                    Count = ip.totalProduct,
                    GrandTotal = ip.subTotal,
                    Store = new StoreViewModel
                    {
                        Name = ip.storeName,
                        StoreCategory = ip.storeCategory

                    }

                });
            };

            List<TotalViewModel> WholeSale2 = new List<TotalViewModel>();
            foreach (var ip in CombineData.Where(x => x.storeCategory == "PENJUALAN UMUM"))
            {
                WholeSale2.Add(new TotalViewModel
                {
                    Count = ip.totalProduct,
                    GrandTotal = ip.subTotal,
                    Store = new StoreViewModel
                    {
                        Name = ip.storeName,
                        StoreCategory = ip.storeCategory,
                        Remark = ip.remark

                    }

                });
            };

            List<TotalViewModel> Vvip2 = new List<TotalViewModel>();
            foreach (var ip in CombineData.Where(x => x.storeCategory == "PENJUALAN VVIP"))
            {
                Vvip2.Add(new TotalViewModel
                {
                    Count = ip.totalProduct,
                    GrandTotal = ip.subTotal,
                    Store = new StoreViewModel
                    {
                        Name = ip.storeName,
                        StoreCategory = ip.storeCategory,
                        Remark = ip.remark

                    }

                });
            };



            DataViewModel dataView = new DataViewModel
            {
                StandAlone = Stand_Alone,
                Konsinyasi = Konsinyasi2,
                Online = Online2,
                WholeSale = WholeSale2,
                vvip = Vvip2
            };

            TotalCategoryViewModel totalCategory = new TotalCategoryViewModel
            {
                CategoryList = CategoryCollect,
                DataList = dataView
            };

            return totalCategory;
        }

        public Tuple<TotalCategoryViewModel, int> GetOmzetDailyReport(DateTimeOffset dateFrom, DateTimeOffset dateTo, int offset, int page = 1, int size = 25, string Order = "{}")
        {
            var Query = GetOmzetDayReportQuery(dateFrom, dateTo);

            Dictionary<string, string> OrderDictionary = JsonConvert.DeserializeObject<Dictionary<string, string>>(Order);


            // Pageable<InventoriesReportViewModel> pageable = new Pageable<InventoriesReportViewModel>(Query, page - 1, size);
            //TotalCategoryViewModel> Data = Query.ToList<TotalCategoryViewModel>();
            // int TotalData = pageable.TotalCount;

            return Tuple.Create(Query, 1);

        }


        private List<StoreViewModel> GetStoreData()
        {
            string StoreUri = "master/stores?Size=200";

            string uri = StoreUri;
            IHttpClientService httpClient = (IHttpClientService)this.ServiceProvider.GetService(typeof(IHttpClientService));
            var response = httpClient.GetAsync($"{APIEndpoint.Core}{uri}").Result;
            if (response.IsSuccessStatusCode)
            {
                var content = response.Content.ReadAsStringAsync().Result;
                Dictionary<string, object> result = JsonConvert.DeserializeObject<Dictionary<string, object>>(content);
                List<StoreViewModel> upo = JsonConvert.DeserializeObject<List<StoreViewModel>>(result.GetValueOrDefault("data").ToString()); ;
                return upo;
            }
            else
            {
                return null;
            }
        }

        #endregion 
        /*
        public List<SalesDocByRoViewModel> GetByRO(string articleRealizationOrder)
        {
            var Query = GetByRoQuery(articleRealizationOrder);
            return Query.ToList();
        }
        */
        public List<SalesDocByRoViewModel> GetByRO(string articleRealizationOrder)
        {
            var Query = (from a in DbContext.SalesDocs
                         join b in DbContext.SalesDocDetails on a.Id equals b.SalesDocId

                         where b.ItemArticleRealizationOrder == articleRealizationOrder
                         && a.isReturn == false
                         && a.isVoid == false
                         && a._IsDeleted == false
                         && b._IsDeleted == false

                         orderby b._CreatedUtc descending

                         group new { a, b } by new { a.StoreCode, a.StoreName, b.ItemSize, b.Quantity ,b.ItemArticleRealizationOrder, b.ItemCode } into data
                         
                         select new SalesDocByRoViewModel
                         {
                             StoreCode = data.Key.StoreCode,
                             StoreName = data.Key.StoreName,
                             ItemArticleRealizationOrder = data.Key.ItemArticleRealizationOrder,
                             ItemCode = data.Key.ItemCode,

                             size = data.Key.ItemSize,
                             quantityOnSales = data.Sum(x => x.b.Quantity),
                         });

            return Query.ToList();

        }
    }
}
