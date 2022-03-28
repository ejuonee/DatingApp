using AutoMapper;
using DatingApp.DTO;
using DatingApp.Entities;
using DatingApp.Extensions;
using DatingApp.Helpers;
using DatingApp.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace DatingApp.Controllers
{
    [Authorize]
    public class UsersController : BaseApiController
    {
        // private readonly IUserRepository _userRepository;

        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        private readonly IPhotoService _photoService;

        public UsersController(IUnitOfWork unitOfWork,/* IUserRepository userRepository, */ IMapper mapper, IPhotoService photoService,IUnitOfWork unitOf)
           
        {
            // _userRepository = userRepository;
            _mapper = mapper;
            _photoService = photoService;
            _unitOfWork = unitOfWork;
        }

        // [Authorize (Roles = "Admin")]
        // [AllowAnonymous]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<MemberDto>>> GetUsers([FromQuery]UserParams userParams)
        {
            // var user = await _unitOfWork.UserRepository.GetUserByUsernameAsync(User.GetUserName());

            // userParams.CurrentUsername= user.UserName;
            // if (string.IsNullOrEmpty(userParams.Gender))
            // {userParams.Gender = user.Gender =="male" ? "female" : "male";}
            // var users = await _unitOfWork.UserRepository.GetMembersAsync(userParams);
           
            // Response.AddPaginationHeader(users.CurrentPage, users.PageSize, users.TotalCount, users.TotalPages);

            // return Ok(users);

            var gender = await _unitOfWork.UserRepository.GetUserGender(User.GetUserName());

            userParams.CurrentUsername= User.GetUserName();
            if (string.IsNullOrEmpty(userParams.Gender))
            {userParams.Gender = gender =="male" ? "female" : "male";}
            var users = await _unitOfWork.UserRepository.GetMembersAsync(userParams);
           
            Response.AddPaginationHeader(users.CurrentPage, users.PageSize, users.TotalCount, users.TotalPages);

            return Ok(users);
        }
        
        //api/user/id
        [HttpGet("{username}",Name ="GetUser")]
        // [Authorize (Roles = "Member")]
        public async Task<ActionResult<MemberDto>> GetUser(string username)
        {
            var currentUsername = User.GetUserName();
            var user = await _unitOfWork.UserRepository.GetMemberAsync(username,isCurrentUser:currentUsername==username);

            return Ok(_mapper.Map<MemberDto>(user));
        }

        [HttpPut]

        public async Task<ActionResult> updateUser(MemberUpdateDto memberUpdateDto){
           

            var user = await _unitOfWork.UserRepository.GetUserByUsernameAsync(User.GetUserName());

            _mapper.Map(memberUpdateDto, user);
            _unitOfWork.UserRepository.Update(user);

            if (await _unitOfWork.Complete()) return NoContent();

            else return BadRequest("Failed to Update User");
        }


        [HttpPost("add-photo")]
        public async Task<ActionResult<PhotoDto>> AddPhoto(IFormFile file)
        {
              var user = await _unitOfWork.UserRepository.GetUserByUsernameAsync(User.GetUserName());

            var result = await _photoService.AddPhotoAsync(file);

            if (result.Error != null) return BadRequest( result.Error.Message);

            var photo = new Photo
            {
                Url = result.SecureUrl.AbsoluteUri,
               
                PublicId = result.PublicId
            };

            // if (user.Photos.Count == 0)
            // {
            //     photo.IsMain = false;
            // }

            user.Photos.Add(photo);

            if(await _unitOfWork.Complete())
            {
               // return _mapper.Map<PhotoDto>(photo);
              // return CreatedAtRoute("GetUser", _mapper.Map<PhotoDto>(photo));
              return CreatedAtRoute("GetUser", new { username = user.UserName }, _mapper.Map<PhotoDto>(photo));
            }

            return BadRequest("Failed to add photo");
        }
        [HttpPut("set-main-photo/{photoId}")]
        public async Task<ActionResult> SetMainPhoto(int photoId)
        {
            var user = await _unitOfWork.UserRepository.GetUserByUsernameAsync(User.GetUserName());
           var photo= user.Photos.FirstOrDefault(p => p.Id == photoId);

            if (photo == null) return BadRequest("Photo not found");

            if (photo.IsMain) return BadRequest("This is already the main photo");

            var currentMainPhoto = user.Photos.FirstOrDefault(p => p.IsMain);

            currentMainPhoto.IsMain = false;
            photo.IsMain = true;

            if (await _unitOfWork.Complete()) return NoContent();

            return BadRequest("Failed to set photo as main");
            
        }
        [HttpDelete("delete-photo/{photoId}")]
        public async Task<ActionResult> DeletePhoto(int photoId)
        {
            var user = await _unitOfWork.UserRepository.GetUserByUsernameAsync(User.GetUserName());
            var photo = user.Photos.FirstOrDefault(p => p.Id == photoId);

            if (photo == null) return NotFound();

            if (photo.IsMain) return BadRequest("You cannot delete your main photo");

            if (photo.PublicId != null)
            {
                var deleteResult = await _photoService.DeletePhotoAsync(photo.PublicId);

                if (deleteResult.Error != null) return BadRequest(deleteResult.Error.Message);
            }

            user.Photos.Remove(photo);

            if (await _unitOfWork.Complete()) return Ok();

            return BadRequest("Failed to delete photo");
        }

    }
}
