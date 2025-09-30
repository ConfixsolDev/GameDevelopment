using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.ComponentModel;
using TechWebSol.Constants;
using TechWebSol.Data;
using TechWebSol.Extensions;
using TechWebSol.Filters;
using TechWebSol.Models;
using TechWebSol.Services;
using TechWebSol.ViewModels;

namespace TechWebSol.Areas.Identity.Controllers
{
    [AuthorizeDynamic]
    [Area("Identity")]
    [DisplayName("Role Management: Assign to Super Admin")]
    public class RoleController : Controller
    {
        private readonly IMvcControllerDiscovery _mvcControllerDiscovery;
        private readonly RoleManager<ApplicationRole> _roleManager;
        private readonly ApplicationDbContext _context;

        public RoleController(IMvcControllerDiscovery mvcControllerDiscovery, RoleManager<ApplicationRole> roleManager, ApplicationDbContext context)
        {
            _mvcControllerDiscovery = mvcControllerDiscovery;
            _roleManager = roleManager;
            _context = context;
        }

        [DisplayName("Role Management: Detail View Permissions")]
        public IActionResult Details(string id)
        {
            try
            {
                ViewBag.roleName = _context.Roles.Where(x => x.Id == id).FirstOrDefault().Name;
                List<ApplicationUserRoleVM> applicationUserRoleVMs = new List<ApplicationUserRoleVM>();
                var UserList = (
                                  from usr in _context.Users
                                  join userRole in _context.UserRoles on usr.Id equals userRole.UserId
                                  join role in _context.Roles on userRole.RoleId equals role.Id
                                  where role.Id == id
                                  select usr).ToList();

                foreach (var item in UserList)
                {
                    string UserStatus = "Active";

                    if (item.IsActive == false)
                    {
                        UserStatus = "InActive";
                    }
                    ApplicationUserRoleVM applicationUserRoleVM = new ApplicationUserRoleVM()
                    {
                        UserId = item.UserName,
                        FullName = item.FullName,
                        Id = item.Id,
                        LoginCount = item.LoginCount,
                        LastLoginDate = item.LastLoginDate,
                        IsActive = UserStatus
                    };
                    applicationUserRoleVMs.Add(applicationUserRoleVM);
                }
                return View(applicationUserRoleVMs);
            }
            catch (Exception)
            {
                return RedirectToAction("Index");
            }
        }

        [DisplayName("Role Management: List View Permissions")]
        public ActionResult Index()
        {
            List<UserCountViewModel> RolesList = new List<UserCountViewModel>();
            var roles = _context.Roles.ToList();
            var UsersRoles = _context.UserRoles.ToList();
            var users = _context.Users.ToList();
            foreach (var item in roles)
            {
                int UserCount = 0;
                var UserId = UsersRoles.FirstOrDefault(c => c.RoleId == item.Id)?.UserId;
                var useridActive = users.FirstOrDefault(c => c.Id == UserId)?.IsActive;
                if (useridActive == true)
                {
                    UserCount = UsersRoles.Where(c => c.RoleId == item.Id).Count();
                }
                UserCountViewModel VM = new UserCountViewModel
                {
                    RollName = roles.FirstOrDefault(x => x.Id == item.Id)?.Name,
                    Count = UserCount,
                    ApplicationRoleId = item.Id,
                    AppId = item.ApplicationId
                };
                RolesList.Add(VM);
            }
            return View(RolesList);
            //return View();
        }

        [HttpPost]
        public IActionResult Index([FromForm]JQDTParams param)
        {
            var list = _context.Roles.AsQueryable();

            var totalRecords = list.Count();

            list = list.FilterRecords(param);

            var filteredRecords = list.Count();

            list = list.ApplyPaging(param);

            return Ok(new { param.draw, recordsFiltered = filteredRecords, recordsTotal = totalRecords, data = list.ToList() });
        }
        // GET: Role/Create
        [DisplayName("Role Management: Create New Role Permissions")]
        public ActionResult Create()
        {
            
            ViewData["ApplicationId"] = AppConstants.Id;
            ViewData["Controllers"] = GetControllers(AppConstants.Id);
            return View();
        }

        [Authorize]
        [Route("Update")]
        public async Task<IActionResult> Update()
        {
            var RoleAppUpdate = _mvcControllerDiscovery.GetControllers().OrderBy(x => x.AreaName).ThenBy(x => x.DisplayName);
            var accessJson = JsonConvert.SerializeObject(RoleAppUpdate);
            var Approles = _context.AppRoles.FirstOrDefault(x => x.AppId == AppConstants.Id);


            if (Approles == null)
            {
                AppRoles appRoles = new AppRoles()
                {
                    Id = Guid.NewGuid(),
                    RoleAccess = accessJson,
                    AppId = AppConstants.Id
                };
                _context.Add(appRoles);
            }
            else
            {
                Approles.RoleAccess = accessJson;
                UpdateSuperAdmin(Approles.RoleAccess);
                _context.Update(Approles);
            }
         await _context.SaveChangesAsync();
            return Content("Role Updated");
        }


