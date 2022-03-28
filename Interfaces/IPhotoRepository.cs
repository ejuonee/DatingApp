using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DatingApp.DTO;
using DatingApp.Entities;

namespace DatingApp.Interfaces
{
  public interface IPhotoRepository
  {
    Task<IEnumerable<PhotoApprovalDto>> GetUnapprovedPhotos();

    Task<Photo> GetPhotoById(int id);

    Task<bool> RemovePhoto(int id);

    Task<bool> Complete();
  }
}