using System.ComponentModel.DataAnnotations;

namespace WargameBoard.Web.Models.ViewModels
{
    public class PlacementsOverlayVm
    {
        public List<HexChipVm> HexChips { get; set; } = new List<HexChipVm>();
    }

    public class HexChipVm
    {
        public int HexId { get; set; }
        public List<TokenChipVm> Chips { get; set; } = new List<TokenChipVm>();
    }

    public class TokenChipVm
    {
        public int TokenPieceId { get; set; }
        public string Short { get; set; } = string.Empty;
        public string Color { get; set; } = "#6c757d";
        public string FullName { get; set; } = string.Empty;
        public int? SideId { get; set; }
    }
}
