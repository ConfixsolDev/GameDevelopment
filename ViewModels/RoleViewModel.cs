using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace TechWebSol.ViewModels
{
    public class RoleViewModel
    {
        [Required]
        [StringLength(256, ErrorMessage = "The {0} must be at least {2} characters long.")]
        public string Name { get; set; }
        public string AppId { get; set; }

        public List<MvcControllerInfo> SelectedControllers { get; set; }
        public List<MvcControllerInfo> MvcControllerInfo { get; set; }
        public List<MvcControllerInfoCont> MvcControllerInfoCont { get; set; }
    }

    public class MvcControllerInfoArea
    {
        public string AreaName { get; set; }
        public List<MvcControllerInfoCont> Controller { get; set; }
    }

    public class MvcControllerInfoCont
    {
        public string Id { get; set; }
        public List<MvcActionInfo> Actions { get; set; }
    }
    public class MvcActionInfo
    {
        public string Id => $"{ControllerId}:{Name}";

        public string Name { get; set; }

        public string DisplayName { get; set; }

        public string ControllerId { get; set; }
    }

    public class MvcControllerInfo
    {
        public string Id => $"{AreaName}:{Name}";

        public string Name { get; set; }

        public string DisplayName { get; set; }

        public string AreaName { get; set; }

        public IEnumerable<MvcActionInfo> Actions { get; set; }
    }

}

