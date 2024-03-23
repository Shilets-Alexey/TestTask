using AuthorizationApp.Server.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using System.Diagnostics;
using System.Security.Claims;

namespace AuthorizationApp.Server.Helpers
{
    public class ValidationHelper
    {
        public static ValidationResult OnDeleteUserValidate(ClaimsPrincipal currentUser,
                                                        ApplicationUser CurrentUserInfo,
                                                        ApplicationUser user) {

            if (!currentUser.IsInRole("Admin"))
            {
                return ValidationResult.ValidationProblem(new Dictionary<string, string[]>() { { "user", new string[] { "You don't have admin rights" } } });
            }
            if (user == default)
            {
                return ValidationResult.ValidationProblem(new Dictionary<string, string[]>() { { "user", new string[] { "User not exist" } } });
            }
            if (CurrentUserInfo.Id == user.Id.ToString())
            {
                return ValidationResult.ValidationProblem(new Dictionary<string, string[]>() { { "user", new string[] { "Can't delete current user" } } });
            }
            
            
            return new ValidationResult();
        }

        public static ValidationResult HasAdminRightsValidate(IList<string> roles)
        {

            if (UsersHelper.HasAdminRights(roles))
            {
                return ValidationResult.ValidationProblem(new Dictionary<string, string[]>() { { "user", new string[] { "Can't delete users with admin righs" }} });
            }
            return new ValidationResult();
        }

        
           

        public static ValidationResult OnSetRoleValidate(ClaimsPrincipal currentUser, ApplicationUser user, string role)
        {

            if (!currentUser.IsInRole("Admin"))
            {
                return ValidationResult.ValidationProblem(new Dictionary<string, string[]>() { { "user", new string[] { "You don't have admin rights" } } });
            }
            if (user == default)
            {
                return ValidationResult.ValidationProblem(new Dictionary<string, string[]>() { { "user", new string[] { "User not exist" } } });

            }
            if (role.IsNullOrEmpty())
            {
                return ValidationResult.ValidationProblem(new Dictionary<string, string[]>() { { "role", new string[] { "Role is empty" } } });

            }
            return new ValidationResult();
        }

        public static ValidationResult OnUpdatePhoto(ClaimsPrincipal currentUser, ApplicationUser currentUserInfo, ApplicationUser user)
        {
            if (currentUserInfo.Id != user.Id.ToString())
            {
                return ValidationResult.ValidationProblem(new Dictionary<string, string[]>() { { "user", new string[] { "Can't upload profile pic to another user" } } });
            }
            if (user == default)
            {
                return ValidationResult.ValidationProblem(new Dictionary<string, string[]>() { { "user", new string[] { "User with this id dosen't exist" } } });
            }
            return new ValidationResult();
        }

        public static ValidationResult OnRegeisterValidate(string email, IdentityError[] messages)
        {
            var emailAddressAttribute = new System.ComponentModel.DataAnnotations.EmailAddressAttribute();
            if (string.IsNullOrEmpty(email) || !emailAddressAttribute.IsValid(email))
            {
                return CreateValidationProblem(IdentityResult.Failed(messages));
            }
            return new ValidationResult();
        }


        public static ValidationResult CreateValidationProblem(string errorCode, string errorDescription) =>
        ValidationResult.ValidationProblem(new Dictionary<string, string[]> {
            { errorCode, [errorDescription] }
        });

        public static ValidationResult CreateValidationProblem(IdentityResult result)
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

            return ValidationResult.ValidationProblem(errorDictionary);
        }
    }
}
