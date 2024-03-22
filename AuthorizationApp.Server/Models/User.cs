using System.Text.Json.Serialization;

namespace AuthorizationApp.Server.Models
{
    /// <summary>
    /// Class for sent user info to client
    /// </summary>
    [Serializable]
    public class User
    {
        [JsonPropertyName("id")]
        public string Id { get; set; }

        [JsonPropertyName("userName")]
        public string UserName { get; set; }

        [JsonPropertyName("userRoles")]
        public string UserRoles { get; set; }

        [JsonPropertyName("succeededLoginsCount")]
        public int SucceededLoginsCount { get; set; }

        [JsonPropertyName("lastLoginDate")]
        public string LastLoginDate { get; set; }

        [JsonPropertyName("imgData")]
        public string ImgData { get; set; }

        [JsonPropertyName("imgType")]
        public string ImgType { get; set; }

        [JsonPropertyName("isBtnVisible")]
        public bool IsBtnVisible { get; set; }
    }
}
