namespace WargameBoard.Core.Services
{
    public interface ITokenAssignmentService
    {
        Task AssignTokenAsync(string tokenId, string unitId);
    }

    public interface IPlacementService
    {
        Task PlaceUnitAsync(string unitId, int x, int y);
    }

    public interface IObjectiveService
    {
        Task CreateObjectiveAsync(string name, string description);
    }
}
