using System;
using System.Threading.Tasks;
using API.Entities;

namespace API.Interfaces
{
    public interface ITokenService
    {
        Task<String> CreateTokenAsync(AppUser user);
    }
}