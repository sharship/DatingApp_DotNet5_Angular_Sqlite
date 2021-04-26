using System.Collections.Generic;
using Microsoft.AspNetCore.Identity;

namespace API.Entities
{
    public class AppRole : IdentityRole<int>
    {
        // One Role could have many users
        public ICollection<AppUserRole> UserRoles { get; set; }
        
        
    }
}