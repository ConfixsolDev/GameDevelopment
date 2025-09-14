using Microsoft.AspNetCore.Mvc;
using WargameBoard.Core.Entities;
using WargameBoard.Web.Models;
using WargameBoard.Web.Models.ViewModels;
using WargameBoard.Web.Services;
using Microsoft.EntityFrameworkCore;
using WargameBoard.Core.Data;

namespace WargameBoard.Web.Areas.Tokens.Controllers
{
    [Area("Tokens")]
    public class TokensController : Controller
    {
        private readonly WargameDbContext _context;
        private readonly ISelectListService _selectListService;

        public TokensController(WargameDbContext context, ISelectListService selectListService)
        {
            _context = context;
            _selectListService = selectListService;
        }

        // GET: Tokens/Designs
        public async Task<IActionResult> Designs()
        {
            var tokenDesigns = await _context.TokenDesigns
                .Include(td => td.TokenGroup)
                .Include(td => td.DefaultSide)
                .OrderBy(td => td.Name)
                .ToListAsync();

            return View(tokenDesigns);
        }

        // GET: Tokens/CreateDesign
        public async Task<IActionResult> CreateDesign()
        {
            var viewModel = new TokenDesignEditVm
            {
                TokenGroups = await _selectListService.GetTokenGroupsAsync(),
                Sides = await _selectListService.GetSidesAsync()
            };

            return PartialView("_TokenDesignForm", viewModel);
        }

        // POST: Tokens/CreateDesign
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateDesign([Bind("TokenGroupId,Name,DefaultSideId,WidthMm,HeightMm,Notes")] TokenDesignEditVm viewModel)
        {
            if (ModelState.IsValid)
            {
                var tokenDesign = new TokenDesign
                {
                    TokenGroupId = viewModel.TokenGroupId,
                    Name = viewModel.Name,
                    DefaultSideId = viewModel.DefaultSideId,
                    WidthMm = viewModel.WidthMm,
                    HeightMm = viewModel.HeightMm,
                    Notes = viewModel.Notes
                };

                _context.Add(tokenDesign);
                await _context.SaveChangesAsync();
                return Json(new { success = true, id = tokenDesign.Id });
            }

            viewModel.TokenGroups = await _selectListService.GetTokenGroupsAsync();
            viewModel.Sides = await _selectListService.GetSidesAsync();
            return PartialView("_TokenDesignForm", viewModel);
        }

        // GET: Tokens/EditDesign/5
        public async Task<IActionResult> EditDesign(int id)
        {
            var tokenDesign = await _context.TokenDesigns.FindAsync(id);
            if (tokenDesign == null)
            {
                return NotFound();
            }

            var viewModel = new TokenDesignEditVm
            {
                Id = tokenDesign.Id,
                TokenGroupId = tokenDesign.TokenGroupId,
                Name = tokenDesign.Name,
                DefaultSideId = tokenDesign.DefaultSideId,
                WidthMm = tokenDesign.WidthMm,
                HeightMm = tokenDesign.HeightMm,
                Notes = tokenDesign.Notes,
                TokenGroups = await _selectListService.GetTokenGroupsAsync(),
                Sides = await _selectListService.GetSidesAsync()
            };

            return PartialView("_TokenDesignForm", viewModel);
        }

