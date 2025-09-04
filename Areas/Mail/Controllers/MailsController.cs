using TechWebSol.DAL;
using TechWebSol.Extensions;
using TechWebSol.Areas.Mail.Models;
using TechWebSol.Controllers;
using TechWebSol.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using MailMessage = TechWebSol.Areas.Mail.Models.MailMessage;
using Microsoft.AspNetCore.Authorization;
using System.ComponentModel;
using TechWebSol.ViewModels;
using TechWebSol.Services;
using TechWebSol.Constants;

namespace TechWebSol.Areas.InternalMail.Controllers
{
    [Authorize]
    [Area("Mail")]
    [DisplayName("Mails: Mail Controller (User)")]
    public class MailsController : Controller
    {

        private readonly ILogger _logger;
        private readonly ApplicationUserVM applicatonUser;
        private readonly ApplicationDbContext _context;
        private readonly IUserService _IUserService;

        public MailsController(ApplicationDbContext context,
            ILogger<HomeController> logger,
            IUserSessionService IUserSessionService,
            IUserService IUserService
            )
        {
            _logger = logger;
            _IUserService = IUserService;
            applicatonUser = IUserSessionService.GetCurrentUser();
            _context = context;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        [DisplayName("Mails: assigned to every one")]
        public async Task<IActionResult> InBox()
        {
            //var totalRecords = await _context.MailMessage.Where(x=>x.SentTo == applicatonUser.ApplicationUserId).CountAsync();
            var Querable = _context.MailMessage.Include(x => x.MailEntity)
                                                  .Include(x => x.MailEntity).ThenInclude(x => x.WorkFlowDefination)
                                                  .Include(x => x.WorkFlowStep)
                                                  .OrderByDescending(x => x.WorkFlowStep.StepSequence)
                                                  .Where(x => x.SentTo == applicatonUser.ApplicationUserId && x.IsComplete == false)
                                                  .Select(x => new MailMessageDTO
                                                  {
                                                      Id = x.Id,
                                                      Message = x.Message,
                                                      SentTo = x.SentTo,
                                                      IsComplete = x.IsComplete,
                                                      Status = x.Status,
                                                      CreatedBy = x.CreatedBy,
                                                      CreatedDate = x.CreatedDate,
                                                      WorkFlow = x.MailEntity.WorkFlowDefination.Name,
                                                      WorkFlowStepId = x.WorkFlowStepId,
                                                      WorkFlowStepName = x.WorkFlowStep.Name,
                                                      MailEntity = x.MailEntity,
                                                  }).ToList();

            //Use this function to Display User Name Anywhere.
            var FetchData = await _IUserService.GetAllUser();
            foreach (var item in Querable)
            {
                item.CreatedByName = FetchData.FirstOrDefault(x => x.Id == item.CreatedBy).UserName;
                item.SentToName = FetchData.FirstOrDefault(x => x.Id == item.SentTo).UserName;
            }
            return PartialView(Querable);
        }

        [HttpPost]
        [DisplayName("Mails: assigned to every one")]
        public async Task<IActionResult> SentBox()
        {
            //var totalRecords = await _context.MailMessage.Where(x=>x.SentTo == applicatonUser.ApplicationUserId).CountAsync();
            var Querable = _context.MailMessage.Include(x => x.MailEntity)
                                                       .Include(x => x.MailEntity).ThenInclude(x => x.WorkFlowDefination)
                                                       .Include(x => x.WorkFlowStep)
                                                       .OrderByDescending(x => x.WorkFlowStep.StepSequence)
                                                       .Where(x => x.SentTo == applicatonUser.ApplicationUserId || x.CreatedBy == applicatonUser.ApplicationUserId && x.IsComplete == true)
                                                       .Select(x => new MailMessageDTO
                                                       {
                                                           Id = x.Id,
                                                           Message = x.Message,
                                                           SentTo = x.SentTo,
                                                           Status = x.Status,
                                                           CreatedBy = x.CreatedBy,
                                                           CreatedDate = x.CreatedDate,
                                                           WorkFlow = x.MailEntity.WorkFlowDefination.Name,
                                                           WorkFlowStepId = x.WorkFlowStepId,
                                                           MailEntity = x.MailEntity,
                                                           IsComplete = x.IsComplete,
                                                       }).ToList();

            //Use this function to Display User Name Anywhere.
            var FetchData = await _IUserService.GetAllUser();
            foreach (var item in Querable)
            {
                item.CreatedByName = FetchData.FirstOrDefault(x => x.Id == item.CreatedBy).UserName;
                item.SentToName = FetchData.FirstOrDefault(x => x.Id == item.SentTo).UserName;
            }
            return PartialView("InBox",Querable);
        }

        [DisplayName("Mails: assigned to every one")]
        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null || _context.Mail == null)
            {
                return NotFound();
            }

