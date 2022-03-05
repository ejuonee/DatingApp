using AutoMapper;
using DatingApp.Data_Transfer_Object;
using DatingApp.DTO;
using DatingApp.Entities;
using DatingApp.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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
        private readonly UserManager<AppUser> _userManager;
        private readonly ITokenService _tokenService;
        private readonly SignInManager<AppUser> _signInManager;
        private readonly IMapper _mapper;

        public AccountController(UserManager<AppUser> userManager, SignInManager<AppUser> signInManager, ITokenService tokenService, IMapper mapper)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _tokenService = tokenService;
            _mapper = mapper;         
        }

        [HttpPost("register")]
        public async Task<ActionResult<UserDTO>> Register(RegisterDTO data)
        {
            if (await UserExists(data.Username)) return BadRequest("User already exist");

            var user =_mapper.Map<AppUser>(data);
            // using var hmac = new HMACSHA512();
            user.UserName = data.Username.ToLower();
                // user.PasswordHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(data.Password));
                // user.PasswordSalt = hmac.Key;
            

            // _context.Users.Add(user);
            // await _context.SaveChangesAsync();

            var result = await _userManager.CreateAsync(user, data.Password);
            if (!result.Succeeded) return BadRequest(result.Errors);

            var checkRole= await _userManager.AddToRoleAsync(user, "Member");
            if (!checkRole.Succeeded) return BadRequest(checkRole.Errors);
            return new UserDTO
            {
                Username = user.UserName,
                Token = await _tokenService.CreateToken(user),
                KnownAs = user.KnownAs,
                Gender = user.Gender

            };
        }

        [HttpPost("login")]
        public async Task<ActionResult<UserDTO>> Login(LoginDTO data)
        {
            var usernamesmall = data.Username.ToLower();
            //var user = await _context.Users.SingleOrDefaultAsync(username => username.UserName == data.Username);

            var user = await _userManager.Users.Include(p=>p.Photos).SingleOrDefaultAsync(username => username.UserName == usernamesmall);
            if (user == null) return Unauthorized("Invalid Username");
            // using var hmac = new HMACSHA512(user.PasswordSalt);
            // var computedhash = hmac.ComputeHash(Encoding.UTF8.GetBytes(data.Password));

            // for (int i = 0; i < computedhash.Length; i++)
            // {
            //     if (computedhash[i] != user.PasswordHash[i]) { return Unauthorized("Invalid Password"); }
            // }
            var result = await _signInManager.CheckPasswordSignInAsync(user, data.Password, false);

            if (!result.Succeeded) return Unauthorized("Invalid Password");
            return new UserDTO
            {
                Username = user.UserName,
                Token =await _tokenService.CreateToken(user),
                PhotoUrl= user.Photos.FirstOrDefault(x=>x.IsMain)?.Url,
                KnownAs = user.KnownAs,
                Gender = user.Gender

            };
        }

        private async Task<bool> UserExists(string username)
        {
            return await _userManager.Users.AnyAsync(user => user.UserName == username.ToLower());
        }
    }
}