using Com.Danliris.Service.Inventory.Lib;
using Com.Danliris.Service.Inventory.Lib.Services;
using Com.Bateeq.Service.Pos.Lib.Interfaces;
using Com.Bateeq.Service.Pos.Lib.Services.SalesDocReturnService;
using Com.Bateeq.Service.Pos.Lib.Services.SalesDocService;
using Com.Bateeq.Service.Pos.Lib.ViewModels.SalesDoc;
using Com.Bateeq.Service.Pos.Lib.ViewModels.SalesDocReturn;
using Com.Bateeq.Service.Pos.Test.DataUtil.SalesDocDataUtils;
using Com.Bateeq.Service.Pos.Test.DataUtil.SalesDocReturnDataUtils;
using Com.Bateeq.Service.Pos.Test.DataUtil.SalesDocDataUtils;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Text;
using Xunit;

namespace Com.Bateeq.Service.Pos.Test.Service.SalesDocReturnReturnServiceTests
{
    public class BasicTests
    {
        private const string ENTITY = "SalesDocReturns";
        //private PurchasingDocumentAcceptanceDataUtil pdaDataUtil;
        //private readonly IIdentityService identityService;

        [MethodImpl(MethodImplOptions.NoInlining)]
        public string GetCurrentMethod()
        {
            StackTrace st = new StackTrace();
            StackFrame sf = st.GetFrame(1);

            return string.Concat(sf.GetMethod().Name, "_", ENTITY);
        }

        private PosDbContext _dbContext(string testName)
        {
            var serviceProvider = new ServiceCollection()
              .AddEntityFrameworkInMemoryDatabase()
              .BuildServiceProvider();

            DbContextOptionsBuilder<PosDbContext> optionsBuilder = new DbContextOptionsBuilder<PosDbContext>();
            optionsBuilder
                .UseInMemoryDatabase(testName)
                .ConfigureWarnings(w => w.Ignore(InMemoryEventId.TransactionIgnoredWarning))
                .UseInternalServiceProvider(serviceProvider);

            PosDbContext dbContext = new PosDbContext(optionsBuilder.Options);

            return dbContext;
        }

        private SalesDocReturnDataUtil _dataUtil(SalesDocReturnService service)
        {
            var salesDocService = new SalesDocService(GetServiceProvider().Object, _dbContext(GetCurrentMethod()));
            var salesDocDataUtil = new SalesDocDataUtil(salesDocService);
            return new SalesDocReturnDataUtil(service, salesDocDataUtil);
        }

        private Mock<IServiceProvider> GetServiceProvider()
        {
            HttpResponseMessage message = new HttpResponseMessage(System.Net.HttpStatusCode.OK);
            message.Content = new StringContent("{\"apiVersion\":\"1.0\",\"statusCode\":200,\"message\":\"Ok\",\"data\":[{\"Id\":7,\"code\":\"USD\",\"rate\":13700.0,\"date\":\"2018/10/20\"}],\"info\":{\"count\":1,\"page\":1,\"size\":1,\"total\":2,\"order\":{\"date\":\"desc\"},\"select\":[\"Id\",\"code\",\"rate\",\"date\"]}}");
            var HttpClientService = new Mock<IHttpClientService>();
            HttpClientService
                .Setup(x => x.GetAsync(It.IsAny<string>()))
                .ReturnsAsync(message);
            HttpClientService
               .Setup(x => x.GetAsync(It.Is<string>(s => s.Contains("master/stores/code"))))
               .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent("{\"apiVersion\":\"1.0\",\"statusCode\":200,\"message\":\"Ok\",\"data\":[{\"Id\":7,\"code\":\"USD\",\"rate\":13700.0,\"date\":\"2018/10/20\"}],\"info\":{\"count\":1,\"page\":1,\"size\":1,\"total\":2,\"order\":{\"date\":\"desc\"},\"select\":[\"Id\",\"code\",\"rate\",\"date\"]}}") });
            HttpClientService
                .Setup(x => x.GetAsync(It.IsAny<string>()))
                .ReturnsAsync(message);
            var serviceProvider = new Mock<IServiceProvider>();
            serviceProvider
                .Setup(x => x.GetService(typeof(IdentityService)))
                .Returns(new IdentityService() { Token = "Token", Username = "Test" });

            serviceProvider
              .Setup(s => s.GetService(typeof(IIdentityService)))
              .Returns(new IdentityService() { TimezoneOffset = 1, Token = "token", Username = "username" });


            serviceProvider
                .Setup(x => x.GetService(typeof(IHttpClientService)))
                .Returns(HttpClientService.Object);

            serviceProvider
                .Setup(x => x.GetService(typeof(IHttpClientService)))
                .Returns(new SalesDocReturnIHttpService());

