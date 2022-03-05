using System.Threading.Tasks;
using DatingApp.Entities;

namespace DatingApp.Interfaces
{
    public interface ITokenService
    {
        public Task<string> CreateToken(AppUser user);
    }
}