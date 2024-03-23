using AuthorizationApp.Server.Models;

namespace AuthorizationApp.Server.Helpers
{
    public static class UsersHelper
    {
        public static bool HasAdminRights(IList<string> roles)
        {

            if (roles != null && roles.Any())
            {
                return roles.Contains("Admin");
            }
            return false;
        }

        public static string GetRolesString(IList<string> roles)
        {
            if (roles != null && roles.Any())
            {
                return string.Join(",", roles);
            }
            return "";
        }

        public static void SetImage(ApplicationUser user, IFormFile file)
        {
            byte[] bytes = null;
            using (Stream fs = file.OpenReadStream())
            {
                using (BinaryReader br = new BinaryReader(fs))
                {
                    bytes = br.ReadBytes((int)fs.Length);
                }
            }
            user.ImgType = file.ContentType;
            user.ImgData = Convert.ToBase64String(bytes);

        }
    }
}
