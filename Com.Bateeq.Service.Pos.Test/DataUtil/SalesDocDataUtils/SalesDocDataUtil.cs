using Com.Bateeq.Service.Pos.Lib.Models.SalesDoc;
using Com.Bateeq.Service.Pos.Lib.Services.SalesDocService;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Com.Bateeq.Service.Pos.Test.DataUtil.SalesDocDataUtils
{
    public class SalesDocDataUtil
    {
        private readonly SalesDocService _service;
        public SalesDocDataUtil(SalesDocService service)
        {
            _service = service;
        }


        public SalesDoc GetNewData()
        {
            SalesDoc TestData = new SalesDoc()
            {
                Code = "code",
                BankCardName = "name",
                PaymentType = "Cash",
                Date = DateTimeOffset.UtcNow,
                StoreId = 1,
                StoreStorageId = 1,
                StoreStorageCode = "code",
                StoreStorageName = "Name",
                StoreCategory = "offline",
                StoreCode = "code",
                BankCardId = 1,
                BankCardCode = "USD",
                VoucherValue = 1,
                BankId = 1,
                BankName = "BankName",
                BankCode = "BankCode",
                StoreName= "name",
                Card="card",
                Details = new List<SalesDocDetail>()
                {
                    new SalesDocDetail()
                    {
                        ItemCode = "code",
                        ItemId = 1,
                        isReturn=false,
                        ItemName="name",
                        ItemDomesticCOGS=1,
                        ItemArticleRealizationOrder="a"
                    },

                }
            };

            return TestData;
        }



        public async Task<SalesDoc> GetTestData()
        {
            var data = GetNewData();
            await _service.Create(data);
            return _service.ReadModelById(data.Id);
        }
    }
}
