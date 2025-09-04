using Microsoft.AspNetCore.Mvc.Rendering;

namespace TechWebSol.ViewModels.PersonalInfoVM
{
    public class PersonalInfoIndexVM
    {
        public PersonalInfoIndexVM()
        {
            DepartmentsList = new List<SelectListItem>();
            DesignationsList = new List<SelectListItem>();
            EmploymentTypeId = new List<SelectListItem>();
        }
        public string FullName { get; set; }
        public string CNIC { get; set; }
        public string Phone { get; set; }
        public string ServiceNumber { get; set; }
        public IEnumerable<SelectListItem> DepartmentsList { get; set; }
        public IEnumerable<SelectListItem> DesignationsList { get; set; }
        public IEnumerable<SelectListItem> EmploymentTypeId { get; set; }
        public IEnumerable<SelectListItem> EmploymentCategoryId { get; set; }
    }
}