            var mail = await _context.Mail
                .Include(m => m.WorkFlowDefination)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (mail == null)
            {
                return NotFound();
            }

            return View(mail);
        }

        [HttpPost]
        [DisplayName("Mails: assigned to every one")]
        public async Task<IActionResult> Forward(MailMessage mail, Guid? NextStepId, Guid? MailEntityId)
        {
            if (ModelState.IsValid)
            {
                var CurrentStep = _context.MailMessage.OrderBy(x => x.CreatedDate).FirstOrDefault(x => x.Id == mail.Id || (x.WorkFlowStepId == NextStepId && x.MailEntityId == MailEntityId));
                if (CurrentStep != null)
                {
                    CurrentStep.Message = mail.Message;
                    CurrentStep.Status = mail.Status;
                    CurrentStep.IsComplete = true;
                    _context.Update(CurrentStep);
                    await _context.SaveChangesAsync();
                }
                else
                {
                    mail.MailEntityId = MailEntityId;
                    mail.WorkFlowStepId = NextStepId;
                    mail.IsComplete = true;
                    _context.Add(mail);
                    await _context.SaveChangesAsync();
                }
                return RedirectToAction(nameof(Index));
            }
            return View(mail);
        }

        [DisplayName("Mails: assigned to every one")]
        public async Task<IActionResult> Create(Guid Id, string key, Guid? WDI)
        {
            if (WDI == null || WDI == Guid.Empty)
            {
                WDI = Guid.Parse("528b4c43-296d-4547-b213-10fc65133842");
            }

            MailEntity mailEntity = new MailEntity();
            MailMessage mailMessage;
            List<WorkFlowStep> WorkFlowSteps = new List<WorkFlowStep>();

            mailMessage = _context.MailMessage.Include(x => x.MailEntity).FirstOrDefault(x => x.MailEntity.URL.Contains(Id.ToString()) && x.CreatedBy == applicatonUser.ApplicationUserId);
            ViewBag.WDI = WDI;
            if (WDI != null && WDI != Guid.Empty)
            {
                WorkFlowSteps = _context.WorkFlowStep.Where(x => x.WorkFlowDefinationId == WDI).OrderBy(x => x.StepSequence).ToList();
            }

            if (mailMessage != null)
            {
                mailEntity = mailMessage.MailEntity;
            }
            else
            {
                var URLDEtails = AppConstants.MailSettings.FirstOrDefault(x => x.Key == key);
                mailEntity.URL = URLDEtails.Value.ToString() + Id;

                if (WDI == Guid.Empty || WDI == null)
                {
                    mailEntity.WorkFlowDefinationId = _context.WorkFlowDefination.FirstOrDefault().Id;
                }
                else
                {
                    mailEntity.WorkFlowDefinationId = WDI;
                }
            }

            if (WorkFlowSteps.Count() > 0)
            {
                //var mailmessage = _context.MailMessage.Include(x => x.WorkFlowStep).Where(x => x.WorkFlowStep.WorkFlowDefinationId == WDI).OrderBy(x => /*x.WorkFlowStep.StepSequence).ToList();*/

                if (WorkFlowSteps != null)
                {
                    var FetchData = await _IUserService.GetAllUser();
                    foreach (var item in WorkFlowSteps)
                    {
                        item.FullName = FetchData.FirstOrDefault(x => x.Id == item.ApplicationUserAppID).UserName;
                        item.Designation = FetchData.FirstOrDefault(x => x.Id == item.ApplicationUserAppID).Designation;
                        if (item.ApplicationUserAppID == applicatonUser.ApplicationUserId)
                        {
                            item.CurrentStep = true;
                        }
                    }
                    if (mailMessage == null)
                    {
                        mailMessage = new MailMessage();
                    }

                    mailMessage.WorkFlowStep = WorkFlowSteps.FirstOrDefault();
                    mailMessage.WorkFlowStepId = WorkFlowSteps.FirstOrDefault()?.Id;
                    mailMessage.SentTo = WorkFlowSteps.FirstOrDefault()?.ApplicationUserAppID;
                    mailMessage.Status = WorkFlowSteps.FirstOrDefault()?.Name;
                    mailMessage.WorkFlowSteps = WorkFlowSteps;
                    mailMessage.MailEntity = mailEntity;
                    return PartialView(mailMessage);
                }
            }
            else
            {
                mailMessage = new MailMessage();
                mailMessage.WorkFlowStep = null;
                mailMessage.WorkFlowStepId = null;
                mailMessage.Status = "Letral Sharing";
                mailMessage.MailEntity = mailEntity;

                var FetchData = await _IUserService.GetUserSelectList();
                ViewData["AppUserId"] = new SelectList(FetchData, "Value", "Text");
                return PartialView(mailMessage);
            }
            return PartialView();
        }

