using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.Options;
using System.ComponentModel.DataAnnotations.Schema;
using System.Drawing;
using System.Net.Mail;

namespace AuthorizationApp.Server.Models
{
    public class ApplicationUser: IdentityUser
    {
        public string? ImgData { get; set; }

        public string? ImgType { get; set; }

        [NotMapped]
        public IFormFile ImgFile { get; set; }

        public int SucceededLoginsCount { get; set; }

        public DateTime LastLoginDate { get; set; }
    } 
}
