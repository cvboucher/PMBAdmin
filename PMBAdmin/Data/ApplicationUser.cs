using Microsoft.AspNetCore.Identity;

namespace PMBAdmin.Data
{
    // Add profile data for application users by adding properties to the ApplicationUser class
    public class ApplicationUser : IdentityUser
    {
        public string? DisplayName { get; set; }
        public bool SuperUser { get; set; }
        public bool ViewAllAgencies { get; set; }
    }
}
