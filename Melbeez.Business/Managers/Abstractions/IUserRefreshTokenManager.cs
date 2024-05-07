using System;
using System.Threading.Tasks;

namespace Melbeez.Business.Managers.Abstractions
{
    public interface IUserRefreshTokenManager
    {
        bool IsValid(string refreshToken, string userId);
        Task Add(string refreshToken, string userId, string ipAddress, DateTime expiryDateTime);
    }
}
