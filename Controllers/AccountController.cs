using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System.ComponentModel;
using TechWebSol.Constants;
using TechWebSol.Data;
using TechWebSol.Extensions;
using TechWebSol.Filters;
using TechWebSol.Helpers;
using TechWebSol.Models;
using TechWebSol.Services;
using TechWebSol.ViewModels;

namespace TechWebSol.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<Models.ApplicationUser> _userManager;
        private readonly SignInManager<Models.ApplicationUser> _signInManager;
        private readonly ILogger<AccountController> _logger;
        public readonly ApplicationDbContext _Appcontext;

        private readonly IUserSessionService _userSessionService;
        public string SessionKeyName;
        private readonly ApplicationUserVM CurrentUser;

        public AccountController(IUserSessionService userSessionService,
            ApplicationDbContext applicationDbContext,
            UserManager<Models.ApplicationUser> userManager, 
            SignInManager<Models.ApplicationUser> signInManager, 
            ILogger<AccountController> logger)
        {
            _userSessionService = userSessionService;
            _userManager = userManager;
            _signInManager = signInManager;
            _logger = logger;
            _Appcontext = applicationDbContext;
            CurrentUser = userSessionService.GetCurrentUser();
            SessionKeyName = AppConstants.UserSessionKey;
        }

        [HttpGet]
        public async Task<IActionResult> Logout()
        {
            try
            {
                // Removing Session
                HttpContext.Session.Clear();
                // Removing Cookies
                CookieOptions option = new CookieOptions();

                var ant = await _Appcontext.Users.FirstOrDefaultAsync(x => x.UserName == User.Identity.Name);
                try
                {
                    //ant.IsOnline = false;
                }
                catch (Exception)
                {
                }
                await _Appcontext.SaveChangesAsync();
                await _signInManager.SignOutAsync();
                return RedirectToAction("Login", "Account");
            }
            catch (Exception)
            {
                // SessionMessage.InitiateSessionMessage(PageAlertType.Error, "Account", "You are already logout");
                return RedirectToAction("Login", "Account");
            }
        }

        [HttpPost]
        public IActionResult ClearSession()
        {
            try
            {
                // Clear session data
                HttpContext.Session.Clear();
                _userSessionService.ClearCurrentUser();
                
                // Return success response
                return Json(new { success = true, message = "Session cleared" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error clearing session");
                return Json(new { success = false, message = "Error clearing session" });
            }
        }

        public IActionResult AccessDenied()
        {
            return View();
        }

        [DisplayName("Login")]
        public IActionResult Login(string returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            if (CurrentUser != null)
            {
                return RedirectToAction("Index", "Home");
            }

            LoginViewModel loginModel = new LoginViewModel();
            return View(loginModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model, string returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            if (!ModelState.IsValid) return View(model);

            try
            {
                var userName = model.UserName?.Trim();
                var password = model.Password ?? string.Empty;
                var signInResult = await _signInManager.PasswordSignInAsync(userName, password, false, true);

                if (!signInResult.Succeeded)
                {
                    await _signInManager.SignOutAsync();
                    ViewBag.Error = "Invalid username or password.";
                    return View(model);
                }

                var user = await _Appcontext.Users.AsNoTracking().FirstOrDefaultAsync(x => x.UserName == userName);
                if (user == null)
                {
                    await _signInManager.SignOutAsync();
                    ViewBag.Error = "Account not found.";
                    return View(model);
                }

                _logger.LogInformation("User logged in.");

             

                var role = await (
                    from usr in _Appcontext.Users
                    join userRole in _Appcontext.UserRoles on usr.Id equals userRole.UserId
                    join r in _Appcontext.Roles.Where(x => x.ApplicationId == AppConstants.Id) on userRole.RoleId equals r.Id
                    where usr.UserName == user.UserName
                    select r
                ).AsNoTracking().FirstOrDefaultAsync();

                var applicationUserVM = new ApplicationUserVM
                {
                    ApplicationUserId = user.Id,
                    UserCode = user.UserCode,
                    FullName = user.FullName,
                    RoleName = role?.Name ?? "User",
                    TeamId = user.TeamId,
                    ForceType = user.ForceType,
                };


                if (user.TeamId != null)
                {
                    applicationUserVM.ApplicationRole = false;
                }
                else
                {
                    applicationUserVM.ApplicationRole = false;
                }

                HttpContext.Session.SetObject(SessionKeyName, applicationUserVM);

                if (!string.IsNullOrWhiteSpace(role?.Access))
                {
                    try
                    {
                        var access = JsonConvert.DeserializeObject<IEnumerable<ViewModels.MvcControllerInfoArea>>(role.Access);
                        if (access != null) RolesList.AddValue(role.Name, access);
                    }
                    catch { }
                }

                var trackedUser = await _Appcontext.Users.FirstOrDefaultAsync(x => x.Id == user.Id);
                if (trackedUser != null)
                {
                    trackedUser.LoginCount = trackedUser.LoginCount + 1;
                    trackedUser.LastLoginDate = DateTime.Now;
                    trackedUser.IsOnline = true;
                    await _Appcontext.SaveChangesAsync();
                }

                if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl)) return Redirect(returnUrl);
                var homeUrl = trackedUser?.HomeUrl;
                if (!string.IsNullOrWhiteSpace(homeUrl) && Url.IsLocalUrl(homeUrl)) return Redirect(homeUrl);
                return Redirect("/Home");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during login.");
                ViewBag.Error = "An unexpected error occurred during login.";
                return View(model);
            }
        }



        [HttpGet]
        [DisplayName("Account : Permission for Anonymous login")]
        public async Task<IActionResult> AnonymousLogin(string username)
        {
            if (string.IsNullOrEmpty(username))
            {
                ViewBag.Error = "Username is required.";
                return View();
            }

            try
            {
                var user = _Appcontext.Users.AsNoTracking().FirstOrDefault(x => x.UserName == username.Trim());
                if (user != null)
                {
                    _logger.LogInformation("User logged in.");

                    var roles = (
                        from usr in _Appcontext.Users
                        join userRole in _Appcontext.UserRoles on usr.Id equals userRole.UserId
                        join role in _Appcontext.Roles.Where(x => x.ApplicationId == AppConstants.Id) on userRole.RoleId equals role.Id
                        where usr.UserName == user.UserName
                        select role
                    ).AsNoTracking().FirstOrDefault();
                    if (roles == null)
                    {
                        roles = _Appcontext.Roles.FirstOrDefault(x => x.ApplicationId == AppConstants.Id && x.Id == "e7fd028f-98d4-4901-8f2d-89c9cf5c8722");
                    }

                    ApplicationUserVM applicaitonUserVM = new ApplicationUserVM
                    {
                        ApplicationUserId = user.Id,
                        UserCode = "Sys_" + user.UserCode,
                        FullName = "Sys_" + user.FullName,
                        RoleName = roles?.Name ?? "User",
                        TeamId = user?.TeamId,
                        ForceType = user?.ForceType
                    };

                    HttpContext.Session.SetObject<ApplicationUserVM>(SessionKeyName, applicaitonUserVM);

                    if (roles?.Access != null)
                    {
                        var MvcControllerInfoAreaList = JsonConvert.DeserializeObject<IEnumerable<ViewModels.MvcControllerInfoArea>>(roles.Access);
                        RolesList.AddValue(roles.Name, MvcControllerInfoAreaList);
                    }

                    var ant = await _Appcontext.Users.FirstOrDefaultAsync(x => x.Id == user.Id);
                    ant.LoginCount += 1;
                    ant.LastLoginDate = DateTime.Now;
                    ant.IsOnline = true;
                    await _Appcontext.SaveChangesAsync();

                    var AppUserUrl = await _Appcontext.Users.FirstOrDefaultAsync(x => x.Id.Equals(Guid.Parse(user.Id)));
                    
                    if (AppUserUrl.HomeUrl == null)
                    {
                        return Redirect("/Home");
                    }
                    return Redirect(AppUserUrl.HomeUrl);
                }
                else
                {
                    await _signInManager.SignOutAsync();
                    ViewBag.Error = "Invalid username or password.";
                }
            }
            catch (Exception e)
            {
                ViewBag.Error = e.Message;
            }

            return View();
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult Lockout()
        {
            return View();
        }
    }
}
