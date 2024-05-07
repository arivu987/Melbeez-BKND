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
    public class EulaManager : IEulaManager
    {
        private readonly IWebHostEnvironment environment;
        private readonly IAdminTransactionLogManager _adminTransactionLogManager;

        public EulaManager(IWebHostEnvironment environment, IAdminTransactionLogManager adminTransactionLogManager)
        {
            this.environment = environment;
            _adminTransactionLogManager = adminTransactionLogManager;
        }
        public async Task<ManagerBaseResponse<string>> Get()
        {
            var eulaFilePath = Path.Combine(environment.WebRootPath, "Documents/eula.html");
            if (File.Exists(eulaFilePath))
            {
                var eulaContent = File.ReadAllText(eulaFilePath);
                return new ManagerBaseResponse<string>()
                {
                    Result = eulaContent,
                    IsSuccess = true
                };
            }
            return new ManagerBaseResponse<string>()
            {
                Result = "File not found.",
                StatusCode = 404
            };
        }
        public async Task<ManagerBaseResponse<CookiePolicyRequestModel>> GetEulaForWeb()
        {
            var draftFilePath = Path.Combine(environment.WebRootPath, "Documents/eula_Draft.html");
            if (File.Exists(draftFilePath))
            {
                var eulaContent = File.ReadAllText(draftFilePath);
                var model = new CookiePolicyRequestModel()
                {
                    Base64Content = Convert.ToBase64String(Encoding.ASCII.GetBytes(eulaContent)),
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
                var eulaFilePath = Path.Combine(environment.WebRootPath, "Documents/eula.html");
                if (File.Exists(eulaFilePath))
                {
                    var eulaContent = File.ReadAllText(eulaFilePath);
                    var model = new CookiePolicyRequestModel()
                    {
                        Base64Content = Convert.ToBase64String(Encoding.ASCII.GetBytes(eulaContent)),
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
                var filePath = Path.Combine(environment.WebRootPath, "Documents/eula_Draft.html");
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
                                                        TransactionDescription = "Eula saved as draft",
                                                        OldStatus = null,
                                                        NewStatus = "Draft",
                                                    }, userId);

                return new ManagerBaseResponse<CookiePolicyRequestModel>()
                {
                    Result = model,
                    Message = "Eula saved as draft successfully."
                };
            }
            else
            {
                var htmlContent = Encoding.UTF8.GetString(Convert.FromBase64String(model.Base64Content));
                var publishedEulaFilePath = Path.Combine(environment.WebRootPath, "Documents/eula.html");
                if (File.Exists(publishedEulaFilePath))
                {
                    File.WriteAllText(publishedEulaFilePath, htmlContent);

                    var draftEulaFilePath = Path.Combine(environment.WebRootPath, "Documents/eula_Draft.html");
                    if (File.Exists(draftEulaFilePath))
                    {
                        File.Delete(draftEulaFilePath);
                    }

                    model.IsDraft = false;
                    await _adminTransactionLogManager.AddTransactionLog(
                                                    new AdminTransactionLogResponseModel()
                                                    {
                                                        UserId = null,
                                                        TransactionDescription = "Eula published",
                                                        OldStatus = "Draft",
                                                        NewStatus = "Published",
                                                    }, userId);
                    return new ManagerBaseResponse<CookiePolicyRequestModel>()
                    {
                        Result = model,
                        Message = "Eula published successfully."
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
