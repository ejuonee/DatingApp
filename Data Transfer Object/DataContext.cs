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
        // public DbSet<Photo> Photos { get; set; }

        public DbSet<UserLike> Likes { get; set; }

        // public DbSet<UserLike> Messages { get; set; }


        protected override void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<UserLike>()
                .HasKey(k => new { k.SourceUserId, k.LikedUserId });

            builder.Entity<UserLike>()
                .HasOne(s => s.SourceUser)
                .WithMany(l => l.LikedUsers)
                .HasForeignKey(u => u.SourceUserId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<UserLike>()
                .HasOne(u => u.LikedUser)
                .WithMany(u => u.LikedByUsers)
                .HasForeignKey(u => u.LikedUserId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    
    }
}