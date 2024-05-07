using Melbeez.Business.Managers.Abstractions;
using Melbeez.Business.Models.Common;
using Microsoft.AspNetCore.Hosting;
using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Melbeez.Business.Models.UserModels.RequestModels;
using Melbeez.Business.Models.UserModels.ResponseModels;

namespace Melbeez.Business.Managers
{
    public class CookiePolicyManager : ICookiePolicyManager
    {
        private readonly IWebHostEnvironment environment;
        private readonly IAdminTransactionLogManager _adminTransactionLogManager;

        public CookiePolicyManager(IWebHostEnvironment environment, IAdminTransactionLogManager adminTransactionLogManager)
        {
            this.environment = environment;
            _adminTransactionLogManager = adminTransactionLogManager;
        }
        public async Task<ManagerBaseResponse<string>> Get()
        {
            var cookiePolicyFilePath = Path.Combine(environment.WebRootPath, "Documents/cookie-policy.html");
            if (File.Exists(cookiePolicyFilePath))
            {
                var cookiePolicyContent = File.ReadAllText(cookiePolicyFilePath);
                return new ManagerBaseResponse<string>()
                {
                    Result = cookiePolicyContent,
                    IsSuccess = true
                };
            }
            return new ManagerBaseResponse<string>()
            {
                Result = "File not found.",
                StatusCode = 404
            };
        }
        public async Task<ManagerBaseResponse<CookiePolicyRequestModel>> GetCookiePolicyForWeb()
        {
            var draftFilePath = Path.Combine(environment.WebRootPath, "Documents/cookie-policy_Draft.html");
            if (File.Exists(draftFilePath))
            {
                var cookiePolicyContent = File.ReadAllText(draftFilePath);
                var model = new CookiePolicyRequestModel()
                {
                    Base64Content = Convert.ToBase64String(Encoding.ASCII.GetBytes(cookiePolicyContent)),
                    IsDraft = true
                };
                return new ManagerBaseResponse<CookiePolicyRequestModel>()
                {
                    Result = model,
                    IsSuccess = true
                };
            }
            else
            {
                var cookiePolicyFilePath = Path.Combine(environment.WebRootPath, "Documents/cookie-policy.html");
                if (File.Exists(cookiePolicyFilePath))
                {
                    var cookiePolicyContent = File.ReadAllText(cookiePolicyFilePath);
                    var model = new CookiePolicyRequestModel()
                    {
                        Base64Content = Convert.ToBase64String(Encoding.ASCII.GetBytes(cookiePolicyContent)),
                        IsDraft = false
                    };
                    return new ManagerBaseResponse<CookiePolicyRequestModel>()
                    {
                        Result = model,
                        IsSuccess = true
                    };
                }
            }
            return new ManagerBaseResponse<CookiePolicyRequestModel>()
            {
                Result = null,
                Message = "File not found.",
                StatusCode = 404
            };
        }
        public async Task<ManagerBaseResponse<CookiePolicyRequestModel>> WrtieInFile(CookiePolicyRequestModel model, string userId)
        {
            if (model.IsDraft)
            {
                var htmlContent = Encoding.UTF8.GetString(Convert.FromBase64String(model.Base64Content));
                var filePath = Path.Combine(environment.WebRootPath, "Documents/cookie-policy_Draft.html");
                if (!File.Exists(filePath))
                {
                    using (StreamWriter sw = File.CreateText(filePath))
                    {
                        sw.Dispose();
                    }
                }

                File.WriteAllText(filePath, htmlContent);
                model.IsDraft = true;
                await _adminTransactionLogManager.AddTransactionLog(
                                                    new AdminTransactionLogResponseModel()
                                                    {
                                                        UserId = null,
                                                        TransactionDescription = "Cookie Policy saved as draft",
                                                        OldStatus = null,
                                                        NewStatus = "Draft",
                                                    }, userId);

                return new ManagerBaseResponse<CookiePolicyRequestModel>()
                {
                    Result = model,
                    Message = "Cookie Policy saved as draft successfully."
                };
            }
            else
            {
                var htmlContent = Encoding.UTF8.GetString(Convert.FromBase64String(model.Base64Content));
                var publishedCookiePolicyFilePath = Path.Combine(environment.WebRootPath, "Documents/cookie-policy.html");
                if (File.Exists(publishedCookiePolicyFilePath))
                {
                    File.WriteAllText(publishedCookiePolicyFilePath, htmlContent);

                    var draftCookiePolicyFilePath = Path.Combine(environment.WebRootPath, "Documents/cookie-policy_Draft.html");
                    if (File.Exists(draftCookiePolicyFilePath))
                    {
                        File.Delete(draftCookiePolicyFilePath);
                    }

                    model.IsDraft = false;
                    await _adminTransactionLogManager.AddTransactionLog(
                                                    new AdminTransactionLogResponseModel()
                                                    {
                                                        UserId = null,
                                                        TransactionDescription = "Cookie Policy published",
                                                        OldStatus = "Draft",
                                                        NewStatus = "Published",
                                                    }, userId);
                    return new ManagerBaseResponse<CookiePolicyRequestModel>()
                    {
                        Result = model,
                        Message = "Cookie Policy published successfully."
                    };
                }
            }
            return new ManagerBaseResponse<CookiePolicyRequestModel>()
            {
                Result = null,
                Message = "File not found.",
                StatusCode = 404
            };
        }
    }
}
