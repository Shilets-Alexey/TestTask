using AuthorizationApp.Server.Helpers;
using AuthorizationApp.Server.Models;
using AutoMapper;
using Microsoft.AspNetCore.Authentication.BearerToken;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages;

namespace AuthorizationApp.Server.Controllers
{

    [ApiController]
    [Route("users")]
    public class UsersController : ControllerBase
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IUserStore<ApplicationUser> _userStore;
        private readonly IMapper _mapper;

        public UsersController(ApplicationDbContext dbContext,
                                UserManager<ApplicationUser> userManager,
                                SignInManager<ApplicationUser> signInManager,
                                IUserStore<ApplicationUser> userStore,
                                IMapper mapper)
        {
            _userManager = userManager;
            _dbContext = dbContext;
            _signInManager = signInManager;
            _userStore = userStore;
            _mapper = mapper;
        }

        [Authorize]
        [HttpGet]
        public ServiceResult<List<User>> GetUsers()
        {
            List<ApplicationUser> users = _dbContext.Users.ToList();
            List<User> resultList = new List<User>();
            users.ForEach(user =>
            {
                var result = _userManager.GetRolesAsync(user);
                User userOnClient = _mapper.Map<User>(user);
                userOnClient.UserRoles = UsersHelper.GetRolesString(result.Result);
                userOnClient.IsBtnVisible = !UsersHelper.HasAdminRights(result.Result) && User.IsInRole("Admin");
                resultList.Add(userOnClient);
            });
            return ServiceResult<List<User>>.Create(resultList);
        }

        [Authorize]
        [HttpGet, Route("current")]
        public async Task<ServiceResult<User>> GetCurrentUserAsync()
        {
            ApplicationUser applicationUser = await _userManager.FindByNameAsync(User.Identity.Name);
            User user = _mapper.Map<User>(applicationUser);
            return ServiceResult<User>.Create(user);
        }

        [Authorize]
        [HttpPatch, Route("{userId}/photo")]
        public async Task<ValidationResult> UpdatePhotoAsync([FromForm] IFormFile file, Guid userId)
        {
            ApplicationUser currentUserInfo = await _userManager.FindByNameAsync(User.Identity.Name);
            ApplicationUser user = await _userManager.FindByIdAsync(userId.ToString());
            ValidationResult validationInfo =  ValidationHelper.OnUpdatePhoto(User, currentUserInfo, user);
            if (!validationInfo.IsValid)
            {
                return validationInfo;
            }
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

        [HttpPost, Route("login")]
        public async Task<Results<Ok<AccessTokenResponse>, EmptyHttpResult, ProblemHttpResult>> LoginAsync(
            [FromBody] LoginRequest login, 
            [FromQuery] bool? useCookies,
            [FromQuery] bool? useSessionCookies)
        {
            var useCookieScheme = (useCookies == true) || (useSessionCookies == true);
            var isPersistent = (useCookies == true) && (useSessionCookies != true);
            _signInManager.AuthenticationScheme = useCookieScheme ? IdentityConstants.ApplicationScheme : IdentityConstants.BearerScheme;

            var result = await _signInManager.PasswordSignInAsync(login.Email, login.Password, isPersistent, lockoutOnFailure: true);

            if (result.RequiresTwoFactor)
            {
                if (!string.IsNullOrEmpty(login.TwoFactorCode))
                {
                    result = await _signInManager.TwoFactorAuthenticatorSignInAsync(login.TwoFactorCode, isPersistent, rememberClient: isPersistent);
                }
                else if (!string.IsNullOrEmpty(login.TwoFactorRecoveryCode))
                {
                    result = await _signInManager.TwoFactorRecoveryCodeSignInAsync(login.TwoFactorRecoveryCode);
                }
            }

            if (!result.Succeeded)
            {
                return TypedResults.Problem(result.ToString(), statusCode: StatusCodes.Status401Unauthorized);
            }
            ApplicationUser? user = await _userManager.FindByEmailAsync(login.Email);
            if (user != null && user != default)
            {
                user.LastLoginDate = DateTime.UtcNow;
                user.SucceededLoginsCount += 1;
                await _userManager.UpdateAsync(user);
            }
            return TypedResults.Empty;
        }

        [HttpPost, Route("register")]
        public async Task<ValidationResult> RegisterAsync([FromBody] RegisterRequest registration)
        {
            if (!_userManager.SupportsUserEmail)
            {
                throw new NotSupportedException($"{nameof(Register)} requires a user store with email support.");
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
            IdentityResult result = await _userManager.CreateAsync(user, registration.Password);
            if (!result.Succeeded)
            {
                return ValidationHelper.CreateValidationProblem(result);
            }
            await _signInManager.SignInAsync(user, false);
            return validationInfo;
        }

        [Authorize]
        [HttpPost, Route("logout")]
        public async Task LogoutAsync()
        {
            await _signInManager.SignOutAsync();
        }

        [HttpGet, Route("pingauth")]
        public ServiceResult<object> PingAuth()
        {
            return ServiceResult<object>.Create(new { isAutorized = User.Identity.IsAuthenticated, email = User.Identity.Name});
        }


    }
}
