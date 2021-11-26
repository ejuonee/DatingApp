using DatingApp.Entities;
using Microsoft.EntityFrameworkCore;

namespace DatingApp.Data_Transfer_Object
{
    public class DataContext : DbContext
    {
        public DataContext(DbContextOptions options) : base(options)
        {
        }

        public DbSet<AppUser> Users { get; set; }
        public DbSet<Photo> Photos { get; set; }
    }
}