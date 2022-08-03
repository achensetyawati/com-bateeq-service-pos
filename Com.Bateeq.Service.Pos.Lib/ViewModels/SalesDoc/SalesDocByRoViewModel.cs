using Com.Bateeq.Service.Pos.Lib.ViewModels.NewIntegrationViewModel;
using Com.Danliris.Service.Inventory.Lib.Helpers;
using System;
using System.Collections.Generic;
using System.Text;

namespace Com.Bateeq.Service.Pos.Lib.ViewModels.SalesDoc
{
    public class SalesDocByRoViewModel : BasicViewModel
    {
        public string StoreCode { get; set; }
        public string StoreName { get; set; }
        public string StoreStorageCode { get; set; }
        public string StoreStorageName { get; set; }
        public string ItemCode { get; set; }
        public string ItemArticleRealizationOrder { get; set; }
        public string size { get; set; }
        public double quantityOnSales { get; set; }
    }
}
