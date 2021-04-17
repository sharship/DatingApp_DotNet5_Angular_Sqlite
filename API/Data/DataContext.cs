using API.Entities;
using Microsoft.EntityFrameworkCore;

namespace API.Data
{
    public class DataContext : DbContext
    {
        public DataContext(DbContextOptions options) : base(options)
        {
        }
        public DbSet<AppUser> Users { get; set; }

        public DbSet<UserLike> Likes { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<UserLike>()
                .HasKey(ul => new {ul.SourceUserId, ul.TargetUserId});
            
            // one source user could have many followings
            builder.Entity<UserLike>()
                .HasOne(lk => lk.SourceUser) 
                .WithMany(u => u.Followings)
                .HasForeignKey(fln => fln.SourceUserId)
                .OnDelete(DeleteBehavior.Cascade);

            // one target user could have many followers
            builder.Entity<UserLike>()
                .HasOne(lk => lk.TargetUser)
                .WithMany(u => u.Followers)
                .HasForeignKey(flr => flr.TargetUserId)
                .OnDelete(DeleteBehavior.Cascade);

        }
        
        
    }
}