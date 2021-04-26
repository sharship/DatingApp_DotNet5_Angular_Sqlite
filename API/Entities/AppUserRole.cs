using Microsoft.AspNetCore.Identity;

namespace API.Entities
{
    // This is a joint table for many-to-many relationship
    public class AppUserRole : IdentityUserRole<int>
    {
        public AppUser User { get; set; }
        
        public AppRole Role { get; set; }
        
        
    }
}