        // POST: Role/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(RoleViewModel viewModel)
        {
            if (!ModelState.IsValid)
            {
                ViewData["Controllers"] = GetControllers(AppConstants.Id);
                return View(viewModel);
            }

            List<MvcControllerInfoArea> MvcControllerInfoAreaList = new List<MvcControllerInfoArea>();

            var role = new ApplicationRole { Name = viewModel.Name,
            ApplicationId= viewModel.AppId};
            if (viewModel.SelectedControllers != null && viewModel.SelectedControllers.Any())
            {
                //foreach (var controller in viewModel.SelectedControllers)
                //    foreach (var action in controller.Actions)
                //        action.ControllerId = controller.Id;

                foreach (var area in viewModel.SelectedControllers.GroupBy(x => x.AreaName))
                {
                    var ObjArea = new MvcControllerInfoArea();
                    ObjArea.AreaName = area.FirstOrDefault().AreaName;
                    List<MvcControllerInfoCont> MvcControllerInfoContList = new List<MvcControllerInfoCont>();
                    foreach (var controller1 in area)
                    {
                        var objcontroller = new MvcControllerInfoCont()
                        {
                            Id = controller1.Name,
                        };

                        List<MvcActionInfo> MvcActionInfo1List = new List<MvcActionInfo>();
                        foreach (var action in controller1.Actions)
                        {
                            var MvcActionInfo1 = new MvcActionInfo()
                            {
                                Name = action.Name,
                            };
                            MvcActionInfo1List.Add(MvcActionInfo1);
                        }
                        objcontroller.Actions = MvcActionInfo1List;
                        MvcControllerInfoContList.Add(objcontroller);

                    }
                    ObjArea.Controller = MvcControllerInfoContList;
                    MvcControllerInfoAreaList.Add(ObjArea);
                }
                var accessJson = JsonConvert.SerializeObject(MvcControllerInfoAreaList);
                role.Access = accessJson;
            }

            var result = await _roleManager.CreateAsync(role);
            if (result.Succeeded)
                return RedirectToAction(nameof(Index));

            foreach (var error in result.Errors)
                ModelState.AddModelError("", error.Description);

            ViewData["Controllers"] = GetControllers(AppConstants.Id);

            return View(viewModel);
        }

        // GET: Role/Edit/5
        [DisplayName("Role Management: Edit Role Permissions")]
        public async Task<ActionResult> Edit(string id)
        {
            try
            {
                ViewData["Controllers"] = GetControllers(AppConstants.Id);

                var role = await _roleManager.FindByIdAsync(id);
                if (role == null)
                    return NotFound();
                var viewModel = new RoleViewModel
                {
                    Name = role.Name
                };
                if (role.Access != null)
                {
                    var MvcControllerInfoArea = JsonConvert.DeserializeObject<List<MvcControllerInfoArea>>(role.Access);
                    viewModel.MvcControllerInfoCont = MvcControllerInfoArea.SelectMany(x => x.Controller).ToList();

                    return View(viewModel);
                }
                return View(viewModel);
            }
            catch (Exception)
            {

                throw;
            }
        }
        //[DisplayName("Role Management: Edit Role Permissions")]
        //public async Task<ActionResult> Edit(string id)
        //{
        //    ViewData["Controllers"] = GetControllers(AppConstants.Id);

        //    var role = await _roleManager.FindByIdAsync(id);
        //    if (role == null)
        //        return NotFound();

        //    var viewModel = new RoleViewModel
        //    {
        //        Name = role.Name,
        //        MvcControllerInfoCont = new List<MvcControllerInfoCont>()
        //    };

        //    if (string.IsNullOrWhiteSpace(role.Access))
        //        return View(viewModel);

        //    var controllers = JsonConvert.DeserializeObject<List<MvcControllerInfo>>(role.Access)
        //                      ?? new List<MvcControllerInfo>();

        //    var areas = new List<MvcControllerInfoArea>();

        //    foreach (var c in controllers)
        //    {
        //        var areaName = string.IsNullOrWhiteSpace(c.AreaName) ? "Default" : c.AreaName;

        //        var area = areas.FirstOrDefault(a => a.AreaName == areaName);
        //        if (area == null)
        //        {
        //            area = new MvcControllerInfoArea
        //            {
        //                AreaName = areaName,
        //                Controller = new List<MvcControllerInfoCont>()
        //            };
        //            areas.Add(area);
        //        }

        //        var cont = new MvcControllerInfoCont
        //        {
        //            Id = c.Id,
        //            Actions = (c.Actions ?? Enumerable.Empty<MvcActionInfo>()).ToList()
        //        };

        //        area.Controller.Add(cont);
        //    }

        //    viewModel.MvcControllerInfoCont = areas.SelectMany(x => x.Controller).ToList();

        //    return View(viewModel);
        //}



