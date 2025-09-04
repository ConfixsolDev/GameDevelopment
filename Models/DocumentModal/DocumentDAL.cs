
using TechWebSol.Data;

namespace TechWebSol.Models.DocumentModal
{
    public class DocumentDAL
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;
        public DocumentDAL(ApplicationDbContext context, IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
        }

       
    }
}
