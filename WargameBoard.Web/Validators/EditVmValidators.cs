using FluentValidation;
using WargameBoard.Web.Models.ViewModels;
using WargameBoard.Core.Entities; // enums

namespace WargameBoard.Web.Validators
{
    public class ScenarioEditVmValidator : AbstractValidator<ScenarioEditVm>
    {
        public ScenarioEditVmValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Scenario name is required")
                .MaximumLength(100).WithMessage("Scenario name cannot exceed 100 characters");

            RuleFor(x => x.TurnLengthMinutes)
                .GreaterThan(0)
                .LessThanOrEqualTo(480);

            RuleFor(x => x.MaxTurns)
                .GreaterThan(0)
                .LessThanOrEqualTo(100);

            RuleFor(x => x.Weather)
                .IsInEnum().WithMessage("Invalid weather selection");

            RuleFor(x => x.Notes)
                .MaximumLength(1000)
                .When(x => !string.IsNullOrWhiteSpace(x.Notes));
        }
    }

    public class ScenarioUnitEditVmValidator : AbstractValidator<ScenarioUnitEditVm>
    {
        public ScenarioUnitEditVmValidator()
        {
            RuleFor(x => x.ScenarioId).GreaterThan(0);
            RuleFor(x => x.UnitId).GreaterThan(0);
            RuleFor(x => x.StartHexId).GreaterThan(0);

            RuleFor(x => x.Steps)
                .GreaterThan(0)
                .LessThanOrEqualTo(10);

            RuleFor(x => x.Posture)
                      .Must(value => Enum.IsDefined(typeof(Posture), value))
                .WithMessage("Invalid posture");
        }
    }


    public class ScenarioObjectiveEditVmValidator : AbstractValidator<ScenarioObjectiveEditVm>
    {
        public ScenarioObjectiveEditVmValidator()
        {
            RuleFor(x => x.ScenarioId).GreaterThan(0);
            RuleFor(x => x.HexId).GreaterThan(0);
            RuleFor(x => x.SideId).GreaterThan(0);

            RuleFor(x => x.VictoryPoints)
                .GreaterThan(0)
                .LessThanOrEqualTo(100);

            // Validate ConditionKind is a defined enum value
            RuleFor(x => x.ConditionKind)
                .Must(value => Enum.IsDefined(typeof(VictoryConditionKind), value))
                .WithMessage("Invalid condition kind");

            // Apply TurnThreshold rules only when ConditionKind == SeizeByTurn
            When(
                x => Enum.IsDefined(typeof(VictoryConditionKind), x.ConditionKind) &&
                     (VictoryConditionKind)x.ConditionKind == VictoryConditionKind.SeizeByTurn,
                () =>
                {
                    RuleFor(x => x.TurnThreshold)
                        .NotNull().WithMessage("Turn threshold is required for 'Seize by Turn'")
                        .GreaterThan(0)
                        .LessThanOrEqualTo(100);
                }
            );
        }
    }

    public class TokenDesignEditVmValidator : AbstractValidator<TokenDesignEditVm>
    {
        public TokenDesignEditVmValidator()
        {
            RuleFor(x => x.TokenGroupId).GreaterThan(0);

            RuleFor(x => x.Name)
                .NotEmpty()
                .MaximumLength(100);

            RuleFor(x => x.WidthMm)
                .GreaterThan(0)
                .LessThanOrEqualTo(100);

            RuleFor(x => x.HeightMm)
                .GreaterThan(0)
                .LessThanOrEqualTo(100);

            RuleFor(x => x.Notes)
                .MaximumLength(500)
                .When(x => !string.IsNullOrWhiteSpace(x.Notes));
        }
    }

    public class TokenPieceEditVmValidator : AbstractValidator<TokenPieceEditVm>
    {
        public TokenPieceEditVmValidator()
        {
            RuleFor(x => x.TokenDesignId).GreaterThan(0);

            RuleFor(x => x.Serial)
                .MaximumLength(50)
                .When(x => !string.IsNullOrWhiteSpace(x.Serial));

            RuleFor(x => x.HardwareIdentity)
                .MaximumLength(100)
                .When(x => !string.IsNullOrWhiteSpace(x.HardwareIdentity));
        }
    }

    public class HexEditVmValidator : AbstractValidator<HexEditVm>
    {
        public HexEditVmValidator()
        {
            RuleFor(x => x.Q).InclusiveBetween(-10, 10);
            RuleFor(x => x.R).InclusiveBetween(-10, 10);
            RuleFor(x => x.TerrainTypeId).GreaterThan(0);

            RuleFor(x => x.KeyFeature)
                .MaximumLength(100)
                .When(x => !string.IsNullOrWhiteSpace(x.KeyFeature));
        }
    }

    public class HexFeatureEditVmValidator : AbstractValidator<HexFeatureEditVm>
    {
        public HexFeatureEditVmValidator()
        {
            RuleFor(x => x.HexId).GreaterThan(0);

            RuleFor(x => x.FeatureKind)
                .IsInEnum().WithMessage("Invalid feature kind");

            When(x => x.FeatureKind == FeatureKind.Fort, () =>
            {
                RuleFor(x => x.FortificationTypeId)
                    .NotNull().WithMessage("Fortification type is required")
                    .GreaterThan(0);
            });

            When(x => x.FeatureKind == FeatureKind.Obstacle, () =>
            {
                RuleFor(x => x.ObstacleTypeId)
                    .NotNull().WithMessage("Obstacle type is required")
                    .GreaterThan(0);
            });
        }
    }

    public class UnitEditVmValidator : AbstractValidator<UnitEditVm>
    {
        public UnitEditVmValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty()
                .MaximumLength(100);

            RuleFor(x => x.SideId).GreaterThan(0);
            RuleFor(x => x.UnitTypeId).GreaterThan(0);

            // If you made these nullable in the VM, keep the .When; otherwise remove it.
            RuleFor(x => x.Personnel)
                .GreaterThanOrEqualTo(0)
                .When(x => x.Personnel.HasValue);

            RuleFor(x => x.VehiclesPrimary)
                .GreaterThanOrEqualTo(0)
                .When(x => x.VehiclesPrimary.HasValue);

            RuleFor(x => x.Quality)
                .IsInEnum().WithMessage("Invalid quality level");

            RuleFor(x => x.Cohesion)
                .InclusiveBetween(0, 100);

            // If optional, keep When; if required, drop When and just use GreaterThan(0)
            RuleFor(x => x.MovementProfileId)
                .GreaterThan(0)
                .When(x => x.MovementProfileId.HasValue);
        }
    }

    public class BoardCellEditVmValidator : AbstractValidator<BoardCellEditVm>
    {
        public BoardCellEditVmValidator()
        {
            RuleFor(x => x.BoardId).GreaterThan(0);

            RuleFor(x => x.Row)
                .GreaterThanOrEqualTo(0)
                .LessThanOrEqualTo(20);

            RuleFor(x => x.Col)
                .GreaterThanOrEqualTo(0)
                .LessThanOrEqualTo(20);

            RuleFor(x => x.SensorAddress)
                .MaximumLength(50)
                .When(x => !string.IsNullOrWhiteSpace(x.SensorAddress));

            // Threshold is double? now
            RuleFor(x => x.Threshold)
                .InclusiveBetween(0, 5)
                .When(x => x.Threshold.HasValue);
        }
    }
}
