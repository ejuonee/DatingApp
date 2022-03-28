using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DatingApp.Data_Transfer_Object;
using DatingApp.DTO;
using DatingApp.Entities;
using DatingApp.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace DatingApp.DataTransferObject
{
  public class PhotoRepository : IPhotoRepository
  {

    private readonly DataContext _context;
    private readonly ILogger<PhotoRepository> _logger;
    public PhotoRepository()
    {
    }

    public PhotoRepository(DataContext context, ILogger<PhotoRepository> logger)
    {
      _context = context;
      _logger = logger;
    }


    public async Task<Photo> GetPhotoById(int id)
    {
      _logger.LogInformation("GetPhotoById");
      return await _context.Photos.IgnoreQueryFilters().SingleOrDefaultAsync(x => x.Id == id);
    }

    public async Task<IEnumerable<PhotoApprovalDto>> GetUnapprovedPhotos()
    {
      _logger.LogInformation("GetUnapprovedPhotos");
      return await _context.Photos.IgnoreQueryFilters()
              .Where(p => p.IsApproved == false)
              .Select(u => new PhotoApprovalDto
              {
                Id = u.Id,
                Url = u.Url,
                IsApproved = u.IsApproved,
                Username = u.AppUser.UserName
              }).ToListAsync();

    }

    public async Task<bool> RemovePhoto(int id)
    {
      _logger.LogInformation("RemovePhoto");
      var photo = await _context.Photos.FindAsync(id);
      if (photo == null)
      {
        _logger.LogInformation("Photo not found");
        return false;
      }
      _context.Photos.Remove(photo);

      await _context.SaveChangesAsync();
      _logger.LogInformation("Photo removed");
      return true;
    }

    public async Task<bool> Complete()
    {
      return await _context.SaveChangesAsync() > 0;
    }
  }
}