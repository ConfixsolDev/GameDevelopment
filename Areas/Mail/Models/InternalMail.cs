using TechWebSol.Areas.Identity;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using TechWebSol.Models;

namespace TechWebSol.Areas.Mail.Models
{
    public class MailEntity : BaseEntity
    {
        [MaxLength(256)] public string URL { get; set; }
        public virtual WorkFlowDefination WorkFlowDefination { get; set; }
        public Guid? WorkFlowDefinationId { get; set; }
    }

    public class MailMessage : BaseEntity
    {
        public virtual MailEntity MailEntity { get; set; }
        public Guid? MailEntityId { get; set; }
        [DisplayName("Comment")]
        [MaxLength(256)] public string Message { get; set; }
        public string SentTo { get; set; }
        [MaxLength(100)] public string Status { get; set; }
        public DateTime ReadTime { get; set; }
        public virtual WorkFlowStep WorkFlowStep { get; set; }
        public Guid? WorkFlowStepId { get; set; }
        public bool IsComplete { get; set; } = false;

        [NotMapped]
        public List<WorkFlowStep> WorkFlowSteps { get; set; } = new List<WorkFlowStep>();
    }

    public class MailMessageDTO 
    {
        public Guid Id{ get; set; }
        [MaxLength(256)] public string Message { get; set; }
        public string SentTo { get; set; }
        public string SentToName { get; set; }
        public string Status { get; set; }
        [DisplayName("Current Step Id")]
        public Guid? WorkFlowStepId { get; set; }
        public WorkFlowStep WorkFlowStep { get; set; }

        [DisplayName("Next Step Id")]
        public Guid? NextStepId { get; set; }
        public string WorkFlow{ get; set; }
        public string WorkFlowStepName{ get; set; }
        public string CreatedBy { get; set; }
        public DateTime? CreatedDate { get; set; }
        public string CreatedByName { get; set; }
        public DateTime ReadTime { get; set; }
        public virtual MailEntity MailEntity { get; set; }
        public Guid? MailEntityId { get; set; }
        public bool IsComplete { get; set; } = false;
        public bool IsCurrentStep { get; set; } = false;
        public List<WorkFlowStep> WorkFlowSteps { get; set; } = new List<WorkFlowStep>();
    }

    public class WorkFlowDefination : BaseEntity
    {
        [MaxLength(100)] public string Name { get; set; }
        public bool IsActive { get; set; }
        public List<WorkFlowStep> WorkFlowStep { get; set; } = new List<WorkFlowStep>();

    }

    public class WorkFlowDefinationDTO
    {
        public Guid id { get; set; }
        public string name { get; set; }
        public bool isActive { get; set; }
        public DateTime? dt { get; set; }
        public string date { get; set; }
    }
    public class WorkFlowStep : BaseEntity
    {
        [MaxLength(128)]
        [DisplayName("Route")]
        public string Name { get; set; }
        [DisplayName("Action Required")]
        public string ActionRequired { get; set; }
        public Guid? WorkFlowDefinationId { get; set; }
        public virtual WorkFlowDefination WorkFlowDefination { get; set; }
        [DisplayName("Steps Sequence")]
        public int StepSequence { get; set; }
        public virtual ApplicationUser ApplicationUserApp { get; set; }
        [DisplayName("User")]
        public string ApplicationUserAppID { get; set; }

        [NotMapped]
        public string FullName { get; set; }

        [NotMapped]
        public string Designation { get; set; }

        [NotMapped]
        public bool CurrentStep { get; set; } = false;
    }
    public class ColumnSortOrderVM
    {
        public Guid Id { get; set; }
        public string Action { get; set; } = string.Empty;
        public Guid FileId { get; set; }

    }

}
