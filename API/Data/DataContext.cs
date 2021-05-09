using API.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace API.Data
{
    public class DataContext : 
        IdentityDbContext<
            AppUser, 
            AppRole, 
            int,
            IdentityUserClaim<int>,
            AppUserRole,
            IdentityUserLogin<int>,
            IdentityRoleClaim<int>,
            IdentityUserToken<int>
        >
    {
        public DataContext(DbContextOptions options) : base(options)
        {
        }

        public DbSet<UserLike> Likes { get; set; }

        public DbSet<Message> Messages { get; set; }
        
        public DbSet<Group> Groups { get; set; }
        
        public DbSet<Connection> Connections { get; set; }
        
        

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);


            #region Identity and Role Management
            // config AppUser: one User could have many Roles, via joint table of UserRoles (capable Roles)

            // one-to-many relationship:
            // One AppUser row to Many AppUserRole rows, and each AppUserRole has One User (AppUser), and thit User's Id (UserId) is used as foreign key to connect to Table AppUser
            builder.Entity<AppUser>()
                .HasMany(u => u.UserRoles) // capable roles, connect to AppRole table via joint table AppUserRole 
                .WithOne(ur => ur.User)
                .HasForeignKey(ur => ur.UserId)
                .IsRequired();

            // config AppRole: one Role could have many Users, via joint table of UserRoles (included Users)
            builder.Entity<AppRole>()
                .HasMany(u => u.UserRoles) // included users, connect to AppUser table via joint table AppUserRole 
                .WithOne(ur => ur.Role)
                .HasForeignKey(ur => ur.RoleId)
                .IsRequired();

            #endregion


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