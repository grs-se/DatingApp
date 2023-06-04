using Microsoft.AspNetCore.Identity;

namespace API.Entities
{
    // Entity can autogenerate roles but we want maximum control by managing join table ourselves
    public class AppRole : IdentityRole<int>
    {
        public ICollection<AppUserRole> UserRoles { get; set; }
    }
}
