using AutoMapper;
using WargameBoard.Core.Entities;
using WargameBoard.Web.Models.ViewModels;

namespace WargameBoard.Web.Mapping
{
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {
            // Unit mappings
            CreateMap<Unit, UnitEditVm>()
                .ReverseMap();

            CreateMap<UnitCapability, UnitCapabilityEditVm>()
                .ReverseMap();

            // Hex mappings
            CreateMap<Hex, HexEditVm>()
                .ReverseMap();

            CreateMap<HexFeature, HexFeatureEditVm>()
                .ReverseMap();

            // Scenario mappings
            CreateMap<Scenario, ScenarioEditVm>()
                .ReverseMap();

            CreateMap<ScenarioUnit, ScenarioUnitEditVm>()
                .ReverseMap();

            CreateMap<ScenarioObjective, ScenarioObjectiveEditVm>()
                .ReverseMap();

            // Token mappings
            CreateMap<TokenDesign, TokenDesignEditVm>()
                .ReverseMap();

            CreateMap<TokenPiece, TokenPieceEditVm>()
                .ReverseMap();

            // Session mappings
            CreateMap<Session, SessionCreateVm>()
                .ReverseMap()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => DateTime.UtcNow))
                .ForMember(dest => dest.StartedAt, opt => opt.Ignore())
                .ForMember(dest => dest.EndedAt, opt => opt.Ignore());

            // Board mappings
            CreateMap<BoardCell, BoardCellEditVm>()
                .ReverseMap();
        }
    }
}
