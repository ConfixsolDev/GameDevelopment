using TechWebSol.ViewModels;
using TechWebSol.Filters;
using TechWebSol.Services;
using TechWebSol.Data;
using TechWebSol.Constants;
using TechWebSol.Extensions;
using TechWebSol.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System.ComponentModel;

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
            if (ModelState.IsValid)
            {
                try
                {
                    var result = await _signInManager.PasswordSignInAsync(model.UserName.Trim(), model.Password, false, lockoutOnFailure: true);
                    if (result.Succeeded)
                    {
                        var user = _Appcontext.Users.AsNoTracking().FirstOrDefault(x => x.UserName == model.UserName.Trim());
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
                                UserCode = user.UserCode,
                                FullName = user.FullName,
                                RoleName = roles?.Name ?? "User",
                                DepartmentId = "HR",
                                DesignationId = "Manager",
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

                            if (returnUrl != null)
                            {
                                return Redirect(returnUrl);
                            }
                            if (ant.HomeUrl == null)
                            {
                                return Redirect("/Home");
                            }
                            return Redirect(ant.HomeUrl);
                        }
                    }
                    await _signInManager.SignOutAsync();
                    ViewBag.Error = "Invalid username or password.";
                }
                catch (Exception e)
                {
                    ViewBag.Error = e.Message;
                }
            }
            return View(model);
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
                        DepartmentId = "HR",
                        DesignationId = "Manager",
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
