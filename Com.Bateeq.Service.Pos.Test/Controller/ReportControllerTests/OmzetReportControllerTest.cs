using Com.Danliris.Service.Inventory.Lib;
using Com.Danliris.Service.Inventory.Lib.Services;
using Com.Bateeq.Service.Pos.Lib.Models.SalesDoc;
using Com.Bateeq.Service.Pos.Lib.Services.SalesDocService;
using Com.Bateeq.Service.Pos.WebApi.Controllers.v1.ReportControllers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.CompilerServices;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Com.Bateeq.Service.Pos.Test.Controller.ReportControllerTests
{
	public class OmzetReportControllerTest
	{
		protected OmzetReportController GetController(IIdentityService identityService, IValidateService validateService, ISalesDocService service)
		{
			var user = new Mock<ClaimsPrincipal>();
			var claims = new Claim[]
			{
				new Claim("username", "OmzetReporttestusername")
			};
			user.Setup(u => u.Claims).Returns(claims);

			OmzetReportController controller = new OmzetReportController(identityService, validateService, service);
			controller.ControllerContext = new ControllerContext()
			{
				HttpContext = new DefaultHttpContext()
				{
					User = user.Object
				}
			};
			controller.ControllerContext.HttpContext.Request.Headers["Authorization"] = "Bearer OmzetReporttesttoken";
			controller.ControllerContext.HttpContext.Request.Path = new PathString("/v1/OmzetReport-test");
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

		public SalesDoc GetTestData(PosDbContext dbContext)
		{
			SalesDoc data = new SalesDoc();
			dbContext.SalesDocs.Add(data);
			dbContext.SaveChanges();

			return data;
		}

		protected int GetStatusCode(IActionResult response)
		{
			return (int)response.GetType().GetProperty("StatusCode").GetValue(response, null);
		}


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
		public void Get_Return_Data()
		{
			//Setup
			PosDbContext dbContext = _dbContext(GetCurrentAsyncMethod());
			Mock<IServiceProvider> serviceProvider = GetServiceProvider();
			var validateService = new Mock<IValidateService>();
			Mock<IIdentityService> identityService = new Mock<IIdentityService>();

			SalesDocService service = new SalesDocService(serviceProvider.Object, _dbContext("test"));

			serviceProvider.Setup(s => s.GetService(typeof(SalesDocService))).Returns(service);
			serviceProvider.Setup(s => s.GetService(typeof(PosDbContext))).Returns(dbContext);

			SalesDoc testData = GetTestData(dbContext);

			//Act
			IActionResult response = GetController(identityService.Object, validateService.Object, service).GetById("1",It.IsAny<DateTimeOffset>(), It.IsAny<DateTimeOffset>(),"1");

			//Assert
			int statusCode = this.GetStatusCode(response);
			Assert.NotEqual((int)HttpStatusCode.NotFound, statusCode);
		}
		[Fact]
		public void Get_Return_InternalException()
		{
			//Setup
			PosDbContext dbContext = _dbContext(GetCurrentAsyncMethod());
			Mock<IServiceProvider> serviceProvider = GetServiceProvider();
			var validateService = new Mock<IValidateService>();
			Mock<IIdentityService> identityService = new Mock<IIdentityService>();

			SalesDocService service = new SalesDocService(serviceProvider.Object, _dbContext("test"));

			serviceProvider.Setup(s => s.GetService(typeof(SalesDocService))).Returns(service);
			serviceProvider.Setup(s => s.GetService(typeof(PosDbContext))).Returns(dbContext);

			SalesDoc testData = GetTestData(dbContext);

			//Act
			IActionResult result = GetController(identityService.Object, validateService.Object, service).GetById("1", It.IsAny<DateTimeOffset>(), It.IsAny<DateTimeOffset>(), "");

			//Assert
			GetStatusCode(result).Equals((int)HttpStatusCode.InternalServerError);
		}
		[Fact]
		public async Task GetXLSBehavior()
		{
			PosDbContext dbContext = _dbContext(GetCurrentAsyncMethod());
			Mock<IServiceProvider> serviceProvider = GetServiceProvider();
			var validateService = new Mock<IValidateService>();
			Mock<IIdentityService> identityService = new Mock<IIdentityService>();

			SalesDocService service = new SalesDocService(serviceProvider.Object, _dbContext("test"));

			serviceProvider.Setup(s => s.GetService(typeof(SalesDocService))).Returns(service);
			serviceProvider.Setup(s => s.GetService(typeof(PosDbContext))).Returns(dbContext);

			SalesDoc testData = GetTestData(dbContext);
 
		   var result = GetController(identityService.Object, validateService.Object, service).GetXls("1", DateTime.Now, DateTime.Now, "1");


			Assert.Equal("application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", result.GetType().GetProperty("ContentType").GetValue(result, null));

		}
		[Fact]
		public async Task GetXLSInternalException()
		{
			PosDbContext dbContext = _dbContext(GetCurrentAsyncMethod());
			Mock<IServiceProvider> serviceProvider = GetServiceProvider();
			var validateService = new Mock<IValidateService>();
			Mock<IIdentityService> identityService = new Mock<IIdentityService>();

			SalesDocService service = new SalesDocService(serviceProvider.Object, _dbContext("test"));

			serviceProvider.Setup(s => s.GetService(typeof(SalesDocService))).Returns(service);
			serviceProvider.Setup(s => s.GetService(typeof(PosDbContext))).Returns(dbContext);

			SalesDoc testData = GetTestData(dbContext);

		
			var result = GetController(identityService.Object, validateService.Object, service).GetXls("1", DateTime.Now, DateTime.Now, "");

			GetStatusCode(result).Equals((int)HttpStatusCode.InternalServerError);

		}
	}
}
