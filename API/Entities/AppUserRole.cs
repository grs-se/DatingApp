using Microsoft.AspNetCore.Identity;

namespace API.Entities
{
    // This will represent join table between AppUser and Roles
    public class AppUserRole : IdentityUserRole<int>
    {
        public AppUser User { get; set; }
        public AppRole Role { get; set; }
    }
}
