using System.Collections.Generic;
using System.Threading.Tasks;
using API.DTOs;
using API.Entities;
using API.Helpers;

namespace API.Interfaces
{
    public interface ILikesRepository
    {
        // use SourceUserId and TargetUserId together as primary key to query Likes table
        Task<UserLike> GetUserLike(int sourceUserId, int targetUserId);

        Task<AppUser> GetUserWithLikes(int userId);

        // based on predicate expression, return users of followings or followers
        Task<PagedList<LikeDto>> GetUserLikes(LikesParams likesParams);

    }
}