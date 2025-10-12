using System;
using System.Collections.Generic;

namespace TechWebSol.ViewModels
{
    /// <summary>
    /// ViewModel for attack order selection modal
    /// </summary>
    public class AttackOrderSelectionViewModel
    {
        public Guid Id { get; set; }
        public Guid AttackerTokenId { get; set; }
        public Guid TargetTokenId { get; set; }
        public string AttackerTokenName { get; set; } = string.Empty;
        public string TargetTokenName { get; set; } = string.Empty;
        public bool IsSuspectedToken { get; set; }
        public string Status { get; set; } = string.Empty;
        public string CompletionPercentage { get; set; } = string.Empty;
        public DateTime? LastUpdated { get; set; }
    }

    /// <summary>
    /// ViewModel for combat simulation results modal
    /// </summary>
    public class CombatSimulationResultsViewModel
    {
        public string AttackerTokenName { get; set; } = string.Empty;
        public string DefenderTokenName { get; set; } = string.Empty;
        public bool WasSuspectedToken { get; set; }
        public double DetectionConfidence { get; set; }
        public DateTime SimulationTime { get; set; }
        
        public List<AttackPhaseViewModel> AttackPhases { get; set; } = new List<AttackPhaseViewModel>();
        public List<DefensePhaseViewModel> DefensePhases { get; set; } = new List<DefensePhaseViewModel>();
        
        public AttackSummaryViewModel AttackSummary { get; set; } = new AttackSummaryViewModel();
        public DefenseSummaryViewModel DefenseSummary { get; set; } = new DefenseSummaryViewModel();
    }

    /// <summary>
    /// ViewModel for attack phase
    /// </summary>
    public class AttackPhaseViewModel
    {
        public string PhaseName { get; set; } = string.Empty;
        public string PhaseType { get; set; } = string.Empty;
        public string? Location { get; set; }
        public int DelayMinutes { get; set; }
        public int CasualtiesAttacker { get; set; }
        public int CasualtiesDefender { get; set; }
        public string? Notes { get; set; }
    }

    /// <summary>
    /// ViewModel for defense phase
    /// </summary>
    public class DefensePhaseViewModel
    {
        public string PhaseName { get; set; } = string.Empty;
        public string PhaseType { get; set; } = string.Empty;
        public int TimeToStayMinutes { get; set; }
        public int MovementDelayMinutes { get; set; }
        public int CounterAttackDelayMinutes { get; set; }
        public int CasualtiesDefender { get; set; }
        public int CasualtiesAttacker { get; set; }
        public string? Notes { get; set; }
    }

    /// <summary>
    /// ViewModel for attack summary
    /// </summary>
    public class AttackSummaryViewModel
    {
        public string EngagementKillZoneSummary { get; set; } = string.Empty;
        public string DefensePositionsSummary { get; set; } = string.Empty;
        public int TotalDelayMinutes { get; set; }
        public int TotalAttackerCasualties { get; set; }
        public int TotalDefenderCasualties { get; set; }
    }

    /// <summary>
    /// ViewModel for defense summary
    /// </summary>
    public class DefenseSummaryViewModel
    {
        public string TimeToStaySummary { get; set; } = string.Empty;
        public string CounterPenetrationMovementSummary { get; set; } = string.Empty;
        public string CounterAttackSummary { get; set; } = string.Empty;
        public int TotalTimeMinutes { get; set; }
        public int TotalDefenderCasualties { get; set; }
        public int TotalAttackerCasualties { get; set; }
    }
}