        [DisplayName("Mails: assigned to every one")]
        public async Task<IActionResult> GetWorkFlowPartials(Guid Id)
        {
            //if (WDI == null || key == null || Id == Guid.Empty)
            //{
            //    return Problem("Please edit and select Workflow.");
            //}

            var MailMessageDTOs = _context.MailMessage.Include(x => x.MailEntity)
                                                              .Include(x => x.WorkFlowStep)
                                                              .Include(x => x.MailEntity).ThenInclude(x => x.WorkFlowDefination).ThenInclude(x => x.WorkFlowStep)
                                                              .OrderByDescending(x => x.WorkFlowStep.StepSequence)
                                                              .Where(x => x.MailEntity.URL.Contains(Id.ToString()))
                                                              .Select(x => new MailMessageDTO
                                                              {
                                                                  Id = x.Id,
                                                                  Message = x.Message,
                                                                  SentTo = x.SentTo,
                                                                  Status = x.Status,
                                                                  CreatedBy = x.CreatedBy,
                                                                  CreatedDate = x.CreatedDate,
                                                                  WorkFlow = x.WorkFlowStep.WorkFlowDefination.Name,
                                                                  WorkFlowStepId = x.WorkFlowStepId,
                                                                  WorkFlowSteps = x.MailEntity.WorkFlowDefination.WorkFlowStep,
                                                                  MailEntityId = x.MailEntityId,
                                                              }).ToList();

            //Use this function to Display User Name Anywhere.
            var FetchData = await _IUserService.GetAllUser();
            foreach (var item in MailMessageDTOs)
            {
                item.CreatedByName = FetchData.FirstOrDefault(x => x.Id == item.CreatedBy)?.UserName;
                item.SentToName = FetchData.FirstOrDefault(x => x.Id == item.SentTo)?.UserName;
                if (item.SentTo == applicatonUser.ApplicationUserId)
                {
                    item.IsCurrentStep = true;
                    var CurrentStep = item.WorkFlowSteps.FirstOrDefault(x => x.Id == item.WorkFlowStepId);
                    if (CurrentStep != null)
                    {
                        item.NextStepId = item.WorkFlowSteps.OrderBy(x => x.StepSequence).FirstOrDefault(x => x.StepSequence > CurrentStep.StepSequence)?.Id;
                    }
                }
            }
            foreach (var item in MailMessageDTOs.FirstOrDefault().WorkFlowSteps)
            {
                item.FullName = FetchData.FirstOrDefault(x => x.Id == item.ApplicationUserAppID).UserName;
                item.Designation = FetchData.FirstOrDefault(x => x.Id == item.ApplicationUserAppID).Designation;
                if (item.ApplicationUserAppID == applicatonUser.ApplicationUserId)
                {
                    item.CurrentStep = true;
                }
            }
            return PartialView(MailMessageDTOs);
        }

        [HttpPost]
        [DisplayName("Mails: assigned to every one")]
        public async Task<IActionResult> Create(MailMessage mail, string WorkFlowDefinationId, string URL, string SentTo)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelStateExtensions.GetModalErrors(ModelState).FirstOrDefault().Value);
            }

            try
            {
                MailEntity mailEntity = new MailEntity();
                if (mail.MailEntityId == null)
                {
                    mailEntity.URL = URL;
                    mailEntity.WorkFlowDefinationId = Guid.Parse(WorkFlowDefinationId);
                    _context.Add(mailEntity);
                    await _context.SaveChangesAsync();
                }

                if (mail.Id != Guid.Empty && mail.MailEntityId != Guid.Empty)
                {
                    _context.Update(mail);
                }
                else
                {
                    mail.MailEntityId = mailEntity.Id;
                    _context.Add(mail);
                }
                await _context.SaveChangesAsync();
                return Ok("lateral sharing successfully");
            }
            catch (Exception ex)
            {
                return Problem(ex.Message);
            }
        }

        [DisplayName("Mails: assigned to every one")]
        public async Task<IActionResult> Edit(Guid? id)
        {
            if (id == null || _context.Mail == null)
            {
                return NotFound();
            }

            var mail = await _context.Mail.FindAsync(id);
            if (mail == null)
            {
                return NotFound();
            }
            ViewData["WorkFlowDefinationId"] = new SelectList(_context.WorkFlowDefination, "Id", "Id", mail.WorkFlowDefinationId);
            return View(mail);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, [Bind("URL,WorkFlowDefinationId,Id,CreatedBy,CreatedDate,UpdatedBy,UpdatedDate,IsDelete")] MailEntity mail)
        {
            if (id != mail.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(mail);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["WorkFlowDefinationId"] = new SelectList(_context.WorkFlowDefination, "Id", "Id", mail.WorkFlowDefinationId);
            return View(mail);
        }
    }
}
