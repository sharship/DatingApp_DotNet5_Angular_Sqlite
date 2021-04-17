using System.Collections.Generic;
using System.Threading.Tasks;
using API.DTOs;
using API.Entities;
using API.Extensions;
using API.Helpers;
using API.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [Authorize]
    public class LikesController : BaseApiController
    {
        private readonly ILikesRepository _likesReposity;
        private readonly IUserRepository _userRepository;
        public LikesController(IUserRepository userRepository, ILikesRepository likesReposity)
        {
            _userRepository = userRepository;
            _likesReposity = likesReposity;
        }

        [HttpPost("{followingName}")]
        public async Task<ActionResult> Follow(string followingName)
        {

            var sourceUserId = User.GetUserId();
            var sourceUser = await _likesReposity.GetUserWithLikes(sourceUserId);

            var followingUser = await _userRepository.GetUserByUsernameAsync(followingName);


            if (followingUser == null)
            {
                return NotFound();
            }

            if (sourceUser.UserName == followingName)
            {
                return BadRequest("You cannot follow yourself.");
            }


            var existingUserLike = _likesReposity.GetUserLike(sourceUserId, followingUser.Id);
            if (existingUserLike != null)
            {
                return BadRequest("You already followed this user.");
            }

            // now create new like relation item, and then add to login user's followings
            var userLike = new UserLike
            {
                SourceUserId = sourceUserId,
                TargetUserId = followingUser.Id
            };

            sourceUser.Followings.Add(userLike);

            if (await _userRepository.SaveAllAsync())
            {
                return Ok();
            }

            return BadRequest("Failed to follow this user.");

        }

        // get login user's followings or followers based on predicate
        [HttpGet]
        public async Task<ActionResult<IEnumerable<LikeDto>>> GetUserLikes([FromQuery]LikesParams likesParams)
        {
            likesParams.UserId = User.GetUserId();
            var users = await _likesReposity.GetUserLikes(likesParams);

            Response.AddPaginationHeader(users.CurrentPage, users.PageSize, users.TotalCount, users.TotalPages);

            return Ok(users);
        }

    }
}