using DatingApp.DTO;
using System.Collections.Generic;
using System.Threading.Tasks;
using DatingApp.Entities;
using DatingApp.Helpers;

namespace DatingApp.Interfaces
{
    public interface ILikesRepository
    {
        Task<UserLike> GetUserLike(int sourceUserId, int likedUserId);
        Task<AppUser> GetUserWithLikes(int userId);
        Task <PagedList<LikeDto>> GetUserLikes (LikesParams likesParams);
    }
}