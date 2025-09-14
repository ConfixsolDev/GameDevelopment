using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using WargameBoard.Web.Common;

namespace WargameBoard.Web.Services
{
    public interface ISelectListService
    {
        Task<SelectList> GetSidesAsync();
        Task<SelectList> GetUnitTypesAsync();
        Task<SelectList> GetMovementProfilesAsync();
        Task<SelectList> GetTerrainTypesAsync();
        Task<SelectList> GetFortificationTypesAsync();
        Task<SelectList> GetObstacleTypesAsync();
        Task<SelectList> GetScenariosAsync();
        Task<SelectList> GetUnitsAsync();
        Task<SelectList> GetHexesAsync();
        Task<SelectList> GetTokenGroupsAsync();
        Task<SelectList> GetTokenDesignsAsync();
        Task<SelectList> GetBoardsAsync();
        Task<SelectList> GetTokenPiecesAsync();
        Task<SelectList> GetSessionsAsync();
    }

    public class SelectListService : ISelectListService
    {
        private readonly WargameBoard.Core.Data.WargameDbContext _context;
        public SelectListService(WargameBoard.Core.Data.WargameDbContext context)
        {
            _context = context;
        }
        // This would normally use repositories to get data from the database
        // For now, we'll return placeholder data

        public async Task<SelectList> GetSidesAsync()
        {
            var sides = await _context.Sides.ToListAsync();
            return new SelectList(sides, "Id", "Name");
        }

        public async Task<SelectList> GetUnitTypesAsync()
        {
            var unitTypes = await _context.UnitTypes.ToListAsync();
            return new SelectList(unitTypes, "Id", "Name");
        }

        public async Task<SelectList> GetMovementProfilesAsync()
        {
            var profiles = await _context.MovementProfiles.ToListAsync();
            return new SelectList(profiles, "Id", "Name");
        }

        public async Task<SelectList> GetTerrainTypesAsync()
        {
            var terrainTypes = await _context.TerrainTypes.ToListAsync();
            return new SelectList(terrainTypes, "Id", "Name");
        }

        public async Task<SelectList> GetFortificationTypesAsync()
        {
            var fortTypes = await _context.FortificationTypes.ToListAsync();
            return new SelectList(fortTypes, "Id", "Name");
        }

        public async Task<SelectList> GetObstacleTypesAsync()
        {
            var obstacleTypes = await _context.ObstacleTypes.ToListAsync();
            return new SelectList(obstacleTypes, "Id", "Name");
        }

        public async Task<SelectList> GetScenariosAsync()
        {
            var scenarios = await _context.Scenarios.ToListAsync();
            return new SelectList(scenarios, "Id", "Name");
        }

        public async Task<SelectList> GetUnitsAsync()
        {
            var units = await _context.Units.ToListAsync();
            return new SelectList(units, "Id", "Name");
        }

        public async Task<SelectList> GetHexesAsync()
        {
           List<BaseSelectList> selectList=new List<BaseSelectList>();
            var hexes = await _context.Hexes.ToListAsync();

            foreach (var item in hexes)
            {
                BaseSelectList obj = new BaseSelectList();
                obj.Id = item.Id;
                obj.Name = item.Q + "," + item.R;
                selectList.Add(obj);
            }
            return new SelectList(selectList, "Id", "Name");
        }

        public async Task<SelectList> GetTokenGroupsAsync()
        {
            var TokenGroups = await _context.TokenGroups.ToListAsync();
            return new SelectList(TokenGroups, "Id", "Name");
        }

        public async Task<SelectList> GetTokenDesignsAsync()
        {
            var designs = await _context.TokenDesigns.ToListAsync();
            return new SelectList(designs, "Id", "Name");
        }

        public async Task<SelectList> GetBoardsAsync()
        {
            var boards = await _context.Boards.ToListAsync();
            return new SelectList(boards, "Id", "Name");
        }

        public async Task<SelectList> GetTokenPiecesAsync()
        {
            var tokenPieces = await _context.TokenPieces
                .Include(tp => tp.TokenDesign)
                .Include(tp => tp.Side)
                .ToListAsync();
            return new SelectList(tokenPieces, "Id", "Serial");
        }

        public async Task<SelectList> GetSessionsAsync()
        {
            var sessions = await _context.Sessions
                .Include(s => s.Scenario)
                .Where(s => s.EndedAt == null)
                .ToListAsync();
            return new SelectList(sessions, "Id", "Scenario.Name");
        }
    }
}
