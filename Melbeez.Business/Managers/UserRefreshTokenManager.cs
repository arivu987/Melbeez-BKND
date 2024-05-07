using Melbeez.Business.Managers.Abstractions;
using Melbeez.Data.UnitOfWork;
using Melbeez.Domain.Entities;
using System;
using System.Threading.Tasks;

namespace Melbeez.Business.Managers
{
    public class UserRefreshTokenManager : IUserRefreshTokenManager
    {
        private readonly IUnitOfWork unitOfWork;

        public UserRefreshTokenManager(IUnitOfWork unitOfWork)
        {
            this.unitOfWork = unitOfWork;
        }
        public bool IsValid(string refreshToken, string userId)
        {
            return unitOfWork.RefreshTokenRepository.IsValid(refreshToken, userId);
        }
        public async Task Add(string refreshToken, string userId, string ipAddress, DateTime expiryDateTime)
        {
            await unitOfWork
                .RefreshTokenRepository
                .AddAsync(new AspNetUserRefreshTokenEntity()
                {
                    Id = 0,
                    UserId = userId,
                    ExpiresOn = expiryDateTime,
                    IPAddress = ipAddress,
                    IsDeleted = false,
                    Token = refreshToken,
                    CreatedBy = userId,
                    CreatedOn = DateTime.Now,
                    UpdatedBy = userId,
                    UpdatedOn = DateTime.UtcNow
                });
            await unitOfWork.CommitAsync();
        }
    }
}
