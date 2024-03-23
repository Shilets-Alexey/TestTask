using AuthorizationApp.Web.Helpers;
using AuthorizationApp.Web.Models;
using AutoMapper;
using Microsoft.AspNetCore.Authentication.BearerToken;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.Data;

namespace AuthorizationApp.BusinessLogic.Services
{
    public class UsersService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IUserStore<ApplicationUser> _userStore;
        private readonly IMapper _mapper;

        public UsersService(UserManager<ApplicationUser> userManager,
                                SignInManager<ApplicationUser> signInManager,
                                IUserStore<ApplicationUser> userStore,
                                IMapper mapper)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _userStore = userStore;
            _mapper = mapper;
        }

        public async Task<ServiceResult<IEnumerable<User>>> GetUsersAsync(bool isCurrntUserAdmin)
        {
            var users = _userManager.Users.ToList();
            var usersDto = new List<User>();
            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);
                User userDto = _mapper.Map<User>(user);
                userDto.UserRoles = UsersHelper.GetRolesString(roles);
                userDto.CanDelete = !UsersHelper.HasAdminRights(roles) && isCurrntUserAdmin;
                userDto.CanGrantRole = !UsersHelper.HasAdminRights(roles) && isCurrntUserAdmin;
                usersDto.Add(userDto);
            }
            return ServiceResult<IEnumerable<User>>.Create(usersDto);
        }

        public async Task<ServiceResult<User>> GetCurrentUserAsync(string currentUserName)
        {
            ApplicationUser user = await _userManager.FindByNameAsync(currentUserName);
            User userDto = _mapper.Map<User>(user);
            return ServiceResult<User>.Create(userDto);
        }

        public async Task<ValidationResult> UpdatePhotoAsync(IFormFile file, Guid userId, string currentUserName)
        {
            ApplicationUser currentUser = await _userManager.FindByNameAsync(currentUserName);
            ApplicationUser user = await _userManager.FindByIdAsync(userId.ToString());
            ValidationResult validationInfo = ValidationHelper.OnUpdatePhoto(currentUser, user);
            if (!validationInfo.IsValid)
            {
                return validationInfo;
            }
            User usere = null;
            User usere2 = default;
            if (file != null)
            {

                UsersHelper.SetImage(user, file);
                var setImageResult = await _userManager.UpdateAsync(user);
                if (!setImageResult.Succeeded)
                {
                    return ValidationHelper.CreateValidationProblem(setImageResult);
                }
            }
            return validationInfo;
        }

        public async Task<Results<Ok<AccessTokenResponse>, EmptyHttpResult, ProblemHttpResult>> LoginAsync(LoginRequest login, bool? useCookies, bool? useSessionCookies)
        {
            bool useCookieScheme = (useCookies == true) || (useSessionCookies == true);
            bool isPersistent = (useCookies == true) && (useSessionCookies != true);
            _signInManager.AuthenticationScheme = useCookieScheme ? IdentityConstants.ApplicationScheme : IdentityConstants.BearerScheme;
            SignInResult signInResult = await _signInManager.PasswordSignInAsync(login.Email, login.Password, isPersistent, lockoutOnFailure: true);
            if (signInResult.RequiresTwoFactor)
            {
                if (!string.IsNullOrEmpty(login.TwoFactorCode))
                {
                    signInResult = await _signInManager.TwoFactorAuthenticatorSignInAsync(login.TwoFactorCode, isPersistent, rememberClient: isPersistent);
                }
                else if (!string.IsNullOrEmpty(login.TwoFactorRecoveryCode))
                {
                    signInResult = await _signInManager.TwoFactorRecoveryCodeSignInAsync(login.TwoFactorRecoveryCode);
                }
            }
            if (!signInResult.Succeeded)
            {
                return TypedResults.Problem(signInResult.ToString(), statusCode: StatusCodes.Status401Unauthorized);
            }
            ApplicationUser? user = await _userManager.FindByEmailAsync(login.Email);
            if (user != default)
            {
                user.LastLoginDate = DateTime.UtcNow;
                user.SucceededLoginsCount += 1;
                await _userManager.UpdateAsync(user);
            }
            return TypedResults.Empty;
        }

        public async Task<ValidationResult> RegisterAsync(RegisterRequest registration)
        {
            if (!_userManager.SupportsUserEmail)
            {
                throw new NotSupportedException($"{nameof(RegisterAsync)} requires a user store with email support.");
            }
            IUserEmailStore<ApplicationUser> emailStore = (IUserEmailStore<ApplicationUser>)_userStore;
            string email = registration.Email;
            ApplicationUser user = new ApplicationUser();
            ValidationResult validationInfo = ValidationHelper.OnRegeisterValidate(email, [_userManager.ErrorDescriber.InvalidEmail(email)]);
            if (!validationInfo.IsValid)
            {
                return validationInfo;
            }
            await _userStore.SetUserNameAsync(user, email, CancellationToken.None);
            await emailStore.SetEmailAsync(user, email, CancellationToken.None);
            user.LastLoginDate = DateTime.UtcNow;
            user.SucceededLoginsCount = 1;
            IdentityResult createResult = await _userManager.CreateAsync(user, registration.Password);
            if (!createResult.Succeeded)
            {
                return ValidationHelper.CreateValidationProblem(createResult);
            }
            await _signInManager.SignInAsync(user, false);
            return validationInfo;
        }

        public async Task LogoutAsync()
        {
            await _signInManager.SignOutAsync();
        }

        public ServiceResult<object> PingAuth(bool IsAuthenticated, string currentUserName)
        {
            return ServiceResult<object>.Create(new { isAutorized = IsAuthenticated, email = currentUserName });
        }




    }
}
