using Microsoft.AspNetCore.Identity;

namespace AuthorizationApp.Web.Models
{
    public class ApplicationUser:IdentityUser
    {
        public string? ImgData { get; set; }

        public string? ImgType { get; set; }

        public int SucceededLoginsCount { get; set; }

        public DateTime LastLoginDate { get; set; }
    } 
}