        // POST: Role/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(string id, RoleViewModel viewModel)
        {
            if (!ModelState.IsValid)
            {
                ViewData["Controllers"] = GetControllers(AppConstants.Id);
                return View(viewModel);
            }

            var role = await _roleManager.FindByIdAsync(id);
            if (role == null)
            {
                ModelState.AddModelError("", "Role not found");
                ViewData["Controllers"] = GetControllers(AppConstants.Id);
                return View();
            }

            role.Name = viewModel.Name;
            List<MvcControllerInfoArea> MvcControllerInfoAreaList = new List<MvcControllerInfoArea>();

            if (viewModel.SelectedControllers != null && viewModel.SelectedControllers.Any())
            {
                //foreach (var controller in viewModel.SelectedControllers)
                //    foreach (var action in controller.Actions)
                //        action.ControllerId = controller.Id;

                foreach (var area in viewModel.SelectedControllers.GroupBy(x => x.AreaName))
                {
                    var ObjArea = new MvcControllerInfoArea();
                    ObjArea.AreaName = area.FirstOrDefault().AreaName;
                    List<MvcControllerInfoCont> MvcControllerInfoContList = new List<MvcControllerInfoCont>();
                    foreach (var controller1 in area)
                    {
                        var objcontroller = new MvcControllerInfoCont()
                        {
                            Id = controller1.Name,
                        };

                        List<MvcActionInfo> MvcActionInfo1List = new List<MvcActionInfo>();
                        foreach (var action in controller1.Actions)
                        {
                            var MvcActionInfo1 = new MvcActionInfo()
                            {
                                Name = action.Name,
                            };
                            MvcActionInfo1List.Add(MvcActionInfo1);
                        }
                        objcontroller.Actions = MvcActionInfo1List;

                        MvcControllerInfoContList.Add(objcontroller);

                    }
                    ObjArea.Controller = MvcControllerInfoContList;
                    MvcControllerInfoAreaList.Add(ObjArea);
                }
                var accessJson = JsonConvert.SerializeObject(MvcControllerInfoAreaList);
                role.Access = accessJson;
            }

            var result = await _roleManager.UpdateAsync(role);
            if (result.Succeeded)
                return RedirectToAction(nameof(Index));

            foreach (var error in result.Errors)
                ModelState.AddModelError("", error.Description);

            ViewData["Controllers"] = GetControllers(AppConstants.Id);
            return View(viewModel);
        }

        // Delete: role/5
        [HttpDelete("role/{id}")]
        [DisplayName("Role Management: Delete Role Permissions")]
        public async Task<ActionResult> Delete(string id)
        {
            var role = await _roleManager.FindByIdAsync(id);
            if (role == null)
            {
                ModelState.AddModelError("Error", "Role not found");
                return BadRequest(ModelState);
            }

            var result = await _roleManager.DeleteAsync(role);
            if (result.Succeeded)
                return Ok(new { });

            foreach (var error in result.Errors)
                ModelState.AddModelError("Error", error.Description);

            return BadRequest(ModelState);
        }

        private IEnumerable<MvcControllerInfo> GetControllers(string AppId)
        {
            var accessJson = JsonConvert.DeserializeObject<IEnumerable<MvcControllerInfo>>(_context.AppRoles.FirstOrDefault(x => x.AppId == AppId).RoleAccess);
            return accessJson;
        }
        private void UpdateSuperAdmin(string SuperAdminRoles)
        {
            var RoleLists = JsonConvert.DeserializeObject<IEnumerable<MvcControllerInfo>>(SuperAdminRoles);
            List<MvcControllerInfoArea> MvcControllerInfoAreaList = new List<MvcControllerInfoArea>();

            var role = _context.Roles.ToList();
            foreach (var area in RoleLists.GroupBy(x => x.AreaName))
            {
                var ObjArea = new MvcControllerInfoArea();
                ObjArea.AreaName = area.FirstOrDefault().AreaName;
                List<MvcControllerInfoCont> MvcControllerInfoContList = new List<MvcControllerInfoCont>();
                foreach (var controller1 in area)
                {
                    var objcontroller = new MvcControllerInfoCont()
                    {
                        Id = controller1.Name,
                    };

                    List<MvcActionInfo> MvcActionInfo1List = new List<MvcActionInfo>();
                    foreach (var action in controller1.Actions)
                    {
                        var MvcActionInfo1 = new MvcActionInfo()
                        {
                            Name = action.Name,
                        };
                        MvcActionInfo1List.Add(MvcActionInfo1);
                    }
                    objcontroller.Actions = MvcActionInfo1List;
                    MvcControllerInfoContList.Add(objcontroller);

                }
                ObjArea.Controller = MvcControllerInfoContList;
                MvcControllerInfoAreaList.Add(ObjArea);
            }
            var accessJson = JsonConvert.SerializeObject(MvcControllerInfoAreaList);
            var RoleAppId = _context.Roles.FirstOrDefault(x => x.Name == "Super Administrator");
            RoleAppId.Access = accessJson;
            _context.Update(RoleAppId);
        }
    }
}