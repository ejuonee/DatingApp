using DatingApp.DTO;
using DatingApp.Entities;
using DatingApp.Helpers;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DatingApp.Interfaces
{
    public interface IUserRepository
    {
        void Update(AppUser user);

        // Task<bool> SaveAllAsync();

        Task<IEnumerable<AppUser>> GetUsersAsync();

        Task<AppUser> GetUserByIdAsync(int id);

        Task<AppUser> GetUserByUsernameAsync(string username);

        Task<PagedList<MemberDto>> GetMembersAsync(UserParams userParams);

        Task<string> GetUserGender(string username);
        Task<MemberDto> GetMemberAsync(string username);

        Task<AppUser> GetUserByPhotoId(int photoId);
         Task<MemberDto> GetMemberAsync(string username, bool isCurrentUser);
    }
}