using DatingApp.Data_Transfer_Object;
using DatingApp.DTO;
using DatingApp.Entities;
using DatingApp.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace DatingApp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController : ControllerBase
    {

        private readonly DataContext _context;
        private readonly ITokenService _tokenService;

        public AccountController(DataContext context, ITokenService tokenService) 
        {
             _context = context;
            _tokenService = tokenService;
        }

        [HttpPost("register")]
        public async Task<ActionResult<UserDTO>> Register(RegisterDTO data)
        {
            if (await UserExists(data.Username)) return BadRequest("User already exist");
            using var hmac = new HMACSHA512();

            var user = new AppUser
            {
                UserName = data.Username.ToLower(),
                PasswordHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(data.Password)),
                PasswordSalt = hmac.Key
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();
            return new UserDTO
            {Username=user.UserName,
            Token=_tokenService.CreateToken(user)
            };

        }

        [HttpPost("login")]
        public async Task <ActionResult<UserDTO>> Login (LoginDTO data)
        {
            var usernamesmall = data.Username.ToLower();
            //var user = await _context.Users.SingleOrDefaultAsync(username => username.UserName == data.Username);

            var user = await _context.Users.SingleOrDefaultAsync(username => username.UserName == usernamesmall);
            if (user == null) return Unauthorized("Invalid Username");
            using var hmac = new HMACSHA512(user.PasswordSalt);
            var computedhash =hmac.ComputeHash(Encoding.UTF8.GetBytes(data.Password));

            for (int i = 0; i < computedhash.Length; i++)
            {
                if (computedhash[i] != user.PasswordHash[i]) { return Unauthorized("Invalid Password"); }
            }

            return new UserDTO
            {
                Username = user.UserName,
                Token = _tokenService.CreateToken(user)
            };
        }
        private async Task<bool> UserExists(string username)
        {
            return await _context.Users.AnyAsync(user => user.UserName == username.ToLower());
        }
    }
}
