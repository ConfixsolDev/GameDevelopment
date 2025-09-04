using TechWebSol.Data;
using TechWebSol.Areas.Mail.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace TechWebSol.Areas.Mail.Controllers
{
    [Area("Mail")]
    public class MailMessagesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public MailMessagesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: InternalMail/MailMessages
        public async Task<IActionResult> Index()
        {
            var webApplication3Context = _context.MailMessage.Include(m => m.WorkFlowStep);
            return View(await webApplication3Context.ToListAsync());
        }

        // GET: InternalMail/MailMessages/Details/5
        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null || _context.MailMessage == null)
            {
                return NotFound();
            }

            var mailMessage = await _context.MailMessage
                .Include(m => m.WorkFlowStep)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (mailMessage == null)
            {
                return NotFound();
            }

            return View(mailMessage);
        }

        // GET: InternalMail/MailMessages/Create
        public IActionResult Create()
        {
            ViewData["WorkFlowStepId"] = new SelectList(_context.WorkFlowStep, "Id", "Id");
            return View();
        }

        // POST: InternalMail/MailMessages/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Message,SentTo,Status,ReadTime,WorkFlowStepId,Id,CreatedBy,CreatedDate,UpdatedBy,UpdatedDate,IsDelete")] MailMessage mailMessage)
        {
            if (ModelState.IsValid)
            {
                mailMessage.Id = Guid.NewGuid();
                _context.Add(mailMessage);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["WorkFlowStepId"] = new SelectList(_context.WorkFlowStep, "Id", "Id", mailMessage.WorkFlowStepId);
            return View(mailMessage);
        }

        // GET: InternalMail/MailMessages/Edit/5
        public async Task<IActionResult> Edit(Guid? id)
        {
            if (id == null || _context.MailMessage == null)
            {
                return NotFound();
            }

            var mailMessage = await _context.MailMessage.FindAsync(id);
            if (mailMessage == null)
            {
                return NotFound();
            }
            ViewData["WorkFlowStepId"] = new SelectList(_context.WorkFlowStep, "Id", "Id", mailMessage.WorkFlowStepId);
            return View(mailMessage);
        }

        // POST: InternalMail/MailMessages/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, [Bind("Message,SentTo,Status,ReadTime,WorkFlowStepId,Id,CreatedBy,CreatedDate,UpdatedBy,UpdatedDate,IsDelete")] MailMessage mailMessage)
        {
            if (id != mailMessage.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(mailMessage);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!MailMessageExists(mailMessage.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["WorkFlowStepId"] = new SelectList(_context.WorkFlowStep, "Id", "Id", mailMessage.WorkFlowStepId);
            return View(mailMessage);
        }

        // GET: InternalMail/MailMessages/Delete/5
        public async Task<IActionResult> Delete(Guid? id)
        {
            if (id == null || _context.MailMessage == null)
            {
                return NotFound();
            }

            var mailMessage = await _context.MailMessage
                .Include(m => m.WorkFlowStep)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (mailMessage == null)
            {
                return NotFound();
            }

            return View(mailMessage);
        }

        // POST: InternalMail/MailMessages/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            if (_context.MailMessage == null)
            {
                return Problem("Entity set 'WebApplication3Context.MailMessage'  is null.");
            }
            var mailMessage = await _context.MailMessage.FindAsync(id);
            if (mailMessage != null)
            {
                _context.MailMessage.Remove(mailMessage);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool MailMessageExists(Guid id)
        {
            return (_context.MailMessage?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}
