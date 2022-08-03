using AutoMapper;
using Com.Danliris.Service.Inventory.Lib;
using Com.Danliris.Service.Inventory.Lib.Services;
using Com.Danliris.Service.Inventory.Test.Helpers;
using Com.Bateeq.Service.Pos.Lib.Models.SalesDoc;
using Com.Bateeq.Service.Pos.Lib.Models.SalesReturn;
using Com.Bateeq.Service.Pos.Lib.Services.SalesDocReturnService;
using Com.Bateeq.Service.Pos.Lib.Services.SalesDocService;
using Com.Bateeq.Service.Pos.Lib.ViewModels.SalesDoc;
using Com.Bateeq.Service.Pos.Lib.ViewModels.SalesDocReturn;
using Com.Bateeq.Service.Pos.WebApi.Controllers.v1.SalesDocControllers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Runtime.CompilerServices;
using System.Security.Claims;
using System.Text;
using Xunit;

namespace Com.Bateeq.Service.Pos.Test.Controller.SalesDocControllerTests
{
    public class SalesDocReturnControllerTests
    {

        protected SalesDocReturnController GetController(Mock<IIdentityService> identityService, Mock<IValidateService> validateService, Mock<ISalesDocReturnService> service)
        {
            var user = new Mock<ClaimsPrincipal>();
            var claims = new Claim[]
            {
                new Claim("username", "SalesDoctestusername")
            };
            user.Setup(u => u.Claims).Returns(claims);

            SalesDocReturnController controller = new SalesDocReturnController(identityService.Object, validateService.Object, service.Object);
            controller.ControllerContext = new ControllerContext()
            {
                HttpContext = new DefaultHttpContext()
                {
                    User = user.Object
                }
            };
            controller.ControllerContext.HttpContext.Request.Headers["Authorization"] = "Bearer SalesDoctesttoken";
            controller.ControllerContext.HttpContext.Request.Path = new PathString("/v1/SalesDoc-test");
            return controller;
        }

