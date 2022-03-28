using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using DatingApp.Data_Transfer_Object;
using DatingApp.Interfaces;
using Microsoft.Extensions.Logging;

namespace DatingApp.DataTransferObject
{
  public class UnitOfWork : IUnitOfWork
  {
    // private readonly DataContext _context;

    // private readonly IMapper _mapper;

    // private readonly ILogger<PhotoRepository> _logger;
    // public UnitOfWork(DataContext context, IMapper mapper, ILogger<PhotoRepository> logger)
    // {
    //   _context = context;
    //   _mapper = mapper;
    //   _logger = logger;
    // }

    // public IUserRepository UserRepository => new UserRepository(_context, _mapper);

    // public IMessageRepository MessageRepository => new MessageRepository(_context, _mapper);

    // public ILikesRepository LikesRepository => new LikesRepository(_context);

    // public IPhotoRepository PhotoRepository => new PhotoRepository(_context, _logger);


    // public async Task<bool> Complete()
    // {

    //   var bin = await _context.SaveChangesAsync();
    //   return bin > 0;
    // }

    // public bool HasChanges()
    // {
    //   _context.ChangeTracker.DetectChanges();
    //   var changes = _context.ChangeTracker.HasChanges();

    //   return changes;
    // }

    // // public async Task<bool> Complete()
    // // {
    // //   return await _context.SaveChangesAsync() > 0;
    // // }

    // // public bool HasChanges()
    // // {
    // //   return _context.ChangeTracker.HasChanges();
    // // }


    private readonly IMapper _mapper;
    private readonly DataContext _context;

    private readonly ILogger<PhotoRepository> _logger;
    public UnitOfWork(DataContext context, IMapper mapper, ILogger<PhotoRepository> logger)
    {
      _context = context;
      _mapper = mapper;
      _logger = logger;
    }

    public IUserRepository UserRepository => new UserRepository(_context, _mapper);

    public IMessageRepository MessageRepository => new MessageRepository(_context, _mapper);

    public ILikesRepository LikesRepository => new LikesRepository(_context);

    public IPhotoRepository PhotoRepository => new PhotoRepository(_context, _logger);

    public async Task<bool> Complete()
    {
      return await _context.SaveChangesAsync() > 0;
    }

    public bool HasChanges()
    {
      _context.ChangeTracker.DetectChanges();
      var changes = _context.ChangeTracker.HasChanges();

      return changes;
    }
  }
}