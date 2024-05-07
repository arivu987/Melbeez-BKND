using Melbeez.Business.Managers.Abstractions;
using Melbeez.Business.Models.Common;
using Melbeez.Business.Models.UserModels.RequestModels;
using Melbeez.Business.Models.UserModels.ResponseModels;
using Microsoft.AspNetCore.Hosting;
using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace Melbeez.Business.Managers
{
    public class PrivacyPolicyManager : IPrivacyPolicyManager
    {
        private readonly IWebHostEnvironment environment;
        private readonly IAdminTransactionLogManager _adminTransactionLogManager;
        public PrivacyPolicyManager(IWebHostEnvironment environment, IAdminTransactionLogManager adminTransactionLogManager)
        {
            this.environment = environment;
            _adminTransactionLogManager = adminTransactionLogManager;
        }
        public async Task<ManagerBaseResponse<string>> Get()
        {
            var privacyPolicyFilePath = Path.Combine(environment.WebRootPath, "Documents/privacy.html");
            if (File.Exists(privacyPolicyFilePath))
            {
                var privacyPolicyContent = File.ReadAllText(privacyPolicyFilePath);
                return new ManagerBaseResponse<string>()
                {
                    Result = privacyPolicyContent,
                    IsSuccess = true
                };
            }
            return new ManagerBaseResponse<string>()
            {
                Result = "File not found.",
                StatusCode = 404
            };
        }
        public async Task<ManagerBaseResponse<PrivacyPolicyRequestModel>> GetPrivacyPolicyForWeb()
        {
            var draftFilePath = Path.Combine(environment.WebRootPath, "Documents/privacy_Draft.html");
            if (File.Exists(draftFilePath))
            {
                var privacyPolicyContent = File.ReadAllText(draftFilePath);
                var model = new PrivacyPolicyRequestModel()
                {
                    Base64Content = Convert.ToBase64String(Encoding.ASCII.GetBytes(privacyPolicyContent)),
                    IsDraft = true
                };
                return new ManagerBaseResponse<PrivacyPolicyRequestModel>()
                {
                    Result = model,
                    IsSuccess = true
                };
            }
            else
            {
                var privacyPolicyFilePath = Path.Combine(environment.WebRootPath, "Documents/privacy.html");
                if (File.Exists(privacyPolicyFilePath))
                {
                    var privacyPolicyContent = File.ReadAllText(privacyPolicyFilePath);
                    var model = new PrivacyPolicyRequestModel()
                    {
                        Base64Content = Convert.ToBase64String(Encoding.ASCII.GetBytes(privacyPolicyContent)),
                        IsDraft = false
                    };
                    return new ManagerBaseResponse<PrivacyPolicyRequestModel>()
                    {
                        Result = model,
                        IsSuccess = true
                    };
                }
            }
            return new ManagerBaseResponse<PrivacyPolicyRequestModel>()
            {
                Result = null,
                Message = "File not found.",
                StatusCode = 404
            };
        }
        public async Task<ManagerBaseResponse<PrivacyPolicyRequestModel>> WrtieInFile(PrivacyPolicyRequestModel model, string userId)
        {
            if (model.IsDraft)
            {
                var htmlContent = Encoding.UTF8.GetString(Convert.FromBase64String(model.Base64Content));
                var filePath = Path.Combine(environment.WebRootPath, "Documents/privacy_Draft.html");
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
                                                        TransactionDescription = "Privacy Policy saved as draft",
                                                        OldStatus = null,
                                                        NewStatus = "Draft",
                                                    }, userId);

                return new ManagerBaseResponse<PrivacyPolicyRequestModel>()
                {
                    Result = model,
                    Message = "Privacy Policy saved as draft successfully."
                };
            }
            else
            {
                var htmlContent = Encoding.UTF8.GetString(Convert.FromBase64String(model.Base64Content));
                var publishedPolicyFilePath = Path.Combine(environment.WebRootPath, "Documents/privacy.html");
                if (File.Exists(publishedPolicyFilePath))
                {
                    File.WriteAllText(publishedPolicyFilePath, htmlContent);

                    var draftPolicyFilePath = Path.Combine(environment.WebRootPath, "Documents/privacy_Draft.html");
                    if (File.Exists(draftPolicyFilePath))
                    {
                        File.Delete(draftPolicyFilePath);
                    }

                    model.IsDraft = false;
                    await _adminTransactionLogManager.AddTransactionLog(
                                                    new AdminTransactionLogResponseModel()
                                                    {
                                                        UserId = null,
                                                        TransactionDescription = "Privacy Policy published",
                                                        OldStatus = "Draft",
                                                        NewStatus = "Published",
                                                    }, userId);
                    return new ManagerBaseResponse<PrivacyPolicyRequestModel>()
                    {
                        Result = model,
                        Message = "Privacy Policy published successfully."
                    };
                }
            }
            return new ManagerBaseResponse<PrivacyPolicyRequestModel>()
            {
                Result = null,
                Message = "File not found.",
                StatusCode = 404
            };
        }
    }
}
