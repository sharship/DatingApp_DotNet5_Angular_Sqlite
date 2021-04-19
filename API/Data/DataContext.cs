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

        public DbSet<Message> Messages { get; set; }
        
        

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);


            #region Like-related functionality
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
            #endregion


            #region Message-related functionality
            // AppUser as a Recipient, has many massages received
            builder.Entity<Message>()
                .HasOne(m => m.Recipient)
                .WithMany(u => u.MessagesReceived)
                .OnDelete(DeleteBehavior.Restrict);
            
            // AppUser as a Sender, has many messages sent
            builder.Entity<Message>()
                .HasOne(m => m.Sender)
                .WithMany(s => s.MessagesSent)
                .OnDelete(DeleteBehavior.Restrict);

            #endregion

        }
        
        
    }
}