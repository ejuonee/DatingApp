﻿using AutoMapper;
using AutoMapper.QueryableExtensions;
using DatingApp.DTO;
using DatingApp.Helpers;
using DatingApp.Entities;
using DatingApp.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System;
using System.Diagnostics.CodeAnalysis;

namespace DatingApp.Data_Transfer_Object
{
    public class UserRepository : IUserRepository
    {
        private readonly DataContext _context;
        private readonly IMapper _mapper;

        public UserRepository(DataContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<MemberDto> GetMemberAsync(string username)
        {
            return await _context.Users.Where(x => x.UserName == username).ProjectTo<MemberDto>(_mapper.ConfigurationProvider).SingleOrDefaultAsync();
        }

          public async Task<MemberDto> GetMemberAsync(string username, bool
     isCurrentUser)
             {

                //  if (isCurrentUser == null)
                //  {
                //      return await _context.Users.Where(x => x.UserName == username).ProjectTo<MemberDto>(_mapper.ConfigurationProvider).SingleOrDefaultAsync();
                //  }
                //  else
                //  {
                //       var query = _context.Users
                //      .Where(x => x.UserName == username)
                //      .ProjectTo<MemberDto>(_mapper.ConfigurationProvider)
                //      .AsQueryable();
                //  if ((bool)isCurrentUser) query = query.IgnoreQueryFilters();
                //  return await query.FirstOrDefaultAsync();
                // }
                var query = _context.Users
                     .Where(x => x.UserName == username)
                     .ProjectTo<MemberDto>(_mapper.ConfigurationProvider)
                     .AsQueryable();
                 if (isCurrentUser) query = query.IgnoreQueryFilters();
                 return await query.FirstOrDefaultAsync();


             }

               
          public async Task<AppUser> GetUserByPhotoId(int photoId)
          {
              return await _context.Users
                  .Include(p => p.Photos)
                  .IgnoreQueryFilters()
                  .Where(p => p.Photos.Any(p => p.Id == photoId))
                  .FirstOrDefaultAsync();
}
             

            public async Task<PagedList<MemberDto>> GetMembersAsync( UserParams userParams)
              {
                  var query = _context.Users.AsQueryable();

                    query = query.Where(u => u.UserName != userParams.CurrentUsername);
                    query = query.Where(u => u.Gender == userParams.Gender);
                    var minDob = DateTime.Today.AddYears(-userParams.MaxAge-1);
                    var maxDob = DateTime.Today.AddYears(-userParams.MinAge);

                    query= query.Where(u=> u.DateOfBirth>=minDob && u.DateOfBirth<=maxDob);


                    query= userParams.OrderBy switch
                    {
                        "created" => query.OrderByDescending(u => u.DateCreated),
                        // "lastActive" => query.OrderByDescending(u => u.LastActive),
                        // "age" => query.OrderByDescending(u => u.DateOfBirth),
                        _ => query.OrderByDescending(u => u.LastActive)
                    };
                    
                return await PagedList<MemberDto>.CreateAsync(query.ProjectTo<MemberDto>(_mapper
                        .ConfigurationProvider).AsNoTracking(), 
                            userParams.PageNumber, userParams.PageSize);
                }
        public async Task<AppUser> GetUserByIdAsync(int id)
        {
            return await _context.Users.FindAsync(id);
        }

        public async Task<AppUser> GetUserByUsernameAsync(string username)
        {
            return await _context.Users.Include(photo => photo.Photos.Where(x=>x.IsApproved)).FirstOrDefaultAsync(x => x.UserName == username);
        }

    public async Task<string> GetUserGender(string username)
    {
      return await _context.Users.Where(x => x.UserName == username).Select(x => x.Gender).FirstOrDefaultAsync();
    }

    public async Task<IEnumerable<AppUser>> GetUsersAsync()
        {
            return await _context.Users.Include(photo => photo.Photos).ToListAsync();
        }

        // public async Task<bool> SaveAllAsync()
        // {
        //     return await _context.SaveChangesAsync() > 0;
        // }

        public void Update(AppUser user)
        {
            _context.Entry(user).State = EntityState.Modified;
        }

        

    }
}