namespace TechWebSol.DTOs
{
    public class AssignUserToTeamRequest
    {
        public string UserId { get; set; } = string.Empty;
        public Guid? TeamId { get; set; }
    }
}
