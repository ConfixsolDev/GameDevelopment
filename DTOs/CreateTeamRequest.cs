namespace TechWebSol.DTOs
{
    public class CreateTeamRequest
    {
        public Guid? id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string TeamCode { get; set; } = string.Empty;
        public Guid? TeamTypeId { get; set; }
        public string SubTeamCode { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string TeamCategory { get; set; } = string.Empty;
        public string ForceType { get; set; } = string.Empty;
    }
}
