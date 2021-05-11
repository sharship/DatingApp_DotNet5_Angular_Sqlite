using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using API.DTOs;
using API.Entities;
using API.Interfaces;
using API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers
{
    public class AdminController : BaseApiController
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IPhotoService _photoService;

        public AdminController(UserManager<AppUser> userManager, IUnitOfWork unitOfWork, IPhotoService photoService)
        {
            _photoService = photoService;
            _unitOfWork = unitOfWork;
            _userManager = userManager;
        }

        [Authorize(Policy = "RequireAdminRole")]
        [HttpGet("users-with-roles")]
        public async Task<ActionResult<IList<object>>> GetUsersWithRoles()
        {
            var users = await _userManager.Users
                .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
                .OrderBy(u => u.UserName)
                .Select(u => new
                {
                    u.Id,
                    Username = u.UserName,
                    Roles = u.UserRoles
                        .Select(ur => ur.Role.Name)
                        .ToList()
                })
                .ToListAsync();

            return Ok(users);
        }


        [Authorize(Policy = "RequireAdminRole")]
        [HttpPost("edit-roles/{username}")]
        public async Task<ActionResult<IList<string>>> EditRoles(string username, [FromQuery] string roles)
        {
            var selectedRoles = roles.Split(",").ToArray();
            if (selectedRoles.Length == 0)
            {
                return BadRequest("Roles are not specified");
            }

            var user = await _userManager.FindByNameAsync(username);
            if (user == null)
            {
                return NotFound($"{username} not found.");
            }

            var existingRoles = await _userManager.GetRolesAsync(user);

            // add new roles in
            var result = await _userManager.AddToRolesAsync(user, selectedRoles.Except(existingRoles));
            if (!result.Succeeded)
            {
                return BadRequest("Failed to add new roles in.");
            }

            // remove roles
            result = await _userManager.RemoveFromRolesAsync(user, existingRoles.Except(selectedRoles));
            if (!result.Succeeded)
            {
                return BadRequest("Failed to remove roles.");
            }

            return Ok(await _userManager.GetRolesAsync(user));

        }


        [Authorize(Policy = "ModeratePhotoRole")]
        [HttpGet("photos-to-moderate")]
        public async Task<ActionResult> GetPhotosForModeration()
        {
            var photos = await _unitOfWork.PhotoRepository.GetUnapprovedPhotos();
            return Ok(photos);
        }

        [Authorize(Policy = "ModeratePhotoRole")]
        [HttpPost("approve-photo/{photoId}")]
        public async Task<ActionResult> ApprovePhoto(int photoId)
        {
            var photo = await _unitOfWork.PhotoRepository.GetPhotoById(photoId);

            if (photo == null)
            {
                return NotFound();
            }

            photo.IsApproved = true;

            var user = await _unitOfWork.UserRepository.GetUserByPhotoId(photo.Id);

            if (!user.Photos.Any(p => p.IsMain))
            {
                photo.IsMain = true;
            }

            if (await _unitOfWork.Complete())
            {
                return Ok();
            }

            return BadRequest("Fail to approve photo");
        }

        [Authorize(Policy = "ModeratePhotoRole")]
        [HttpPost("reject-photo/{photoId}")]
        public async Task<ActionResult> RejectPhoto(int photoId)
        {
            var photo = await _unitOfWork.PhotoRepository.GetPhotoById(photoId);

            if (photo == null)
            {
                return NotFound();
            }

            if (photo.PublicId != null)
            {
                var result = await _photoService.DeletePhotoAsync(photo.PublicId);

                if (result.Error != null)
                {
                    return BadRequest(result.Error.Message);
                }
                
                _unitOfWork.PhotoRepository.RemovePhoto(photo);
                
            }
            else
            {
                _unitOfWork.PhotoRepository.RemovePhoto(photo);
            }


            if (await _unitOfWork.Complete())
            {
                return Ok();
            }

            return BadRequest("Fail to reject photo");
        }
    }
}