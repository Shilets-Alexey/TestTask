using AuthorizationApp.Server.Helpers;
using AuthorizationApp.Server.Models;
using AutoMapper;
using Microsoft.AspNetCore.Authentication.BearerToken;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages;
using System.ComponentModel.DataAnnotations;

namespace AuthorizationApp.Server.Controllers
{

    [ApiController]
    [Route("users")]
    public class UserController : ControllerBase
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IUserStore<ApplicationUser> _userStore;
        private readonly IMapper _mapper;
        private static readonly EmailAddressAttribute _emailAddressAttribute = new();

        public UserController(ApplicationDbContext dbContext,
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

        [HttpGet]
        public IResult GetUsers()
        {
            if (!User.Identity.IsAuthenticated)
            {
                return TypedResults.ValidationProblem(new Dictionary<string, string[]>() { { "user", new string[] { "Need sign in" } } });
            }
            List<ApplicationUser> users = _dbContext.Users.ToList();
            List<User> resultList = new List<User>();
            users.ForEach(user =>
            {
                var result = _userManager.GetRolesAsync(user);
                User userOnClient = _mapper.Map<User>(user);
                userOnClient.UserRoles = WorkWithUsersHelper.GetRolesString(result.Result);
                userOnClient.IsBtnVisible = !WorkWithUsersHelper.HasAdminRights(result.Result) && User.IsInRole("Admin");
                resultList.Add(userOnClient);
            });
            return Results.Json(resultList);
        }

        [HttpGet, Route("current")]
        public async Task<IResult> GetCurrentUser()
        {
            if (!User.Identity.IsAuthenticated)
            {
                return TypedResults.ValidationProblem(new Dictionary<string, string[]>() { { "user", new string[] { "Need sign in" } } });
            }
            ApplicationUser applicationUser = await _userManager.FindByNameAsync(User.Identity.Name);
            User user = _mapper.Map<User>(applicationUser);
            return Results.Json(user);
        }

        [HttpPatch, Route("{userId}/photo")]
        public async Task<IResult> UpdatePhoto([FromForm] IFormFile file, Guid userId)
        {
            ApplicationUser CurrentUserInfo = await _userManager.FindByNameAsync(User.Identity.Name);
            if (!User.Identity.IsAuthenticated)
            {
                return TypedResults.ValidationProblem(new Dictionary<string, string[]>() { { "user", new string[] { "Need sign in" } } });
            }
            if (CurrentUserInfo.Id != userId.ToString())
            {
                return TypedResults.ValidationProblem(new Dictionary<string, string[]>() { { "userId", new string[] { "Can't upload profile pic to another user" } } });
            }
            ApplicationUser user = await _userManager.FindByIdAsync(userId.ToString());
            if (user == default)
            {
                return TypedResults.ValidationProblem(new Dictionary<string, string[]>() { { "userId", new string[] { "User with this id dosen't exist" } } });
            }
            if (file != user.ImgFile)
            {
                if (file != null)
                {

                    WorkWithUsersHelper.SetImage(user, file);
                    var setImageResult = await _userManager.UpdateAsync(user);
                    if (!setImageResult.Succeeded)
                    {
                        return WorkWithUsersHelper.CreateValidationProblem(setImageResult);
                    }
                }
            }
            return Results.Ok();
        }

        [HttpPost, Route("login")]
        public async Task<Results<Ok<AccessTokenResponse>, EmptyHttpResult, ProblemHttpResult>> Login(
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
        public async Task<Results<Ok, ValidationProblem>> Register([FromBody] RegisterRequest registration)
        {
            if (!_userManager.SupportsUserEmail)
            {
                throw new NotSupportedException($"{nameof(Register)} requires a user store with email support.");
            }
            IUserEmailStore<ApplicationUser> emailStore = (IUserEmailStore<ApplicationUser>)_userStore;
            string email = registration.Email;

            if (string.IsNullOrEmpty(email) || !_emailAddressAttribute.IsValid(email))
            {
                return WorkWithUsersHelper.CreateValidationProblem(IdentityResult.Failed(_userManager.ErrorDescriber.InvalidEmail(email)));
            }

            ApplicationUser user = new ApplicationUser();
            await _userStore.SetUserNameAsync(user, email, CancellationToken.None);
            await emailStore.SetEmailAsync(user, email, CancellationToken.None);
            user.LastLoginDate = DateTime.UtcNow;
            user.SucceededLoginsCount = 1;
            IdentityResult result = await _userManager.CreateAsync(user, registration.Password);
            if (!result.Succeeded)
            {
                return WorkWithUsersHelper.CreateValidationProblem(result);
            }
            await _signInManager.SignInAsync(user, false);
            return TypedResults.Ok();
        }

        [Authorize]
        [HttpPost, Route("logout")]
        public async Task<IResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return Results.Ok();
        }

        [HttpGet, Route("pingauth")]
        public IResult PingAuth()
        {
            return Results.Json(new { isAuthenticated = User.Identity.IsAuthenticated });
        }


    }
}
