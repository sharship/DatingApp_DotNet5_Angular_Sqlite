using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using API.DTOs;
using API.Entities;
using API.Helpers;
using API.Interfaces;
using Extensions;
using Microsoft.EntityFrameworkCore;

namespace API.Data
{
    public class LikesRepository : ILikesRepository
    {
        private readonly DataContext _context;
        public LikesRepository(DataContext context)
        {
            _context = context;
        }

        // get specific like relationship
        public async Task<UserLike> GetUserLike(int sourceUserId, int targetUserId)
        {
            return await _context.Likes.FindAsync(sourceUserId, targetUserId);
        }

        // query Followings or Followers of input user
        public async Task<PagedList<LikeDto>> GetUserLikes(LikesParams likesParams)
        {
            // build two IQueryables of Users and Likes for further join and query execution
            var users = _context.Users  // query Users table
                                .OrderBy(u => u.UserName)
                                .AsQueryable();

            var likes = _context.Likes.AsQueryable(); // query Likes table

            if (likesParams.Predicate == "following")
            {
                // get all like relationship items in which current user is the active side
                likes = likes.Where(like => like.SourceUserId == likesParams.UserId);
                // project these like items to the related target users on the passive side
                users = likes.Select(like => like.TargetUser);
            }

            if (likesParams.Predicate == "follower")
            {
                // input user is the passive side
                likes = likes.Where(like => like.TargetUserId == likesParams.UserId);
                users = likes.Select(like => like.SourceUser);
            }

            // transfer AppUser to LikeDto, and then execute IQueryable to List
            var resultUsers = users.Select
                (user => new LikeDto
                {
                    Id = user.Id,
                    Username = user.UserName,
                    Age = user.DateOfBirth.CalculateAge(),
                    KnownAs = user.KnownAs,
                    PhotoUrl = user.Photos.FirstOrDefault(ph => ph.IsMain).Url,
                    City = user.City
                }
                );
            
            return await PagedList<LikeDto>.CreateAsync(resultUsers, likesParams.PageNumber, likesParams.PageSize);


        }

        // get user with his Followings
        public async Task<AppUser> GetUserWithLikes(int userId)
        {
            return await _context.Users  // query Users table
                .Include(u => u.Followings)  // Join Likes table
                .FirstOrDefaultAsync(u => u.Id == userId);  // add WHERE condition at last

        }
    }
}