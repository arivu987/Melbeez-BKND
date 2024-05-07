using Melbeez.Business.Managers.Abstractions;
using Melbeez.Business.Models.Common;
using Melbeez.Business.Models.UserModels.RequestModels;
using Melbeez.Common.Helpers;
using Melbeez.Data.UnitOfWork;
using Melbeez.Domain.Entities;
using Melbeez.Domain.Entities.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Melbeez.Business.Managers
{
    public class ItemTransferInvitationManager : IItemTransferInvitationManager
    {
        private readonly IUnitOfWork unitOfWork;
        private readonly IConfiguration configuration;
        private readonly IEmailManager emailManager;
        private readonly ISMSManager smsManager;
        private readonly UserManager<ApplicationUser> userManager;

        public ItemTransferInvitationManager(IUnitOfWork unitOfWork, 
            IConfiguration configuration, 
            IEmailManager emailManager, 
            ISMSManager smsManager,
            UserManager<ApplicationUser> userManager)
        {
            this.unitOfWork = unitOfWork;
            this.configuration = configuration;
            this.emailManager = emailManager;
            this.smsManager = smsManager;
            this.userManager = userManager;
        }
        public async Task<ManagerBaseResponse<bool>> InviteUser(ItemTransferInvitationRequestModel model, string userId)
        {
            if (model.IsProduct)
            {
                var productInfo = await unitOfWork.ProductsRepository
                                .GetQueryable(x => !x.IsDeleted && model.TransferItemId.Contains(x.Id) && x.CreatedBy == userId
                                              && x.Status != MovedStatus.Transferred
                                              && x.Status != MovedStatus.Initiated
                                              && x.Status != MovedStatus.Waiting)
                                .ToListAsync();

                if (!productInfo.Any())
                {
                    return new ManagerBaseResponse<bool>()
                    {
                        Message = "Product(s) already in transfer status!",
                        Result = false,
                        StatusCode = 500
                    };
                }
            }
            else
            {
                var locationInfo = await unitOfWork.LocationsRepository
                               .GetQueryable(x => !x.IsDeleted && model.TransferItemId.Contains(x.Id)
                                               && x.Status != MovedStatus.Transferred
                                               && x.Status != MovedStatus.Initiated
                                               && x.Status != MovedStatus.Waiting
                                               && x.CreatedBy == userId)
                               .ToListAsync();

                if (!locationInfo.Any())
                {
                    return new ManagerBaseResponse<bool>()
                    {
                        Message = "Location(s) already in transfer status!",
                        Result = false,
                        StatusCode = 500
                    };
                }

            }

            Random random = new Random();
            var transferId = random.Next(100000000, 999999999);
            string transferIdString = transferId.ToString("D9");

            var response = await unitOfWork.ItemTransferInvitationRepository
                .AddAsync(new ItemTransferInvitationEntity()
                {
                    Email = model.Email,
                    CountryCode = model.CountryCode,
                    PhoneNumber = model.PhoneNumber,
                    ItemId = string.Join(",", model.TransferItemId),
                    IsProduct = model.IsProduct,
                    TransferId = transferIdString,
                    ExpiredOn = DateTime.UtcNow.AddDays(7),
                    IsAccepted = false,
                    IsDeleted = false,
                    CreatedBy = userId,
                    CreatedOn = DateTime.UtcNow,
                });
            await unitOfWork.CommitAsync();

            if (response.Id > 0)
            {
                #region Update Transfer Item Status as Initiate

                foreach (var item in model.TransferItemId)
                {
                    if (model.IsProduct)
                    {
                        var productInfo = await unitOfWork.ProductsRepository
                                  .GetQueryable(x => !x.IsDeleted && x.Id == item && x.CreatedBy == userId
                                                && x.Status != MovedStatus.Transferred
                                                && x.Status != MovedStatus.Initiated
                                                && x.Status != MovedStatus.Waiting)
                                  .FirstOrDefaultAsync();
                        if (productInfo != null)
                        {
                            await unitOfWork.ItemTransferRepository
                                .AddAsync(new MovedItemStatusTransactonsEntity()
                                {
                                    TransferId = transferIdString,
                                    ItemId = item,
                                    FromUserId = userId,
                                    ToUserId = null,
                                    Status = MovedStatus.Initiated,
                                    IsProduct = model.IsProduct,
                                    ExpireOn = DateTime.UtcNow.AddMonths(1),
                                    IsDeleted = false,
                                    CreatedBy = userId,
                                    CreatedOn = DateTime.UtcNow,
                                    UpdatedBy = userId,
                                    UpdatedOn = DateTime.UtcNow
                                });
                            await unitOfWork.CommitAsync();

                            productInfo.IsMoving = true;
                            productInfo.Status = MovedStatus.Initiated;
                            await unitOfWork.CommitAsync();
                        }
                    }
                    else
                    {
                        var locationInfo = await unitOfWork.LocationsRepository
                                .GetQueryable(x => !x.IsDeleted && x.Id == item
                                                && x.Status != MovedStatus.Transferred
                                                && x.Status != MovedStatus.Initiated
                                                && x.Status != MovedStatus.Waiting
                                                && x.CreatedBy == userId)
                                .FirstOrDefaultAsync();

                        if (locationInfo != null)
                        {
                            var productInfoList = await unitOfWork.ProductsRepository
                                .GetQueryable(x => !x.IsDeleted && x.LocationId == locationInfo.Id
                                                && x.Status != MovedStatus.Transferred
                                                && x.Status != MovedStatus.Initiated
                                                && x.Status != MovedStatus.Waiting
                                                && x.CreatedBy == userId)
                                .ToListAsync();

                            var productIdList = string.Empty;
                            if (productInfoList.Any())
                            {
                                productIdList = string.Join(",", productInfoList.Select(p => p.Id.ToString()));
                            }

                            await unitOfWork.ItemTransferRepository
                                .AddAsync(new MovedItemStatusTransactonsEntity()
                                {
                                    TransferId = transferIdString,
                                    ItemId = item,
                                    FromUserId = userId,
                                    ToUserId = null,
                                    Status = MovedStatus.Initiated,
                                    IsProduct = model.IsProduct,
                                    ExpireOn = DateTime.UtcNow.AddMonths(1),
                                    DependentProductIds = productIdList,
                                    IsDeleted = false,
                                    CreatedBy = userId,
                                    CreatedOn = DateTime.UtcNow,
                                    UpdatedBy = userId,
                                    UpdatedOn = DateTime.UtcNow
                                });
                            await unitOfWork.CommitAsync();

                            locationInfo.IsMoving = true;
                            locationInfo.Status = MovedStatus.Initiated;
                            await unitOfWork.CommitAsync();

                            if (productInfoList.Any())
                            {
                                foreach (var productInfo in productInfoList)
                                {
                                    productInfo.IsMoving = true;
                                    productInfo.Status = MovedStatus.Initiated;
                                    await unitOfWork.CommitAsync();
                                }
                            }
                        }
                    }
                }

                #endregion

                var fromUserInfo = userManager.Users.FirstOrDefault(x => !x.IsDeleted && x.Id == userId);
                var phonenumber = model.CountryCode + model.PhoneNumber;
                var result = !string.IsNullOrEmpty(model.Email)
                            ? await emailManager.SetItemTransferInvitationEmail(model.Email, model.Email.Split('@')[0], string.Join(", ", model.TransferItemName), userId, model.IsProduct)
                            : await smsManager.SetItemTransferInvitation(phonenumber, string.IsNullOrEmpty(fromUserInfo.FirstName) ? fromUserInfo.UserName : fromUserInfo.FirstName, userId);
                return result;
            }

            return new ManagerBaseResponse<bool>()
            {
                Message = "",
                Result = false,
                StatusCode = 500
            };
        }

        public async Task InviteUserStatusUpdate(ApplicationUser user)
        {
            var itemTransferInvitationlst = await unitOfWork.ItemTransferInvitationRepository
                .GetListAsync(x => x.Email == user.Email || x.PhoneNumber == user.PhoneNumber);
            if(itemTransferInvitationlst != null)
            {
                foreach(var itemTransferInvitation in itemTransferInvitationlst)
                {
                    var itemTransferlst = await unitOfWork.ItemTransferRepository
                        .GetListAsync(x => x.TransferId == itemTransferInvitation.TransferId);
                    if (itemTransferlst != null)
                    {
                        foreach (var itemTransfer in itemTransferlst)
                        {
                            itemTransfer.ToUserId = user.Id;
                            itemTransfer.Status = MovedStatus.Waiting;
                            itemTransfer.UpdatedBy = user.Id;
                            itemTransfer.UpdatedOn = DateTime.UtcNow;
                            await unitOfWork.CommitAsync();

                            if (itemTransfer.IsProduct)
                            {
                                var productInfo = await unitOfWork.ProductsRepository
                                    .GetQueryable(x => !x.IsDeleted && x.Id == itemTransfer.ItemId)
                                    .FirstOrDefaultAsync();
                                if (productInfo != null)
                                {
                                    productInfo.TransferTo = user.Id;
                                    productInfo.IsMoving = true;
                                    productInfo.Status = MovedStatus.Waiting;
                                    await unitOfWork.CommitAsync();
                                }
                            }
                            else
                            {
                                var locationInfo = await unitOfWork.LocationsRepository
                                    .GetQueryable(x => !x.IsDeleted && x.Id == itemTransfer.ItemId)
                                    .FirstOrDefaultAsync();

                                if (locationInfo != null)
                                {
                                    var products = await unitOfWork.ProductsRepository
                                           .GetListAsync(x => !x.IsDeleted && x.LocationId == locationInfo.Id && x.Status != MovedStatus.Transferred);
                                    foreach (var product in products)
                                    {
                                        product.TransferTo = user.Id;
                                        product.IsMoving = true;
                                        product.Status = MovedStatus.Waiting;
                                        await unitOfWork.CommitAsync();
                                    }

                                    locationInfo.TransferTo = user.Id;
                                    locationInfo.IsMoving = true;
                                    locationInfo.Status = MovedStatus.Waiting;
                                    await unitOfWork.CommitAsync();
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}
