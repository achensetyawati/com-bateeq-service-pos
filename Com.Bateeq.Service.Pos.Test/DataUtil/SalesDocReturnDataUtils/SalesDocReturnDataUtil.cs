using Com.Bateeq.Service.Pos.Lib.Models.SalesReturn;
using Com.Bateeq.Service.Pos.Lib.Services.SalesDocReturnService;
using Com.Bateeq.Service.Pos.Lib.ViewModels.SalesDoc;
using Com.Bateeq.Service.Pos.Lib.ViewModels.SalesDocReturn;
using Com.Bateeq.Service.Pos.Test.DataUtil.SalesDocDataUtils;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Com.Bateeq.Service.Pos.Test.DataUtil.SalesDocReturnDataUtils
{
    public class SalesDocReturnDataUtil
    {
        private readonly SalesDocReturnService _service;
        private readonly SalesDocDataUtil salesDocDataUtil;
        public SalesDocReturnDataUtil(SalesDocReturnService service, SalesDocDataUtil salesDocDataUtil)
        {
            _service = service;
            this.salesDocDataUtil = salesDocDataUtil;
        }

        public async Task<SalesDocReturnViewModel> getViewModel()
        {
            var data = await Task.Run(() => salesDocDataUtil.GetTestData());
            return new SalesDocReturnViewModel()
            {
                sales= new SalesDocViewModel()
                {
                    code= data.Code,
                    Id= data.Id
                },
                salesDetail = new SalesDetail() {
                    bank=new Bank
                    {
                        _id=1,
                        name="name",

                    },
                    bankCard= new Bank
                    {
                        _id=1,
                        name="name"
                    },
                    cardType= new Card
                    {
                        _id=1,
                        name="name"
                    },
                    voucher=new Voucher
                    {
                        value=1
                    }
                },
                store = new Store()
                {
                    Id = data.StoreId,
                    Name = data.StoreName,
                    Code="code",
                    Storage = new StorageViewModel()
                    {
                        name = data.StoreStorageName,
                        _id = data.StoreStorageId,
                        code="code"
                    }

                },
                items = new List<SalesDocReturnDetailViewModel>()
                {
                    new SalesDocReturnDetailViewModel()
                    {
                        quantity=1,
                        item= new SalesDocDetailViewModel()
                        {
                            itemCode="code",
                            item= new Lib.ViewModels.NewIntegrationViewModel.ItemViewModel()
                            {
                                code="code",
                                ArticleRealizationOrder="a"
                            },
                        },
                        returnItems= new List<SalesDocDetailViewModel>()
                        {
                            new SalesDocDetailViewModel()
                            {
                                itemCode="c",
                                item= new Lib.ViewModels.NewIntegrationViewModel.ItemViewModel()
                                {
                                    code="c",
                                    ArticleRealizationOrder="a"
                                }
                            }
                        }
                    }
                }
            };
        }


        public async Task<SalesDocReturn> GetNewData()
        {
            var datas = await Task.Run(() => salesDocDataUtil.GetTestData());
            SalesDocReturn TestData = new SalesDocReturn()
            {
                Code = "code",
                Date = DateTimeOffset.UtcNow,
                StoreId = 1,
                StoreStorageId = 1,
                StoreStorageCode = "code",
                StoreStorageName = "Name",
                StoreCode = "code",
                StoreName = "name",
                SalesDocId= datas.Id,
                SalesDocCode= datas.Code,
                Details = new List<SalesDocReturnDetail>()
                {
                    new SalesDocReturnDetail()
                    {
                        ItemCode = "code",
                        ItemId = 1,
                        isReturn=false,
                        ItemName="name",
                        ItemDomesticCOGS=1,

                    },

                }
            };

            return TestData;
        }



        public async Task<SalesDocReturn> GetTestData()
        {
            var data =await GetNewData();
            var vm = await getViewModel();
            await _service.Create(data, vm);
            return _service.ReadModelById(data.Id);
        }
    }
}
