using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DatingApp.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DatingApp.Controllers
{
    // [Route("api/[controller]")]
    // [ApiController]
    // [ApiController]
    // [Route("api/[controller]")]
    public class AdminController : BaseApiController
    {
        private readonly UserManager<AppUser> _userManager;

      
        public AdminController(UserManager<AppUser> userManager)
        {
          _userManager = userManager;
        }

        [Authorize(Policy = "RequireAdminRole")]
        [HttpGet("users-with-roles")]
        public async Task<IActionResult> GetUsersWithRoles()
        {
            var users = await _userManager.Users.Include(r => r.UserRoles).ThenInclude(r=>r.Role)
            .OrderBy(u=>u.UserName)
            .Select(u=> new
            {
                u.Id,
                Username= u.UserName,
                Roles= u.UserRoles.Select(r=>r.Role.Name).ToList()
            }).ToListAsync();

            return Ok(users);
        }
        
        [HttpPost("edit-roles/{userName}")]
        public async Task<IActionResult> EditRoles(String userName,[FromQuery] string roles){

            var selectedRoles = roles.Split(",").ToArray();
            var user = await _userManager.FindByNameAsync(userName);

            if (user == null)
                return NotFound("User not found");
                
            var userRoles = await _userManager.GetRolesAsync(user);
            var result = await _userManager.AddToRolesAsync(user,selectedRoles.Except(userRoles));
            if (!result.Succeeded)
            {
                return BadRequest("Failed to add to roles");
            }

            result = await _userManager.RemoveFromRolesAsync(user,userRoles.Except(selectedRoles));

            if (!result.Succeeded)
            {
                return BadRequest("Failed to remove the roles");
            }

            return Ok(await _userManager.GetRolesAsync(user));
        }
        
        [Authorize(Policy = "ModeratePhotoRole")]
        [HttpGet("photos-to-moderate")]
        public IActionResult GetPhotosForModeration()
        {
            return Ok("Only admins or moderators can see this");
        }
    }
}