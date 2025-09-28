using TechWebSol.Models;
using TechWebSol.Data;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System;


namespace TechWebSol.DAL
{
    public interface IUserService
    {
        Task<Models.ApplicationUser> GetUserById(Guid id);
        Task<List<Models.ApplicationUser>> GetAllActiveUser();
        Task<List<Models.ApplicationUser>> GetAllUser();
        Task<List<SelectListItem>> GetUserSelectList();
    }

    public class UserService : IUserService
    {
        private readonly ApplicationDbContext _context;
        public UserService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Models.ApplicationUser> GetUserById(Guid id)
        {
            return await _context.Users.FirstOrDefaultAsync(x => x.Id == id.ToString());
        }

        public async Task<List<Models.ApplicationUser>> GetAllActiveUser()
        {
            return await _context.Users.Where(x => !x.IsDeleted).ToListAsync();
        }

        public async Task<List<Models.ApplicationUser>> GetAllUser()
        {
            return await _context.Users.ToListAsync();
        }

        public async Task<List<SelectListItem>> GetUserSelectList()
        {
            var userList = await _context.Users.Where(x=>x.IsDeleted == false)
                                       .Select(u => new SelectListItem
                                       {
                                           Value = u.Id.ToString(),
                                           Text = $"{u.UserCode} | {u.TeamId}"
                                       })
                                       .ToListAsync();
            return userList;
        }
    }
}
