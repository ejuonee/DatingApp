using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CloudinaryDotNet.Actions;
using DatingApp.Entities;
using DatingApp.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace DatingApp.Controllers
{
  // [Route("api/[controller]")]
  // [ApiController]
  // [ApiController]
  // [Route("api/[controller]")]
  public class AdminController : BaseApiController
  {
    private readonly UserManager<AppUser> _userManager;
    private readonly IUnitOfWork _unitOfWork;

    private readonly ILogger<AdminController> _logger;

    private readonly IPhotoService _photoService;

    // private readonly IPhotoRepository _photoRepository;

    public AdminController(UserManager<AppUser> userManager, IUnitOfWork unitOfWork, /* IPhotoRepository photoRepository,  */ILogger<AdminController> logger, IPhotoService photoService)
    {
      _userManager = userManager;
      _unitOfWork = unitOfWork;
      _logger = logger;
      _photoService = photoService;
      // _photoRepository = photoRepository;
    }

    [Authorize(Policy = "RequireAdminRole")]
    [HttpGet("users-with-roles")]
    public async Task<ActionResult> GetUsersWithRoles()
    {
      var users = await _userManager.Users.Include(r => r.UserRoles).ThenInclude(r => r.Role)
      .OrderBy(u => u.UserName)
      .Select(u => new
      {
        u.Id,
        Username = u.UserName,
        Roles = u.UserRoles.Select(r => r.Role.Name).ToList()
      }).ToListAsync();

      return Ok(users);
    }




    [HttpPost("edit-roles/{userName}")]
    public async Task<ActionResult> EditRoles(String userName, [FromQuery] string roles)
    {

      var selectedRoles = roles.Split(",").ToArray();
      var user = await _userManager.FindByNameAsync(userName);

      if (user == null)
        return NotFound("User not found");

      var userRoles = await _userManager.GetRolesAsync(user);
      var result = await _userManager.AddToRolesAsync(user, selectedRoles.Except(userRoles));
      if (!result.Succeeded)
      {
        return BadRequest("Failed to add to roles");
      }

      result = await _userManager.RemoveFromRolesAsync(user, userRoles.Except(selectedRoles));

      if (!result.Succeeded)
      {
        return BadRequest("Failed to remove the roles");
      }

      return Ok(await _userManager.GetRolesAsync(user));
    }

    // [Authorize(Policy = "ModeratePhotoRole")]
    // [HttpGet("photos-to-moderate")]
    // public IActionResult GetPhotosForModeration()
    // {
    //     return Ok("Only admins or moderators can see this");
    // }

    [Authorize(Policy = "ModeratePhotoRole")]
    [HttpGet("photos-for-moderation")]
    public async Task<ActionResult> GetPhotosForModeration()
    {

      var photos = await _unitOfWork.PhotoRepository.GetUnapprovedPhotos();
      // var photos = await _photoRepository.GetUnapprovedPhotos();
      return Ok(photos);
    }

    // [Authorize(Policy = "ModeratePhotoRole")]
    // [HttpPut("approve-photo/{id}")]
    // public async Task<IActionResult> ApprovePhoto(int id)
    // {
    //     var photo = await _unitOfWork.PhotoRepository.GetPhotoById(id);
    //     if (photo == null)
    //         return NotFound("Photo not found");

    //     photo.IsApproved = true;
    //     await _unitOfWork.Complete();
    //     return NoContent();
    // }

    [Authorize(Policy = "ModeratePhotoRole")]
    [HttpDelete("reject-photo/{id}")]
    public async Task<ActionResult> RejectPhoto(int id)
    {
      var photo = await _unitOfWork.PhotoRepository.GetPhotoById(id);
      // var photo = await _photoRepository.GetPhotoById(id);


      if (photo.PublicId != null)
      {

        var result = await _photoService.DeletePhotoAsync(photo.PublicId);

        if (result.Result == "ok")
        {
          await _unitOfWork.PhotoRepository.RemovePhoto(photo.Id);
          // await _photoRepository.RemovePhoto(photo.Id);
          await _unitOfWork.Complete();

        }
        else
        {
          // await _photoRepository.RemovePhoto(photo.Id);
          await _unitOfWork.PhotoRepository.RemovePhoto(photo.Id);
          await _unitOfWork.Complete();
        }

        return Ok();

      }

      if (photo == null)
        return NotFound("Photo not found");

      if (photo.IsMain)
        return BadRequest("You cannot reject the main photo");


      if (photo.PublicId == null)
      {
        await _unitOfWork.PhotoRepository.RemovePhoto(photo.Id);
        // await _photoRepository.RemovePhoto(photo.Id);
      }

      await _unitOfWork.Complete();
      return Ok();
    }
    [Authorize(Policy = "ModeratePhotoRole")]
    [HttpPost("approve-photo/{photoId}")]
    public async Task<ActionResult> ApprovePhoto(int photoId)
    {
      // var photo = await _photoRepository.GetPhotoById(photoId);
      var photo = await _unitOfWork.PhotoRepository.GetPhotoById(photoId);
      if (photo == null) return NotFound("Could not find photo");
      photo.IsApproved = true;
      var user = await _unitOfWork.UserRepository.GetUserByPhotoId(photoId);
      if (!user.Photos.Any(x => x.IsMain)) photo.IsMain = true;
      await _unitOfWork.Complete();

      return Ok();
    }


  }
}