using Melbeez.Business.Managers.Abstractions;
using Melbeez.Business.Models.Common;
using Melbeez.Business.Models.UserModels.RequestModels;
using Melbeez.Common.Extensions;
using Melbeez.Common.Models;
using Melbeez.Common.Models.Entities;
using Melbeez.Data.UnitOfWork;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Melbeez.Business.Managers
{
    public class PushNotificationManager : IPushNotificationManager
    {
        private readonly IUnitOfWork unitOfWork;
        private readonly Dictionary<string, string> orderByTranslations = new Dictionary<string, string>();
        public PushNotificationManager(IUnitOfWork unitOfWork)
        {
            this.unitOfWork = unitOfWork;
        }
        public async Task<ManagerBaseResponse<IEnumerable<PushNotificationModel>>> Get(PagedListCriteria pagedListCriteria, string userId)
        {
            var result = await unitOfWork
                        .PushNotificationRepositry
                        .GetQueryable(x => !x.IsDeleted && x.IsSuccess && x.RecipientId == userId)
                        .Select(x => new PushNotificationModel()
                        {
                            NotificationId = x.Id,
                            RecipientId = x.RecipientId,
                            Type = x.Type,
                            Title = x.Title,
                            Description = x.Description,
                            IsSuccess = x.IsSuccess,
                            IsRead = x.IsRead,
                            ReferenceId = x.ReferenceId,
                            ExpiryDate = x.ExpiryDate,
                            ErrorMeassge = x.ErrorMeassge,
                            Status = x.Status,
                            CreatedBy = x.CreatedBy,
                            CreatedOn = x.CreatedOn
                        })
                        .WhereIf(!string.IsNullOrWhiteSpace(pagedListCriteria.SearchText), x => x.Title.Contains(pagedListCriteria.SearchText))
                        .WhereIf(!string.IsNullOrWhiteSpace(pagedListCriteria.SearchText), x=> x.Description.Contains(pagedListCriteria.SearchText))
                        .AsNoTracking()
                        .ToPagedListAsync(pagedListCriteria, orderByTranslations);

            var unreadNotificationCount = unitOfWork
                                         .PushNotificationRepositry
                                         .GetQueryable(x => !x.IsDeleted && !x.IsRead && x.IsSuccess && x.RecipientId == userId)
                                         .Count();

            return new ManagerBaseResponse<IEnumerable<PushNotificationModel>>()
            {
                Result = result.Data,
                NotificationPageDetail = new NotificationPageDetailModel()
                {
                    Skip = pagedListCriteria.Skip,
                    Take = pagedListCriteria.Take,
                    Count = result.TotalCount,
                    UnreadNotificationCount = unreadNotificationCount,
                    SearchText = pagedListCriteria.SearchText
                }
            };
        }
        public async Task Add(PushNotificationModel model, string senderUserId)
        {
            if (model.NotificationId == 0)
            {
                await unitOfWork.PushNotificationRepositry.AddAsync(new Domain.Entities.PushNotificationEntity()
                {
                    RecipientId = model.RecipientId,
                    Type = model.Type,
                    Title = model.Title,
                    Description = model.Description,
                    IsSuccess = model.IsSuccess,
                    IsRead = model.IsRead,
                    ReferenceId = model.ReferenceId,
                    ExpiryDate = model.ExpiryDate.HasValue ? model.ExpiryDate.Value.ToUniversalTime() : null,
                    ErrorMeassge = model.ErrorMeassge,
                    Status = model.Status,
                    CreatedBy = senderUserId,
                    CreatedOn = DateTime.UtcNow
                });
                await unitOfWork.CommitAsync();
            }
        }
        public async Task<ManagerBaseResponse<bool>> ReadNotification(PushNotificationReadRequestModel model, string userId)
        {
            if (model.IsReadAll)
            {
                var pushNotifications = await unitOfWork
                                      .PushNotificationRepositry
                                      .GetQueryable(pn => !pn.IsDeleted && pn.RecipientId == userId && !pn.IsRead)
                                      .ToListAsync();

                if (pushNotifications.Any())
                {
                    foreach (var pushNotification in pushNotifications)
                    {
                        pushNotification.IsRead = true;
                        pushNotification.UpdatedBy = userId;
                        pushNotification.UpdatedOn = DateTime.UtcNow;
                        await unitOfWork.CommitAsync();
                    }
                }
            }
            else
            {
                var pushNotification = await unitOfWork
                                       .PushNotificationRepositry
                                       .GetAsync(pn => !pn.IsDeleted && !pn.IsRead && pn.Id == model.Id && pn.RecipientId == userId);

                if (pushNotification != null)
                {
                    pushNotification.IsRead = true;
                    pushNotification.UpdatedBy = userId;
                    pushNotification.UpdatedOn = DateTime.UtcNow;
                    await unitOfWork.CommitAsync();
                }
            }
            return new ManagerBaseResponse<bool>()
            {
                Result = true
            };
        }
        public async Task<ManagerBaseResponse<bool>> DeleteNotification(List<long?> ids, string userId)
        {
            if (ids.Count() > 0)
            {
                foreach (var id in ids)
                {
                    var pushNotification = await unitOfWork
                                      .PushNotificationRepositry
                                      .GetAsync(pn => !pn.IsDeleted
                                                   && pn.Id == id
                                                   && pn.RecipientId == userId);

                    if (pushNotification != null)
                    {
                        pushNotification.IsDeleted = true;
                        pushNotification.DeletedBy = userId;
                        pushNotification.DeletedOn = DateTime.UtcNow;
                        await unitOfWork.CommitAsync();
                    }
                }
            }
            else
            {
                var pushNotifications = await unitOfWork
                                      .PushNotificationRepositry
                                      .GetQueryable(pn => !pn.IsDeleted
                                                       && pn.RecipientId == userId)
                                      .ToListAsync();

                if (pushNotifications.Any())
                {
                    foreach (var pushNotification in pushNotifications)
                    {
                        pushNotification.IsDeleted = true;
                        pushNotification.DeletedBy = userId;
                        pushNotification.DeletedOn = DateTime.UtcNow;
                        await unitOfWork.CommitAsync();
                    }
                }
            }
            return new ManagerBaseResponse<bool>()
            {
                Result = true
            };
        }
    }
}
