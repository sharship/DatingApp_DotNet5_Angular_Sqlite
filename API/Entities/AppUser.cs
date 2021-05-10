using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Identity;

namespace API.Entities
{
    public class AppUser: IdentityUser<int>
    {

        public DateTime DateOfBirth { get; set; }
        public string KnownAs { get; set; }
        public DateTime Created { get; set; } = DateTime.UtcNow;
        public DateTime LastActive { get; set; } = DateTime.UtcNow;
        public string Gender { get; set; }
        public string Introduction { get; set; }
        public string LookingFor { get; set; }
        public string Interests { get; set; }
        public string City { get; set; }
        public string Country { get; set; }

        public ICollection<Photo> Photos { get; set; }
        
        #region Like functionality
        public ICollection<UserLike> Followers { get; set; }
        public ICollection<UserLike> Followings { get; set; }            
        #endregion

        #region Message functionality
        public ICollection<Message> MessagesSent { get; set; }
        
        public ICollection<Message> MessagesReceived { get; set; }
        
        #endregion

        #region Identity and Role management

        // One User could have many Roles
        public ICollection<AppUserRole> UserRoles { get; set; }
        
        

        #endregion
        
    }
}