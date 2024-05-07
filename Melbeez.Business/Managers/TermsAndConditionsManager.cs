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
    public class TermsAndConditionsManager : ITermsAndConditionsManager
    {
        private readonly IWebHostEnvironment environment;
        private readonly IAdminTransactionLogManager _adminTransactionLogManager;
        public TermsAndConditionsManager(IWebHostEnvironment environment, IAdminTransactionLogManager adminTransactionLogManager)
        {
            this.environment = environment;
            _adminTransactionLogManager = adminTransactionLogManager;
        }
        public async Task<ManagerBaseResponse<string>> Get()
        {
            var privacyPolicyFilePath = Path.Combine(environment.WebRootPath, "Documents/terms-and-conditions.html");
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

        public async Task<ManagerBaseResponse<TermsAndConditionsRequestModel>> GetTermAndConditionsForWeb()
        {
            var draftTermAndConditionFilePath = Path.Combine(environment.WebRootPath, "Documents/terms-and-conditions_Draft.html");
            if (File.Exists(draftTermAndConditionFilePath))
            {
                var termAndConditionContent = File.ReadAllText(draftTermAndConditionFilePath);
                var model = new TermsAndConditionsRequestModel()
                {
                    Base64Content = Convert.ToBase64String(Encoding.ASCII.GetBytes(termAndConditionContent)),
                    IsDraft = true
                };
                return new ManagerBaseResponse<TermsAndConditionsRequestModel>()
                {
                    Result = model,
                    IsSuccess = true
                };
            }
            else
            {
                var termAndConditionFilePath = Path.Combine(environment.WebRootPath, "Documents/terms-and-conditions.html");
                if (File.Exists(termAndConditionFilePath))
                {
                    var termAndConditionContent = File.ReadAllText(termAndConditionFilePath);
                    var model = new TermsAndConditionsRequestModel()
                    {
                        Base64Content = Convert.ToBase64String(Encoding.ASCII.GetBytes(termAndConditionContent)),
                        IsDraft = false
                    };
                    return new ManagerBaseResponse<TermsAndConditionsRequestModel>()
                    {
                        Result = model,
                        IsSuccess = true
                    };
                }
            }
            return new ManagerBaseResponse<TermsAndConditionsRequestModel>()
            {
                Result = null,
                Message = "File not found.",
                StatusCode = 404
            };
        }
        public async Task<ManagerBaseResponse<TermsAndConditionsRequestModel>> WrtieInFile(TermsAndConditionsRequestModel model, string userId)
        {
            if (model.IsDraft)
            {
                var htmlContent = Encoding.UTF8.GetString(Convert.FromBase64String(model.Base64Content));
                var filePath = Path.Combine(environment.WebRootPath, "Documents/terms-and-conditions_Draft.html");
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
                                                        TransactionDescription = "Term & Conditions saved as draft",
                                                        OldStatus = null,
                                                        NewStatus = "Draft",
                                                    }, userId);
                return new ManagerBaseResponse<TermsAndConditionsRequestModel>()
                {
                    Result = model,
                    Message = "Term & Conditions saved as draft successfully."
                };
            }
            else
            {
                var htmlContent = Encoding.UTF8.GetString(Convert.FromBase64String(model.Base64Content));
                var publishedFilePath = Path.Combine(environment.WebRootPath, "Documents/terms-and-conditions.html");
                if (File.Exists(publishedFilePath))
                {
                    File.WriteAllText(publishedFilePath, htmlContent);

                    var draftFilePath = Path.Combine(environment.WebRootPath, "Documents/terms-and-conditions_Draft.html");
                    if (File.Exists(draftFilePath))
                    {
                        File.Delete(draftFilePath);
                    }

                    model.IsDraft = false;
                    await _adminTransactionLogManager.AddTransactionLog(
                                                    new AdminTransactionLogResponseModel()
                                                    {
                                                        UserId = null,
                                                        TransactionDescription = "Term & Conditions published",
                                                        OldStatus = "Draft",
                                                        NewStatus = "Published",
                                                    }, userId);
                    return new ManagerBaseResponse<TermsAndConditionsRequestModel>()
                    {
                        Result = model,
                        Message = "Term & Conditions published successfully."
                    };
                }
            }
            return new ManagerBaseResponse<TermsAndConditionsRequestModel>()
            {
                Result = null,
                Message = "File not found.",
                StatusCode = 404
            };
        }
    }
}
