using Melbeez.Business.Managers.Abstractions;
using Melbeez.Business.Models.Common;
using Melbeez.Business.Models.UserModels.ResponseModels;
using Melbeez.Common.Extensions;
using Melbeez.Common.Models;
using Melbeez.Common.Models.Entities;
using Melbeez.Common.Services.Abstraction;
using Melbeez.Data.UnitOfWork;
using Melbeez.Domain.Entities;
using Melbeez.Domain.Entities.Identity;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Melbeez.Business.Managers
{
    public class ContactusManager : IContactusManager
    {
        private readonly IUnitOfWork unitOfWork;
        private readonly UserManager<ApplicationUser> userManager;
        private readonly IConfiguration configuration;
        private readonly IEmailSenderService emailSenderService;
        private readonly IWebHostEnvironment environment;
        private readonly Dictionary<string, string> orderByTranslations = new Dictionary<string, string>();

        public ContactusManager(
            IUnitOfWork unitOfWork,
            UserManager<ApplicationUser> userManager,
            IConfiguration configuration,
            IEmailSenderService emailSenderService,
            IWebHostEnvironment environment
        )
        {
            this.userManager = userManager;
            this.unitOfWork = unitOfWork;
            this.configuration = configuration;
            this.emailSenderService = emailSenderService;
            this.environment = environment;
        }
        public async Task<ManagerBaseResponse<IEnumerable<ContactusResponseModel>>> Get(PagedListCriteria pagedListCriteria)
        {
            var mediaBaseUrl = configuration.GetValue<string>("MediaUploadConfiguration:MediaBaseUrl");
            var result = await unitOfWork.ContactusRepository
                         .GetQueryable(x => !x.IsDeleted && !x.applicationUser.IsDeleted)
                         .Include(x => x.applicationUser)
                         .Select(x => new ContactusResponseModel()
                         {
                             Id = x.Id,
                             Email = x.applicationUser.Email,
                             Subject = x.Subject,
                             Message = x.Message,
                             Image = !string.IsNullOrEmpty(x.Image) ? mediaBaseUrl + x.Image : x.Image,
                             CreatedOn = x.CreatedOn
                         })
                         .WhereIf(!string.IsNullOrWhiteSpace(pagedListCriteria.SearchText), x => x.Email.ToLower().Contains(pagedListCriteria.SearchText.ToLower())
                                                                                            || x.Subject.ToLower().Contains(pagedListCriteria.SearchText.ToLower())
                                                                                            || x.Message.ToLower().Contains(pagedListCriteria.SearchText.ToLower()))
                         .AsNoTracking()
                         .ToPagedListAsync(pagedListCriteria, orderByTranslations);
            return new ManagerBaseResponse<IEnumerable<ContactusResponseModel>>()
            {
                Result = result.Data,
                PageDetail = new PageDetailModel
                {
                    Skip = pagedListCriteria.Skip,
                    Take = pagedListCriteria.Take,
                    Count = result.TotalCount,
                    SearchText = pagedListCriteria.SearchText
                }
            };
        }
        public async Task<ManagerBaseResponse<bool>> AddContactusData(ContactusRequestModel model, string userId)
        {
            try
            {
                var dailyLimit = Convert.ToInt32(configuration.GetValue<string>("ContactUsDailyLimit"));
                DateTime startDateTime = DateTime.Today; //Today at 00:00:00
                DateTime endDateTime = DateTime.Today.AddDays(1).AddTicks(-1); //Today at 23:59:59
                var todaysCount = unitOfWork.ContactusRepository.GetQueryable()
                                  .Where(x => !x.IsDeleted && x.CreatedOn >= startDateTime.ToUniversalTime()
                                             && x.CreatedOn <= endDateTime.ToUniversalTime()
                                             && x.CreatedBy == userId)
                                  .ToList()
                                  .Count();
                if (todaysCount >= dailyLimit)
                {
                    return new ManagerBaseResponse<bool>()
                    {
                        Message = "You are only allowed to send " + dailyLimit + " inquiries in a day. Please contact us later.",
                        Result = false
                    };
                }

                if (model.Id == 0)
                {
                    var response = await unitOfWork.ContactusRepository.AddAsync(new ContactUsEntity()
                    {
                        Id = model.Id,
                        Subject = model.Subject,
                        Message = model.Message,
                        Image = model.Image,
                        IsDeleted = false,
                        CreatedBy = userId,
                        CreatedOn = DateTime.UtcNow,
                    });
                    await unitOfWork.CommitAsync();

                    if (response.Id != 0)
                    {
                        var user = userManager.Users.FirstOrDefault(s => s.Id == userId);
                        var htmlContent = string.Empty;
                        if (user != null)
                        {
                            htmlContent = File.ReadAllText(Path.Combine(environment.WebRootPath, "MailTemplates/Contact_us.html"));
                            htmlContent = htmlContent.Replace("{name}", string.Concat(user.FirstName, " ", user.LastName));
                            htmlContent = htmlContent.Replace("{email}", user.Email);
                            htmlContent = htmlContent.Replace("{message}", model.Message);
                        }
                        else
                        {
                            htmlContent = model.Message;
                        }
                        var attachmentPath = string.Empty;
                        var uploadTo = configuration.GetValue<string>("MediaUploadConfiguration:UploadTo");
                        if (uploadTo == "AWSS3")
                        {
                            attachmentPath = !string.IsNullOrEmpty(response.Image)
                                             ? configuration.GetValue<string>("MediaUploadConfiguration:MediaBaseUrl") + response.Image
                                             : null;
                        }
                        else
                        {
                            attachmentPath = !string.IsNullOrEmpty(response.Image)
                                             ? Path.Combine(configuration.GetValue<string>("MediaUploadConfiguration:BasePath"), response.Image)
                                             : null;
                        }
                        
                        string[] emailIds = configuration["SendContactusMessage"].ToString().Split(',', ' ');
                        foreach (var emailId in emailIds)
                        {
                            await emailSenderService.SendMail(emailId, model.Subject, htmlContent, attachmentPath, model.AttachImageName);
                        }
                    }
                }
                else
                {
                    return new ManagerBaseResponse<bool>()
                    {
                        Message = "Invalid data request.",
                        Result = false
                    };
                }
                return new ManagerBaseResponse<bool>()
                {
                    Message = "Thank you for your message, our customer support team will review your message and get back to you shortly",
                    Result = true
                };
            }
            catch (Exception ex)
            {
                return new ManagerBaseResponse<bool>()
                {
                    Message = ex.Message,
                    Result = false
                };
            }
        }
    }
}
