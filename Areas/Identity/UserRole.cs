using System.ComponentModel.DataAnnotations;
using TechWebSol.Models;

namespace TechWebSol.Areas.Identity
{
    public class ApplicationUserApp : BaseEntity
    {
        //This is must be same as ApplicatoionUserId

        [MaxLength(256)]
        public string Email { get; set; }
        [MaxLength(256)]
        public string UserName { get; set; }
        [MaxLength(256)]
        public string Designation { get; set; }
        [MaxLength(256)]
        public string Department { get; set; }

        [MaxLength(256)]
        public string HomeUrl { get; set; }
    }
}