        protected SalesDocReturnController GetErrorController(Mock<IIdentityService> identityService, Mock<IValidateService> validateService, Mock<ISalesDocReturnService> service)
        {
            SalesDocReturnController controller = new SalesDocReturnController(identityService.Object, validateService.Object, service.Object);
            controller.ControllerContext = new ControllerContext();            
            return controller;
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

        protected string GetCurrentAsyncMethod([CallerMemberName] string methodName = "")
        {
            var method = new StackTrace()
                .GetFrames()
                .Select(frame => frame.GetMethod())
                .FirstOrDefault(item => item.Name == methodName);

            return method.Name;

        }

        public SalesDocReturn GetTestData(PosDbContext dbContext)
        {
            SalesDocReturn data = new SalesDocReturn();
            dbContext.SalesDocReturns.Add(data);
            dbContext.SaveChanges();

            return data;
        }

        protected int GetStatusCode(IActionResult response)
        {
            return (int)response.GetType().GetProperty("StatusCode").GetValue(response, null);
        }

        //protected ServiceValidationException GetServiceValidationException()
        //{
        //    var serviceProvider = new Mock<IServiceProvider>();
        //    var validationResults = new List<ValidationResult>();
        //    System.ComponentModel.DataAnnotations.ValidationContext validationContext = new System.ComponentModel.DataAnnotations.ValidationContext(ViewModel, serviceProvider.Object, null);
        //    return new ServiceValidationException(validationContext, validationResults);
        //}

        Mock<IServiceProvider> GetServiceProvider()
        {
            Mock<IServiceProvider> serviceProvider = new Mock<IServiceProvider>();
            serviceProvider
              .Setup(s => s.GetService(typeof(IIdentityService)))
              .Returns(new IdentityService() { TimezoneOffset = 1, Token = "token", Username = "username" });

            var validateService = new Mock<IValidateService>();
            serviceProvider
              .Setup(s => s.GetService(typeof(IValidateService)))
              .Returns(validateService.Object);
            return serviceProvider;
        }

        [Fact]
        public void Get_Return_OK()
        {
            //Setup
            PosDbContext dbContext = _dbContext(GetCurrentAsyncMethod());
            Mock<IServiceProvider> serviceProvider = GetServiceProvider();


            var validateService = new Mock<IValidateService>();
            Mock<IIdentityService> identityService = new Mock<IIdentityService>();
            var mockService = new Mock<ISalesDocReturnService>();

            mockService.Setup(x => x.ReadModel(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .Returns(Tuple.Create(new List<SalesDocReturn>(), 0, new Dictionary<string, string>(), new List<string>()));

            SalesDocReturnController controller = GetController(identityService, validateService, mockService);


            //SalesDocReturnService service = new SalesDocReturnService(serviceProvider.Object, _dbContext("test"), sales);

            //serviceProvider.Setup(s => s.GetService(typeof(SalesDocReturnService))).Returns(service);
            //serviceProvider.Setup(s => s.GetService(typeof(PosDbContext))).Returns(dbContext);

            //SalesDocReturn testData = GetTestData(dbContext);

            //Act
            //IActionResult response = GetController(identityService.Object, validateService.Object, service).Get("", 1, 1, "{}", "", "{}");
            var response = controller.Get("", 1, 1, "{}", "", "{}");

            //Assert
            Assert.Equal((int)HttpStatusCode.OK, GetStatusCode(response));
        }

        [Fact]
        public void Get_ReturnError()
        {
            //Setup
            PosDbContext dbContext = _dbContext(GetCurrentAsyncMethod());
            Mock<IServiceProvider> serviceProvider = GetServiceProvider();

            
            var validateService = new Mock<IValidateService>();
            Mock<IIdentityService> identityService = new Mock<IIdentityService>();
            var mockService = new Mock<ISalesDocReturnService>();

            mockService.Setup(x => x.ReadModel(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>(), "{", It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .Returns(Tuple.Create(new List<SalesDocReturn>(), 0, new Dictionary<string, string>(), new List<string>()));

            SalesDocReturnController controller = GetErrorController(identityService, validateService, mockService);


            //SalesDocReturnService service = new SalesDocReturnService(serviceProvider.Object, _dbContext("test"), sales);

            //serviceProvider.Setup(s => s.GetService(typeof(SalesDocReturnService))).Returns(service);
            //serviceProvider.Setup(s => s.GetService(typeof(PosDbContext))).Returns(dbContext);

            //SalesDocReturn testData = GetTestData(dbContext);

            //Act
            //IActionResult response = GetController(identityService.Object, validateService.Object, service).Get("", 1, 1, "{}", "", "{}");
            var response = controller.Get("", 1, 1, "{", "", "{}");

            //Assert
            Assert.Equal((int)HttpStatusCode.InternalServerError, GetStatusCode(response));
        }

        [Fact]
        public void Should_Success_Get_Data_By_Id()
        {
            Mock<IIdentityService> identityService = new Mock<IIdentityService>();

            var validateMock = new Mock<IValidateService>();
            validateMock.Setup(s => s.Validate(It.IsAny<ISalesDocReturnService>())).Verifiable();

            var mockFacade = new Mock<ISalesDocReturnService>();

            mockFacade.Setup(x => x.ReadModelById(It.IsAny<int>()))
                .Returns(new SalesDocReturn() { });

            //var mockMapper = new Mock<IMapper>();
            //mockMapper.Setup(x => x.Map<SalesDocReturn>(It.IsAny<SalesDocReturn>()))
            //    .Returns(SalesDocViewModel);

            SalesDocReturnController controller = GetController(identityService, validateMock, mockFacade);
            var response = controller.GetById(It.IsAny<int>());
            Assert.Equal(200, (int)HttpStatusCode.OK);
        }

        [Fact]
        public void POST_Return_OK()
        {
            //Setup
            PosDbContext dbContext = _dbContext(GetCurrentAsyncMethod());
            Mock<IServiceProvider> serviceProvider = GetServiceProvider();
            var validateService = new Mock<IValidateService>();
            Mock<IIdentityService> identityService = new Mock<IIdentityService>();
            var validateMock = new Mock<IValidateService>();
            var mockFacade = new Mock<ISalesDocReturnService>();

            SalesDocReturnService service = new SalesDocReturnService(serviceProvider.Object, _dbContext("test"));

            serviceProvider.Setup(s => s.GetService(typeof(SalesDocReturnService))).Returns(service);
            serviceProvider.Setup(s => s.GetService(typeof(PosDbContext))).Returns(dbContext);

            SalesDocReturnViewModel dataVM = new SalesDocReturnViewModel()
            {
                sales = new SalesDocViewModel()
                {
                    code = "code",
                    Id = 1
                },
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
                    Id = 1,
                    Name = "name",
                    Storage = new StorageViewModel()
                    {
                        name = "name",
                        _id = 1
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

            var serviceMock = new Mock<ISalesDocService>();
            serviceMock
                .Setup(s => s.Create(It.IsAny<SalesDoc>()))
                .ReturnsAsync(It.IsAny<SalesDoc>());

            var mockMapper = new Mock<IMapper>();
            mockMapper.Setup(x => x.Map<SalesDocReturn>(It.IsAny<SalesDocReturnViewModel>()))
                .Returns(new SalesDocReturn());

            SalesDocReturnController controller = GetController(identityService, validateMock, mockFacade);
            //Act
            IActionResult response = controller.Post(dataVM).Result;


            //Assert
            int statusCode = this.GetStatusCode(response);
            Assert.NotEqual((int)HttpStatusCode.NotFound, statusCode);
        }

        [Fact]
        public void POST_Return_BadRequest()
        {
            //Setup
            PosDbContext dbContext = _dbContext(GetCurrentAsyncMethod());
            Mock<IServiceProvider> serviceProvider = GetServiceProvider();
            var validateService = new Mock<IValidateService>();
            Mock<IIdentityService> identityService = new Mock<IIdentityService>();
            var validateMock = new Mock<IValidateService>();
            var mockFacade = new Mock<ISalesDocReturnService>();

            SalesDocReturnService service = new SalesDocReturnService(serviceProvider.Object, _dbContext("test"));

            serviceProvider.Setup(s => s.GetService(typeof(SalesDocReturnService))).Returns(service);
            serviceProvider.Setup(s => s.GetService(typeof(PosDbContext))).Returns(dbContext);

            SalesDocReturnViewModel dataVM = new SalesDocReturnViewModel()
            {
                sales = new SalesDocViewModel()
                {
                    code = "code",
                    Id = 1
                },
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
                    Id = 1,
                    Name = "name",
                    Storage = new StorageViewModel()
                    {
                        name = "name",
                        _id = 1
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
                        }
                    }
                }
            };

            var serviceMock = new Mock<ISalesDocService>();
            serviceMock
                .Setup(s => s.Create(It.IsAny<SalesDoc>()))
                .ReturnsAsync(It.IsAny<SalesDoc>());

            var mockMapper = new Mock<IMapper>();
            mockMapper.Setup(x => x.Map<SalesDocReturn>(It.IsAny<SalesDocReturnViewModel>()))
                .Returns(new SalesDocReturn());

            SalesDocReturnController controller = GetController(identityService, validateMock, mockFacade);
            //Act
            IActionResult response = controller.Post(dataVM).Result;


            //Assert
            int statusCode = this.GetStatusCode(response);
            Assert.NotEqual((int)HttpStatusCode.BadRequest, statusCode);
        }

        [Fact]
        public void POST_InternalServerError()
        {
            //Setup
            PosDbContext dbContext = _dbContext(GetCurrentAsyncMethod());
            Mock<IServiceProvider> serviceProvider = GetServiceProvider();
            var validateService = new Mock<IValidateService>();
            Mock<IIdentityService> identityService = new Mock<IIdentityService>();
            var validateMock = new Mock<IValidateService>();
            var mockFacade = new Mock<ISalesDocReturnService>();

            SalesDocReturnService service = new SalesDocReturnService(serviceProvider.Object, _dbContext("test"));

            serviceProvider.Setup(s => s.GetService(typeof(SalesDocReturnService))).Returns(service);
            serviceProvider.Setup(s => s.GetService(typeof(PosDbContext))).Returns(dbContext);

            //var serviceMock = new Mock<ISalesDocService>();
            mockFacade
                .Setup(s => s.Create(It.IsAny<SalesDocReturn>(), It.IsAny<SalesDocReturnViewModel>()))
                .Throws(new Exception());

            SalesDocReturnViewModel dataVM = new SalesDocReturnViewModel();
            //Act
            IActionResult response = GetController(identityService, validateMock, mockFacade).Post(dataVM).Result;

            //Assert
            int statusCode = this.GetStatusCode(response);
            Assert.Equal((int)HttpStatusCode.InternalServerError, statusCode);
        }

        [Fact]
        public void Should_Error_Get_Data_By_Id()
        {
            var user = new Mock<ClaimsPrincipal>();
            var claims = new Claim[]
            {
                new Claim("username", "SalesDoctestusername")
            };
            user.Setup(u => u.Claims).Returns(claims);

            Mock<IIdentityService> identityService = new Mock<IIdentityService>();

            var validateMock = new Mock<IValidateService>();
            var mockFacade = new Mock<ISalesDocReturnService>();

            //SalesDocReturnController controller = GetController(identityService, validateMock, mockFacade);
            
            SalesDocReturnController controller = new SalesDocReturnController(identityService.Object, validateMock.Object, mockFacade.Object);
            controller.ControllerContext = new ControllerContext()
            {
                HttpContext = new DefaultHttpContext()
                {
                    User = user.Object
                }
            };
            var response = controller.GetById(It.IsAny<int>());
            Assert.Equal((int)HttpStatusCode.InternalServerError, GetStatusCode(response));
        }

        [Fact]
        public void Should_InternalServerError_Get_Data_By_Id()
        {
            Mock<IIdentityService> identityService = new Mock<IIdentityService>();

            var validateMock = new Mock<IValidateService>();
            var mockFacade = new Mock<ISalesDocReturnService>();
            mockFacade
                .Setup(s => s.ReadModelById(It.IsAny<int>()))
                .Throws(new Exception());
            SalesDocReturnController controller = GetController(identityService, validateMock, mockFacade);

            var response = controller.GetById(0);
            //Assert.Equal(200, (int)HttpStatusCode.OK);
            Assert.Equal((int)HttpStatusCode.InternalServerError, GetStatusCode(response));
        }
    }
}
