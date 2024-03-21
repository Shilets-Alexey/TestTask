using AuthorizationApp.Server.Models;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using System;
using System.Diagnostics;

namespace AuthorizationApp.Server.Helpers
{
    public static class WorkWithUsersHelper
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

        public static ValidationProblem CreateValidationProblem(string errorCode, string errorDescription) =>
        TypedResults.ValidationProblem(new Dictionary<string, string[]> {
            { errorCode, [errorDescription] }
        });

        public static ValidationProblem CreateValidationProblem(IdentityResult result)
        {
            // We expect a single error code and description in the normal case.
            // This could be golfed with GroupBy and ToDictionary, but perf! :P
            Debug.Assert(!result.Succeeded);
            var errorDictionary = new Dictionary<string, string[]>(1);

            foreach (var error in result.Errors)
            {
                string[] newDescriptions;

                if (errorDictionary.TryGetValue(error.Code, out var descriptions))
                {
                    newDescriptions = new string[descriptions.Length + 1];
                    Array.Copy(descriptions, newDescriptions, descriptions.Length);
                    newDescriptions[descriptions.Length] = error.Description;
                }
                else
                {
                    newDescriptions = [error.Description];
                }

                errorDictionary[error.Code] = newDescriptions;
            }

            return TypedResults.ValidationProblem(errorDictionary);
        }
    }
}
