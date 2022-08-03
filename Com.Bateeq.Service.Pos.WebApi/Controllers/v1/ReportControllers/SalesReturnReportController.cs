using AutoMapper;
using Com.Bateeq.Service.Pos.Lib.Models.SalesDoc;
using Com.Bateeq.Service.Pos.Lib.Services.SalesDocReturnService;
using Com.Bateeq.Service.Pos.Lib.Services.SalesDocService;
using Com.Bateeq.Service.Pos.Lib.ViewModels.SalesDoc;
using Com.Danliris.Service.Inventory.Lib.Services;
using Com.Danliris.Service.Inventory.WebApi.Helpers;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Com.Bateeq.Service.Pos.WebApi.Controllers.v1.ReportControllers
{

    [Produces("application/json")]
    [ApiVersion("1.0")]
    [Route("v{version:apiVersion}/sales/sales-return")]
    [Authorize]

    public class SalesReturnReportController : Controller
    {
        protected IIdentityService IdentityService;
        protected readonly IValidateService ValidateService;
        //public readonly IServiceProvider serviceProvider;
        protected readonly ISalesDocReturnService Service;
        protected readonly string ApiVersion;

        public SalesReturnReportController(IIdentityService identityService, IValidateService validateService, ISalesDocReturnService service)
        {
            //this.serviceProvider = serviceProvider;
            IdentityService = identityService;
            ValidateService = validateService;
            Service = service;
            ApiVersion = "1.0.0";
        }

        //protected void VerifyUser()
        //{
        //    IdentityService.Username = User.Claims.ToArray().SingleOrDefault(p => p.Type.Equals("username")).Value;
        //    IdentityService.Token = Request.Headers["Authorization"].FirstOrDefault().Replace("Bearer ", "");
        //    IdentityService.TimezoneOffset = Convert.ToInt32(Request.Headers["x-timezone-offset"]);
        //}

        //[HttpGet]
        //public IActionResult Get(string storecode, DateTimeOffset dateFrom, DateTimeOffset dateTo, string shift, string info, int offset, string username, int page = 1, int size = 25, string Order = "{}")
        //{
        //    try
        //    {
        //       // var shift1 = shift == "0" ? "" : shift;
        //        VerifyUser();
        //        var data = Service.GetOmzetReport(storecode, dateFrom, dateTo, shift, info, offset, username, page, size, Order);


        //        return Ok(new
        //        {
        //            apiVersion = ApiVersion,
        //            data = data.Item1,
        //            info = new { count = data.Item2, total = data.Item2 },
        //            message = General.OK_MESSAGE,
        //            statusCode = General.OK_STATUS_CODE
        //        });
        //    }
        //    catch (Exception e)
        //    {
        //        Dictionary<string, object> Result =
        //            new ResultFormatter(ApiVersion, General.INTERNAL_ERROR_STATUS_CODE, e.Message)
        //            .Fail();
        //        return StatusCode(General.INTERNAL_ERROR_STATUS_CODE, Result);
        //    }

        //}

        [HttpGet]
        public IActionResult Read(string storecode, DateTimeOffset dateFrom, DateTimeOffset dateTo, string shift)
        {
            try
            {

                List<SalesDoc> model = Service.SalesReturnReport(storecode, dateFrom, dateTo, shift);


                Dictionary<string, object> Result =
                    new ResultFormatter(ApiVersion, General.OK_STATUS_CODE, General.OK_MESSAGE)
                    .Ok<SalesDoc, SalesDocViewModel>(model, Service.MaptoViewModel);
                return Ok(Result);

            }
            catch (Exception e)
            {
                Dictionary<string, object> Result =
                    new ResultFormatter(ApiVersion, General.INTERNAL_ERROR_STATUS_CODE, e.Message)
                    .Fail();
                return StatusCode(General.INTERNAL_ERROR_STATUS_CODE, Result);
            }
        }
    }
}

