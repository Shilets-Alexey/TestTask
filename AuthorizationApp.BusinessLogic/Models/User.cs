﻿using System.Text.Json.Serialization;

namespace AuthorizationApp.Web.Models
{
    /// <summary>
    /// Class for sent user info to client
    /// </summary>
    public class User
    {
        public string Id { get; set; }

        public string UserName { get; set; }

        public string UserRoles { get; set; }

        public int SucceededLoginsCount { get; set; }

        public string LastLoginDate { get; set; }

        public string ImgData { get; set; }

        public string ImgType { get; set; }

        public bool CanDelete { get; set; }

        public bool CanGrantRole { get; set; }
    }
}