        // POST: Tokens/EditDesign/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditDesign(int id, [Bind("Id,TokenGroupId,Name,DefaultSideId,WidthMm,HeightMm,Notes")] TokenDesignEditVm viewModel)
        {
            if (id != viewModel.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var tokenDesign = await _context.TokenDesigns.FindAsync(id);
                    if (tokenDesign == null)
                    {
                        return NotFound();
                    }

                    tokenDesign.TokenGroupId = viewModel.TokenGroupId;
                    tokenDesign.Name = viewModel.Name;
                    tokenDesign.DefaultSideId = viewModel.DefaultSideId;
                    tokenDesign.WidthMm = viewModel.WidthMm;
                    tokenDesign.HeightMm = viewModel.HeightMm;
                    tokenDesign.Notes = viewModel.Notes;

                    _context.Update(tokenDesign);
                    await _context.SaveChangesAsync();
                    return Json(new { success = true });
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!TokenDesignExists(viewModel.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
            }

            viewModel.TokenGroups = await _selectListService.GetTokenGroupsAsync();
            viewModel.Sides = await _selectListService.GetSidesAsync();
            return PartialView("_TokenDesignForm", viewModel);
        }

        // GET: Tokens/Pieces/5
        public async Task<IActionResult> Pieces(int designId)
        {
            var tokenDesign = await _context.TokenDesigns.FindAsync(designId);
            if (tokenDesign == null)
            {
                return NotFound();
            }

            var tokenPieces = await _context.TokenPieces
                .Where(tp => tp.TokenDesignId == designId)
                .Include(tp => tp.Side)
                .OrderBy(tp => tp.Serial)
                .ToListAsync();

            ViewBag.DesignId = designId;
            ViewBag.DesignName = tokenDesign.Name;
            return PartialView("_TokenPiecesList", tokenPieces);
        }

        // GET: Tokens/CreatePiece/5
        public async Task<IActionResult> CreatePiece(int designId)
        {
            var viewModel = new TokenPieceEditVm
            {
                TokenDesignId = designId,
                Sides = await _selectListService.GetSidesAsync()
            };

            return PartialView("_TokenPieceForm", viewModel);
        }

        // POST: Tokens/CreatePiece
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreatePiece([Bind("TokenDesignId,SideId,Serial,HardwareIdentity,IsUnique")] TokenPieceEditVm viewModel)
        {
            // Validate HardwareIdentity uniqueness if provided
            if (!string.IsNullOrEmpty(viewModel.HardwareIdentity))
            {
                var existingPiece = await _context.TokenPieces
                    .FirstOrDefaultAsync(tp => tp.HardwareIdentity == viewModel.HardwareIdentity);
                
                if (existingPiece != null)
                {
                    ModelState.AddModelError(nameof(viewModel.HardwareIdentity), "Hardware Identity must be unique.");
                }
            }

            if (ModelState.IsValid)
            {
                var tokenPiece = new TokenPiece
                {
                    TokenDesignId = viewModel.TokenDesignId,
                    SideId = viewModel.SideId,
                    Serial = viewModel.Serial,
                    HardwareIdentity = viewModel.HardwareIdentity,
                    IsUnique = viewModel.IsUnique
                };

                _context.Add(tokenPiece);
                await _context.SaveChangesAsync();
                return Json(new { success = true, id = tokenPiece.Id });
            }

            viewModel.Sides = await _selectListService.GetSidesAsync();
            return PartialView("_TokenPieceForm", viewModel);
        }

        // GET: Tokens/EditPiece/5
        public async Task<IActionResult> EditPiece(int id)
        {
            var tokenPiece = await _context.TokenPieces.FindAsync(id);
            if (tokenPiece == null)
            {
                return NotFound();
            }

            var viewModel = new TokenPieceEditVm
            {
                Id = tokenPiece.Id,
                TokenDesignId = tokenPiece.TokenDesignId,
                SideId = tokenPiece.SideId,
                Serial = tokenPiece.Serial,
                HardwareIdentity = tokenPiece.HardwareIdentity,
                IsUnique = tokenPiece.IsUnique,
                Sides = await _selectListService.GetSidesAsync()
            };

            return PartialView("_TokenPieceForm", viewModel);
        }

        // POST: Tokens/EditPiece/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditPiece(int id, [Bind("Id,TokenDesignId,SideId,Serial,HardwareIdentity,IsUnique")] TokenPieceEditVm viewModel)
        {
            if (id != viewModel.Id)
            {
                return NotFound();
            }

            // Validate HardwareIdentity uniqueness if provided
            if (!string.IsNullOrEmpty(viewModel.HardwareIdentity))
            {
                var existingPiece = await _context.TokenPieces
                    .FirstOrDefaultAsync(tp => tp.HardwareIdentity == viewModel.HardwareIdentity && tp.Id != viewModel.Id);
                
                if (existingPiece != null)
                {
                    ModelState.AddModelError(nameof(viewModel.HardwareIdentity), "Hardware Identity must be unique.");
                }
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var tokenPiece = await _context.TokenPieces.FindAsync(id);
                    if (tokenPiece == null)
                    {
                        return NotFound();
                    }

                    tokenPiece.TokenDesignId = viewModel.TokenDesignId;
                    tokenPiece.SideId = viewModel.SideId;
                    tokenPiece.Serial = viewModel.Serial;
                    tokenPiece.HardwareIdentity = viewModel.HardwareIdentity;
                    tokenPiece.IsUnique = viewModel.IsUnique;

                    _context.Update(tokenPiece);
                    await _context.SaveChangesAsync();
                    return Json(new { success = true });
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!TokenPieceExists(viewModel.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
            }

            viewModel.Sides = await _selectListService.GetSidesAsync();
            return PartialView("_TokenPieceForm", viewModel);
        }

        // POST: Tokens/DeleteDesign/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteDesign(int id)
        {
            var tokenDesign = await _context.TokenDesigns.FindAsync(id);
            if (tokenDesign != null)
            {
                _context.TokenDesigns.Remove(tokenDesign);
                await _context.SaveChangesAsync();
                return Json(new { success = true });
            }
            return Json(new { success = false, message = "Design not found" });
        }

        // POST: Tokens/DeletePiece/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeletePiece(int id)
        {
            var tokenPiece = await _context.TokenPieces.FindAsync(id);
            if (tokenPiece != null)
            {
                _context.TokenPieces.Remove(tokenPiece);
                await _context.SaveChangesAsync();
                return Json(new { success = true });
            }
            return Json(new { success = false, message = "Piece not found" });
        }

        private bool TokenDesignExists(int id)
        {
            return _context.TokenDesigns.Any(e => e.Id == id);
        }

        private bool TokenPieceExists(int id)
        {
            return _context.TokenPieces.Any(e => e.Id == id);
        }
    }
}
