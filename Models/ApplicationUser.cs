using Microsoft.AspNetCore.Identity;

namespace ytpmd.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string YTLogin { get; set; }
		public string YTPassword { get; set; }
    }
}