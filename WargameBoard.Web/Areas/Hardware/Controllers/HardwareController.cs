using Microsoft.AspNetCore.Mvc;
using WargameBoard.Core.Entities;
using WargameBoard.Web.Models;
using WargameBoard.Web.Services;
using Microsoft.EntityFrameworkCore;
using WargameBoard.Core.Data;

namespace WargameBoard.Web.Areas.Hardware.Controllers
{
    [Area("Hardware")]
    public class HardwareController : Controller
    {
        private readonly WargameDbContext _context;
        private readonly ISelectListService _selectListService;

        public HardwareController(WargameDbContext context, ISelectListService selectListService)
        {
            _context = context;
            _selectListService = selectListService;
        }

        // GET: Hardware/Diagnostics
        public async Task<IActionResult> Diagnostics()
        {
            var boardCells = await _context.BoardCells
                .Include(bc => bc.Hex)
                .OrderBy(bc => bc.Row)
                .ThenBy(bc => bc.Col)
                .ToListAsync();

            return View(boardCells);
        }

        // GET: Hardware/Diagnostics/Current
        public async Task<IActionResult> Current()
        {
            var boardCells = await _context.BoardCells
                .Include(bc => bc.Hex)
                .OrderBy(bc => bc.Row)
                .ThenBy(bc => bc.Col)
                .ToListAsync();

            // Simulate live strength updates (in real implementation, this would come from hardware)
            var random = new Random();
            foreach (var cell in boardCells)
            {
                if (!string.IsNullOrEmpty(cell.SensorAddress))
                {
                    cell.LastStrength = random.Next(0, 100);
                    cell.LastStrengthUpdate = DateTime.UtcNow;
                }
            }

            await _context.SaveChangesAsync();

            return PartialView("_DiagnosticsTable", boardCells);
        }

        // GET: Hardware/MapBoardCells
        public async Task<IActionResult> MapBoardCells(int page = 1, int pageSize = 25)
        {
            var query = _context.BoardCells
                .Include(bc => bc.Hex)
                .AsQueryable();

            var totalItems = await query.CountAsync();
            var items = await query
                .OrderBy(bc => bc.Row)
                .ThenBy(bc => bc.Col)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var pagingInfo = new PagingInfo
            {
                CurrentPage = page,
                PageSize = pageSize,
                TotalItems = totalItems
            };

            ViewBag.PagingInfo = pagingInfo;
            ViewBag.Hexes = await _selectListService.GetHexesAsync();

            return View(items);
        }

        // POST: Hardware/UpdateBoardCellHex
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateBoardCellHex(int id, int? hexId)
        {
            var boardCell = await _context.BoardCells.FindAsync(id);
            if (boardCell == null)
            {
                return Json(new { success = false, message = "Board cell not found" });
            }

            boardCell.HexId = hexId;
            _context.Update(boardCell);
            await _context.SaveChangesAsync();

            return Json(new { success = true, hexId = hexId });
        }

        // POST: Hardware/UpdateBoardCellThreshold
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateBoardCellThreshold(int id, int? threshold)
        {
            var boardCell = await _context.BoardCells.FindAsync(id);
            if (boardCell == null)
            {
                return Json(new { success = false, message = "Board cell not found" });
            }

            boardCell.Threshold = threshold;
            _context.Update(boardCell);
            await _context.SaveChangesAsync();

            return Json(new { success = true, threshold = threshold });
        }

        // POST: Hardware/UpdateBoardCellSensorAddress
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateBoardCellSensorAddress(int id, string? sensorAddress)
        {
            var boardCell = await _context.BoardCells.FindAsync(id);
            if (boardCell == null)
            {
                return Json(new { success = false, message = "Board cell not found" });
            }

            boardCell.SensorAddress = sensorAddress;
            _context.Update(boardCell);
            await _context.SaveChangesAsync();

            return Json(new { success = true, sensorAddress = sensorAddress });
        }

        // GET: Hardware/CreateBoardCell
        public async Task<IActionResult> CreateBoardCell()
        {
            ViewBag.Hexes = await _selectListService.GetHexesAsync();
            return PartialView("_BoardCellForm", new BoardCell());
        }

        // POST: Hardware/CreateBoardCell
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateBoardCell([Bind("BoardId,Row,Col,SensorAddress,HexId,Threshold")] BoardCell boardCell)
        {
            if (ModelState.IsValid)
            {
                _context.Add(boardCell);
                await _context.SaveChangesAsync();
                return Json(new { success = true, id = boardCell.Id });
            }

            ViewBag.Hexes = await _selectListService.GetHexesAsync();
            return PartialView("_BoardCellForm", boardCell);
        }

        // POST: Hardware/DeleteBoardCell
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteBoardCell(int id)
        {
            var boardCell = await _context.BoardCells.FindAsync(id);
            if (boardCell != null)
            {
                _context.BoardCells.Remove(boardCell);
                await _context.SaveChangesAsync();
                return Json(new { success = true });
            }
            return Json(new { success = false, message = "Board cell not found" });
        }
    }
}
