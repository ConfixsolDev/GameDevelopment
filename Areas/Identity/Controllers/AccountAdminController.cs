using TechWebSol.Controllers;
using TechWebSol.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel;
using System.Text;
using TechWebSol.Filters;
using TechWebSol.Models;
using TechWebSol.ViewModels;
using TechWebSol.Services;
using TechWebSol.Constants;

namespace TechWebSol.Areas.Identity.Controllers
{
    [AuthorizeDynamic]
    [Area("Identity")]
    [DisplayName("Account Admin: Permission for User Account managment across applications")]
    public class AccountAdminController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly ILogger<LoginViewModel> _logger;
        public readonly ApplicationDbContext _context;

        private readonly IUserSessionService _userSessionService;
        public string SessionKeyName;
        public const string PlociesRoles = "RolePolicies";
        private readonly IWebHostEnvironment _appEnvironment;
        private readonly ApplicationUserVM CurrentUser;
        public ApplicationUserVM SessionInfoApUser { get; private set; }

        public AccountAdminController(IUserSessionService userSessionService, UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager, ILogger<LoginViewModel> logger,
            IWebHostEnvironment appEnvironment, ApplicationDbContext context)
        {
            _userSessionService = userSessionService;
            _userManager = userManager;
            _signInManager = signInManager;
            _logger = logger;
            _context = context;
            _appEnvironment = appEnvironment;
            CurrentUser = userSessionService.GetCurrentUser();
            SessionKeyName = AppConstants.UserSessionKey;
        }

        [HttpGet]
        [AllowAnonymous]
        [DisplayName("User: Application User Home URL")]
        public IActionResult SetUserUrl()
        {
            return View();
        }

        [HttpPost]
        [DisplayName("User: Application User Home URL")]
        [AllowAnonymous]
        public async Task<IActionResult> SetUserUrl(ApplicationUserApp applicationUserApp)
        {
            if (applicationUserApp.Id == Guid.Empty)
            {
                return View();
            }
            var AppUserUrl = await _context.Users.FirstOrDefaultAsync(x => x.Id.Equals(CurrentUser.ApplicationUserId));

            if (AppUserUrl == null)
            {
                return NotFound();
            }
            AppUserUrl.HomeUrl = applicationUserApp.HomeUrl;
            _context.Users.Update(AppUserUrl);
            await _context.SaveChangesAsync();
            return Redirect(applicationUserApp.HomeUrl);
        }


