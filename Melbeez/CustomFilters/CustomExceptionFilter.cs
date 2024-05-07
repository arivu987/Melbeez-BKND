using Melbeez.Business.Common.Exceptions;
using Melbeez.Business.Common.Services;
using Melbeez.Business.Models.Common;
using Melbeez.Common.Services.Abstraction;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.Net;
using System.Threading.Tasks;

namespace Melbeez.CustomFilters
{
    public class CustomExceptionFilter : ExceptionFilterAttribute
    {
        private readonly IEmailSenderService emailSenderService;
        private readonly IConfiguration configuration;
        public CustomExceptionFilter(IEmailSenderService emailSenderService, IConfiguration configuration)
        {
            this.emailSenderService = emailSenderService;
            this.configuration = configuration;
        }
        public override void OnException(ExceptionContext context)
        {
            var exception = context.Exception;
            var NewGuid = Guid.NewGuid();


            if (exception is UnauthorizedAccessException)
            {
                context.HttpContext.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                context.Result = new JsonResult(new ApiBaseFailResponse()
                {
                    Message = exception.Message,
                    TrackId = NewGuid
                });
            }
            else if (exception is BadRequestException)
            {
                var customException = (BadRequestException)exception;

                context.HttpContext.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                context.Result = new JsonResult(new ApiBaseFailResponse()
                {
                    Message = customException.errorMessage,
                    TrackId = NewGuid
                });
            }
            else if (exception is NotFoundException)
            {
                var customException = (NotFoundException)exception;

                context.HttpContext.Response.StatusCode = (int)HttpStatusCode.NotFound;
                context.Result = new JsonResult(new ApiBaseFailResponse()
                {
                    Message = customException.errorMessage,
                    TrackId = NewGuid
                });
            }
            else
            {
                var RouteValues = context?.HttpContext.Request?.QueryString.ToString();
                var path = context.HttpContext.Request.Path.Value;
                Handle(exception, NewGuid.ToString(), RouteValues, path);

                context.HttpContext.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                context.Result = new JsonResult(new ApiBaseFailResponse()
                {
                    Message = exception.Message,
                    TrackId = NewGuid
                });
            }

            var newline = "<br/>";
            string ErrorlineNo = exception.StackTrace.Substring(exception.StackTrace.Length - 7, 7);
            string Errormsg = exception.GetType().Name.ToString();
            string extype = exception.GetType().ToString();
            string exurl = context.HttpContext.Request.Path.ToString();
            string ErrorLocation = exception.Message.ToString();
            string EmailHead = "<b>Dear Team,</b>" + "<br/>" + "An exception occurred in a Application Url" + " " + exurl + " " + "With following Details" + "<br/>" + "<br/>";
            string EmailSing = newline + "Thanks and Regards" + newline + "    " + "     " + "<b>Application Admin </b>" + "</br>";
            string Sub = "Exception occurred" + " " + "in Application" + " " + exurl;
            string loggedInUser = context?.HttpContext?.User?.Identity?.Name;
            string errortomail = EmailHead + "<b>Log Written Date: </b>" + " " + DateTime.Now.ToString("dd-MMM-yyyy hh:mm:ss ")
                                 + newline + "<b>Error Line No :</b>" + " " + ErrorlineNo + "\t\n" + " " 
                                 + newline + "<b>Error Message:</b>" + " " + Errormsg 
                                 + newline + "<b>Exception Type:</b>" + " " + extype 
                                 + newline + "<b> Error Details :</b>" + " " + ErrorLocation 
                                 + newline + "<b>Error Page Url:</b>" + " " + exurl
                                 + newline + "<b>Logged In User:</b>" + " " + loggedInUser
                                 + newline + newline + newline + newline + EmailSing;

            string[] emailIds = configuration["SendExceptionMessage"].ToString().Split(',',' ');
            foreach (var emailId in emailIds)
            {
                emailSenderService.SendMail(emailId, Sub, errortomail, null, null);
            }
            base.OnException(context);
        }
        void Handle(Exception customException, string trackId, string RouteValues, string path)
        {
            Task task = new Task(() =>
            {
                LoggerService.Log(trackId, "Exceptions");
                LoggerService.Log(RouteValues, "Exceptions");
                LoggerService.Log(path, "Exceptions");
                LoggerService.Log(JsonConvert.SerializeObject(customException), "Exceptions");
            });
            task.Start();
        }
    }
}
