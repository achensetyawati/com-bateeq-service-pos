using AutoMapper;
using Com.Bateeq.Service.Pos.Lib.Models.SalesDoc;
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
    [Route("v{version:apiVersion}/sales/omzet-monitoring")]
    [Authorize]

    public class OmzetReportController : Controller
    {
        protected IIdentityService IdentityService;
        protected readonly IValidateService ValidateService;
        //public readonly IServiceProvider serviceProvider;
        protected readonly ISalesDocService Service;
        protected readonly string ApiVersion;

        public OmzetReportController(IIdentityService identityService, IValidateService validateService, ISalesDocService service)
        {
            //this.serviceProvider = serviceProvider;
            IdentityService = identityService;
            ValidateService = validateService;
            Service = service;
            ApiVersion = "1.0.0";
        }

        protected void VerifyUser()
        {
            IdentityService.Username = User.Claims.ToArray().SingleOrDefault(p => p.Type.Equals("username")).Value;
            IdentityService.Token = Request.Headers["Authorization"].FirstOrDefault().Replace("Bearer ", "");
            IdentityService.TimezoneOffset = Convert.ToInt32(Request.Headers["x-timezone-offset"]);
        }

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
        public IActionResult GetById(string storecode, DateTimeOffset dateFrom, DateTimeOffset dateTo, string shift)
        {
            try
            {
				VerifyUser();


				List<SalesDoc> model = Service.OmzetReport(storecode, dateFrom, dateTo, shift);


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


        [HttpGet("download")]
        public IActionResult GetXls(string storecode, DateTime? dateFrom, DateTime? dateTo, string shift)
        {

            try
            {
				VerifyUser();
				byte[] xlsInBytes;
                int offset = Convert.ToInt32(Request.Headers["x-timezone-offset"]);
                DateTime DateFrom = dateFrom == null ? new DateTime(1970, 1, 1) : Convert.ToDateTime(dateFrom);
                DateTime DateTo = dateTo == null ? DateTime.Now : Convert.ToDateTime(dateTo);
                string filename;

                var xls = Service.GenerateExcelOmzet(storecode, DateFrom, DateTo, shift);


                filename = String.Format("Laporan Penjualan - Accounting - {0}.xlsx", DateTime.UtcNow.ToString("dd-MMM-yyyy"));

                xlsInBytes = xls.ToArray();
                var file = File(xlsInBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", filename);
                return file;

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

