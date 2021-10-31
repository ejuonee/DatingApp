using DatingApp.Entities;

namespace DatingApp.Interfaces
{
    public interface ITokenService
    {
        public string CreateToken(AppUser user);
    }
}