            return serviceProvider;
        }

        private Mock<IServiceProvider> GetServiceProvider1()
        {
            HttpResponseMessage message = new HttpResponseMessage(System.Net.HttpStatusCode.OK);
            message.Content = new StringContent("{\"apiVersion\":\"1.0\",\"statusCode\":200,\"message\":\"Ok\",\"data\":{\"Id\":7,\"code\":\"code\",\"name\":\"name\",\"address\":\"name\",\"city\":\"name\",\"date\":\"2018/10/20\"},\"info\":{\"count\":1,\"page\":1,\"size\":1,\"total\":2,\"order\":{\"date\":\"desc\"}}}");
            var HttpClientService = new Mock<IHttpClientService>();
            HttpClientService
                .Setup(x => x.GetAsync(It.IsAny<string>()))
                .ReturnsAsync(message);
            HttpClientService
                .Setup(x => x.GetAsync(It.IsAny<string>()))
                .ReturnsAsync(message);
            var serviceProvider = new Mock<IServiceProvider>();
            serviceProvider
                .Setup(x => x.GetService(typeof(IdentityService)))
                .Returns(new IdentityService() { Token = "Token", Username = "Test" });

            serviceProvider
              .Setup(s => s.GetService(typeof(IIdentityService)))
              .Returns(new IdentityService() { TimezoneOffset = 1, Token = "token", Username = "username" });


            serviceProvider
                .Setup(x => x.GetService(typeof(IHttpClientService)))
                .Returns(HttpClientService.Object);

            return serviceProvider;
        }

        [Fact]
        public async void Should_Success_Get_Data()
        {
            var service = new SalesDocReturnService(GetServiceProvider().Object, _dbContext(GetCurrentMethod()));
            var data = await _dataUtil(service).GetTestData();
            var Response = service.ReadModel("code", 1, 25, "{}", "", "{}", "username");
            Assert.NotEmpty(Response.Item1);

            //Dictionary<string, string> order = new Dictionary<string, string>()
            //{
            //    {"No", "asc" }
            //};
            //var response2 = service.ReadModel(1, 25, JsonConvert.SerializeObject(order), null, data.Name, "{}");
            //Assert.NotEmpty(response2.Data);
        }

        [Fact]
        public async void Should_Success_Get_Data_By_Id()
        {
            var service = new SalesDocReturnService(GetServiceProvider().Object, _dbContext(GetCurrentMethod()));
            var data = await _dataUtil(service).GetTestData();
            var Response = service.ReadModelById(data.Id);
            Assert.NotNull(Response);
        }


        [Fact]
        public async void Should_Success_Create_Data()
        {
            var service = new SalesDocReturnService(GetServiceProvider().Object, _dbContext(GetCurrentMethod()));
            var data = await _dataUtil(service).GetNewData();
            var vm = await _dataUtil(service).getViewModel();
            //var vm = service.MapToViewModel(data);
            var Response = await service.Create(data,vm);
            Assert.NotNull(Response);
        }

        [Fact]
        public async void Should_Success_MapToModel()
        {
            var service = new SalesDocReturnService(GetServiceProvider().Object, _dbContext(GetCurrentMethod()));
            
            var vm = await _dataUtil(service).getViewModel();
            var Response = service.MapToModel(vm);
            Assert.NotNull(Response);
        }

        //[Fact]
        //public async void Should_Success_MapToVM()
        //{
        //    var service = new SalesDocReturnService(GetServiceProvider().Object, _dbContext(GetCurrentMethod()));

        //    var data = await _dataUtil(service).GetTestData();
        //    var Response = service.MapToViewModel(data);
        //    Assert.NotNull(Response);
        //}

        [Fact]
        public async void Should_Fail_Create_Data()
        {
            var service = new SalesDocReturnService(GetServiceProvider().Object, _dbContext(GetCurrentMethod()));
            var data = await _dataUtil(service).GetNewData();
            var vm = new SalesDocReturnViewModel()
            {
                salesDetail = new SalesDetail()
                {
                    bank = new Bank
                    {
                        _id = 1,
                        name = "name",

                    },
                    bankCard = new Bank
                    {
                        _id = 1,
                        name = "name"
                    },
                    cardType = new Card
                    {
                        _id = 1,
                        name = "name"
                    },
                    voucher = new Voucher
                    {
                        value = 1
                    }
                },
                store = new Store()
                {
                    Id = data.StoreId,
                    Name = data.StoreName,
                    Storage = new StorageViewModel()
                    {
                        name = data.StoreStorageName,
                        _id = data.StoreStorageId
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
                                itemCode="code",
                                item= new Lib.ViewModels.NewIntegrationViewModel.ItemViewModel()
                                {
                                    code="code",
                                    ArticleRealizationOrder="a"
                                }
                            }
                        }
                    }
                }
            };
            //var vm = service.MapToViewModel(data);
            await Assert.ThrowsAnyAsync<Exception>(() => service.Create(data, vm));
        }


        [Fact]
        public async void Should_Success_GetStore()
        {
            var service = new SalesDocReturnService(GetServiceProvider1().Object, _dbContext(GetCurrentMethod()));
            
            var Response = service.GetStore("code");
            Assert.NotNull(Response);
        }


    }
}
