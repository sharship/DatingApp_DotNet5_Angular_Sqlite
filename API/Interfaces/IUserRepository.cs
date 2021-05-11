using System.Collections.Generic;
using System.Threading.Tasks;
using API.DTOs;
using API.Entities;
using API.Helpers;

namespace API.Interfaces
{
    public interface IUserRepository
    {
        void UpdateUser (AppUser user);

        Task<AppUser> GetUserByIdAsync(int id);
        Task<AppUser> GetUserByUsernameAsync(string username);
        Task<IEnumerable<AppUser>> GetUsersAsync();

        Task<AppUser> GetUserByPhotoId(int photoId);

        Task<MemberDto> GetMemberByUsernameAsync(string username, bool isCurrentUser);
        Task<PagedList<MemberDto>> GetMembersAsync(UserParams userParams);

    }
}