        [DisplayName("User: Reset Account Password for (Users)")]
        public IActionResult PasswordReset()
        {
            try
            {
                var UserDetails = _userSessionService.GetCurrentUser();
                PasswordResetViewModel model = new PasswordResetViewModel
                {
                    UserName = UserDetails.FullName,
                    ApplilicationUserId = UserDetails.ApplicationUserId.ToString()
                };
                return PartialView(model);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost]
        [DisplayName("User: Reset Account Password for (Users)")]
        public async Task<IActionResult> PasswordReset(PasswordResetViewModel PasswordResetViewModel)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(PasswordResetViewModel.ApplilicationUserId);
                if (user == null)
                {
                    return NotFound("User not found.");
                }

                // Check the old password is correct
                var passwordResult = _userManager.PasswordHasher.VerifyHashedPassword(user, user.PasswordHash, PasswordResetViewModel.OldPassword);
                if (passwordResult != PasswordVerificationResult.Success)
                {
                    return BadRequest("Invalid old password.");
                }

                // Reset the password
                string resetToken = await _userManager.GeneratePasswordResetTokenAsync(user);
                IdentityResult updateResult = await _userManager.ResetPasswordAsync(user, resetToken, PasswordResetViewModel.Password);

                if (!updateResult.Succeeded)
                {
                    var errorMessage = string.Join("; ", updateResult.Errors.Select(e => e.Description));
                    return BadRequest(errorMessage);
                }

                return Ok("Password reset successfully.");
            }
            catch (Exception)
            {
                // Consider logging the exception
                return Problem("An error occurred while resetting the password. Please try again later.");
            }
        }

        [AllowAnonymous]
        public async Task<IActionResult> ChangePicture(string id)
        {
            var ant = await _context.Users.FirstOrDefaultAsync(x => x.Id == id);
            return View(ant);
        }
        [DisplayName("Admin:  List Permissions")]
        public async Task<IActionResult> Index()
        {
            // Efficiently fetch all necessary data
            var users = await _context.Users.AsNoTracking().ToListAsync();
            var rolesAsync = await _context.Roles.ToListAsync();
            var userRoles = await _context.UserRoles.ToListAsync();

            foreach (var user in users)
            {
                var selectUserRoles = userRoles.Where(x => x.UserId == user.Id).ToList();
                if (selectUserRoles.Any())
                {
                    System.Text.StringBuilder roleNameBuilder = new StringBuilder();
                    StringBuilder appAssignedBuilder = new StringBuilder();

                    foreach (var userRole in selectUserRoles)
                    {
                        var selectedRole = rolesAsync.FirstOrDefault(role => role.Id == userRole.RoleId);
                        if (selectedRole != null)
                        {
                            if (roleNameBuilder.Length > 0)
                            {
                                roleNameBuilder.Append(",");
                                appAssignedBuilder.Append(",");
                            }

                            roleNameBuilder.Append(selectedRole.ApplicationId).Append(":").Append(selectedRole.Name);
                            appAssignedBuilder.Append(selectedRole.ApplicationId);
                        }
                    }
                    user.RoleName = roleNameBuilder.ToString();
                }
                else
                {
                    user.RoleName = string.Empty;
                }
            }

            return View(users);
        }

        [DisplayName("Admin:  Account De-Activating  Permissions")]
        public async Task<IActionResult> Delete(string id)
        {
            try
            {
                var DeleteUser = _context.Users.FirstOrDefault(c => c.Id == id);
                DeleteUser.IsActive = false;
                await _context.SaveChangesAsync();
            }
            catch (Exception)
            {
            }
            return RedirectToAction("Index");
        }

        [DisplayName("Admin:  Account Activating  Permissions")]
        public async Task<IActionResult> Activate(string id)
        {
            try
            {
                var DeleteUser = _context.Users.FirstOrDefault(c => c.Id == id);
                DeleteUser.IsActive = true;
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
            }
            return RedirectToAction("Index");
        }

        [AllowAnonymous]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePicture(ApplicationUser registerViewModel)
        {

            var files = HttpContext.Request.Form.Files;

            foreach (var Image in files)
            {
                if (Image != null && Image.Length > 0)
                {
                    var file = Image;
                    //There is an error here
                    var path = Path.Combine(_appEnvironment.WebRootPath, "usersprofileimage");
                    if (file.Name.Length > 0)
                    {
                        var fileName = Guid.NewGuid() + Path.GetExtension(file.FileName);
                        using var fileStream = new FileStream(Path.Combine(path, fileName), FileMode.Create);
                        file.CopyTo(fileStream);
                    }
                }
            }
            _context.Update(User);
            await _context.SaveChangesAsync();
            return RedirectToAction("Index", "Home");
        }



        [HttpGet]
        [DisplayName("Admin:  Create New Account")]
        public async Task<IActionResult> Register()
        {
            RegisterViewModel registerView = new RegisterViewModel
            {
                UserName = "",
                RoleList = await _context.Roles.Select(r => new SelectListItem
                                        {
                                            Value = r.Name,
                                            Text = r.Name,
                                        }).ToListAsync(),
                TeamList = await _context.Teams
                    .Where(t => t.IsActive)
                    .Select(t => new SelectListItem
                    {
                        Value = t.Id.ToString(),
                        Text = $"{t.Name} ({t.TeamCode})"
                    }).ToListAsync()
            };
            ViewData["DesignationId"] = "";
            ViewData["SystemUser"] =  "";
            var teamTypes = await _context.TeamTypes.AsNoTracking().Select(tt => new { tt.Id, tt.Name }).ToListAsync();
            ViewData["TeamTypeId"] = new SelectList(teamTypes, "Id", "Name");
            return PartialView(registerView);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [DisplayName("Admin:  Create New Account")]
        public async Task<IActionResult> Register(RegisterViewModel Input)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    if (Input.UserName != null)
                    {
                        var files = HttpContext.Request.Form.Files;
                        var user = new ApplicationUser
                        {
                            UserName = Input.UserName.Trim(),
                            Email = Input.UserName.Trim(),
                            FirstName = Input.FirstName,
                            LastName = Input.LastName,
                            Designation = Input.Designation,
                            CreatedDate = DateTime.Now,
                            UpdatedDate = DateTime.Now,
                            IsActive = true,
                            Id = Input.ApplicationUserID.ToString()
                        };


                        try
                        {
                            var result = await _userManager.CreateAsync(user, Input.Password);

                        if (result.Succeeded == false)
                        {
                          
                            ViewBag.UserName = user.UserName;

                            Input.RoleList = await _context.Roles.Select(r => new SelectListItem
                                                            {
                                                                Value = r.Name,
                                                                Text = r.Name,
                                                            }).ToListAsync();
                                return Problem("User Already Exists");
                        }
                        else if (result.Succeeded)
                        {
                            var user1 = new ApplicationUserApp
                            {
                                UserName = Input.UserName.Trim(),
                                Email = Input.UserName.Trim(),
                                Id = Guid.Parse(user.Id),
                            };

                            _context.Add(user1);
                            _context.SaveChanges();

                            foreach (var item in Input.Role)
                            {
                                await _userManager.AddToRoleAsync(user, item);
                            }
                            await _context.SaveChangesAsync();
                            return Ok($"User Account for {user1.UserName} Created Successfully");
                        }
                        }
                        catch
                        {
                            return Problem("User Already Exists");
                        }
                    }
                }
                else
                {
                    var teamTypes = await _context.TeamTypes.AsNoTracking().Select(tt => new { tt.Id, tt.Name }).ToListAsync();
                    ViewData["TeamTypeId"] = new SelectList(teamTypes, "Id", "Name");
                    return BadRequest();
                }
            }
            catch (Exception ex)
            {
                return Problem(ex.Message);
            }
            return Ok("Some thing went wrong");
        }

        [DisplayName("Admin:  Edit Account Details")]
        public async Task<IActionResult> EditRegister(string id)
        {
            // Fetch the user details
            var UserDetails = _context.Users.AsNoTracking().FirstOrDefault(c => c.Id == id);

            var roles = _context.Roles.ToList();
            var userRoles = _context.UserRoles
                                    .AsNoTracking()
                                    .Where(c => c.UserId == UserDetails.Id)
                                    .ToList();

            var selectedRoleNames = new HashSet<string>();
            foreach (var item in userRoles)
            {
                selectedRoleNames.Add(roles.FirstOrDefault(x => x.Id == item.RoleId).Name);
            }
            ViewData["DesignationId"] = "";

            var allRoles = roles.Select(r => new SelectListItem
                                            {
                                                Value = r.Name,
                                                Text = r.Name,
                                                Selected = selectedRoleNames.Contains(r.Name)
                                            }).ToList();

            RegisterViewModel register = new RegisterViewModel
            {
                RoleList = allRoles,
                ApplicationUserID = id,
                Email = UserDetails.Email,
                FirstName = UserDetails.FirstName,
                LastName = UserDetails.LastName,
                UserName = UserDetails.UserName,
                Designation = UserDetails.Designation,
            };
            var teamTypes = await _context.TeamTypes.AsNoTracking().Select(tt => new { tt.Id, tt.Name }).ToListAsync();
            ViewData["TeamTypeId"] = new SelectList(teamTypes, "Id", "Name");
            return PartialView(register);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [DisplayName("Admin:  Edit Account Details")]
        public async Task<IActionResult> EditRegister(RegisterViewModel registerViewModel, string returnUrl = null)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest();
                }

                var user = await _context.Users.FirstOrDefaultAsync(c => c.Id == registerViewModel.ApplicationUserID);
                if (user == null)
                {
                    return NotFound("User not found.");
                }

                var roles = await _userManager.GetRolesAsync(user);
                if (registerViewModel.Role.Count() > 0)
                {
                    await _userManager.RemoveFromRolesAsync(user, roles.ToArray());
                    foreach (var item in registerViewModel.Role)
                    {
                        await _userManager.AddToRoleAsync(user, item);
                    }
                }
                user.UserName = registerViewModel.UserName;
                user.Email = registerViewModel.Email;  // Assuming you have a separate field for email in your view model
                user.Designation = registerViewModel.Designation;
                user.FirstName = registerViewModel.FirstName;
                user.LastName = registerViewModel.LastName;
                user.UpdatedDate = DateTime.Now;
                user.IsActive = true;

                var result = await _userManager.UpdateAsync(user);
                if (!result.Succeeded)
                {
                    return BadRequest(string.Join("; ", result.Errors.Select(e => e.Description)));
                }
                await _context.SaveChangesAsync();

                // Sync with another context if necessary
                var applicationUserApp = await _context.Users.FirstOrDefaultAsync(x => x.Id == user.Id);
                if (applicationUserApp != null)
                {
                    applicationUserApp.UserName = user.UserName;
                    applicationUserApp.Email = user.Email;
                    applicationUserApp.Designation = user.Designation;
                    applicationUserApp.Department = user.Department;
                    _context.Update(applicationUserApp);
                    _context.SaveChanges();
                }
                else
                {
                    try
                    {
                        var user1 = new ApplicationUserApp
                        {
                            UserName = user.UserName.Trim(),
                            Email = user.UserName.Trim(),
                            Id = Guid.Parse(user.Id),
                            Designation = "Manager",
                            Department = "HR",
                        };
                        _context.Update(user1);
                        _context.SaveChanges();
                    }
                    catch (Exception)
                    {
                        return Ok("User need to Employee of Organization");
                    }
                }

                return Ok("User Data Updated");
            }
            catch (Exception)
            {
                return Problem("An error occurred while processing your request.");
            }
        }

        [AllowAnonymous]
        public IActionResult AccessDenied()
        {
            return View();
        }

        [DisplayName("Admin: Reset Password for any Account")]
        public IActionResult AdminPasswordReset(string Id)
        {
            try
            {
                var UserDetails = _context.Users.FirstOrDefault(x=>x.Id == Id);
                PasswordResetViewModel model = new PasswordResetViewModel
                {
                    UserName = UserDetails.UserName,
                    ApplilicationUserId = UserDetails.Id.ToString()
                };
                return PartialView(model);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost]
        [DisplayName("Admin: Reset Password for any Account")]
        public async Task<IActionResult> AdminPasswordReset(PasswordResetViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var user = await _userManager.FindByIdAsync(model.ApplilicationUserId);
                if (user == null)
                {
                    return NotFound($"User with ID {model.ApplilicationUserId} not found.");
                }

                string resetToken = await _userManager.GeneratePasswordResetTokenAsync(user);
                var result = await _userManager.ResetPasswordAsync(user, resetToken, model.Password);

                if (!result.Succeeded)
                {
                    return Problem(string.Join("; ", result.Errors.Select(e => e.Description)), title: "Password reset failed");
                }

                // Optionally send an email to the user
                // await _emailService.SendPasswordResetConfirmationAsync(user.Email);
                return Ok("Password reset successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Password reset failed for user {UserId}.", model.ApplilicationUserId);  // Log the exception
                return Problem(ex.Message, title: "An error occurred while resetting the password");
            }
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult Lockout()
        {
            return View();
        }

        private IActionResult RedirectToLocal(string returnUrl)
        {
            if (Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            else
            {
                return RedirectToAction(nameof(HomeController), "Home");
            }
        }
    }
}