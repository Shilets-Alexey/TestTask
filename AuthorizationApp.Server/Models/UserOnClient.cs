using System.Text.Json.Serialization;

namespace AuthorizationApp.Server.Models
{
    /// <summary>
    /// Class for sent user info to client
    /// </summary>
    [Serializable]
    public class UserOnClient
    {
        [JsonPropertyName("id")]
        public string Id { get; set; }

        [JsonPropertyName("userName")]
        public string UserName { get; set; }

        [JsonPropertyName("userRoles")]
        public string UserRoles { get; set; }

        [JsonPropertyName("succesLogins")]
        public int SuccesLogins { get; set; }

        [JsonPropertyName("lastLogin")]
        public string LastLogin { get; set; }

        [JsonPropertyName("profileImg")]
        public string ProfileImg { get; set; }

        [JsonPropertyName("imageType")]
        public string ImageType { get; set; }

        [JsonPropertyName("isBtnVisible")]
        public bool IsBtnVisible { get; set; }

        /// <summary>
        /// Convert  ApplicationUser to UserOnClient
        /// </summary>
        public static implicit operator UserOnClient(ApplicationUser user)
        {
            return new UserOnClient
            {
                Id = user.Id,
                UserName =  user.UserName,
                SuccesLogins= user.SuccesLoggCount,
                LastLogin = user.LastLoginDate == default ? null : user.LastLoginDate.ToString(),
                ProfileImg = user.ImgData,
                ImageType = user.ImgType
            };
        }
    }
}
