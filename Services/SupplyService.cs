using TechWebSol.Models;

namespace TechWebSol.Services
{
    public class SupplyService
    {
        public void UpdateSupplyState(MilitaryUnit unit, int newState)
        {
            unit.SupplyState = newState;
            unit.UpdateSupplyModifier();
        }

        public void UpdateSupplyState(UnitDeployment deployment, int newState)
        {
            deployment.SupplyState = newState;
            deployment.UpdateSupplyModifier();
        }

        public double GetMovementModifier(int supplyState)
        {
            return supplyState switch
            {
                100 => 1.0,    // Green
                75 => 0.75,    // Amber
                50 => 0.5,     // Red
                _ => 1.0
            };
        }

        public double GetCombatModifier(int supplyState)
        {
            return supplyState switch
            {
                100 => 1.0,    // Green
                75 => 0.75,    // Amber
                50 => 0.5,     // Red
                _ => 1.0
            };
        }

        public string GetSupplyStateDescription(int supplyState)
        {
            return supplyState switch
            {
                100 => "Green - Full Supply",
                75 => "Amber - Reduced Supply",
                50 => "Red - Critical Supply",
                _ => "Unknown"
            };
        }

        public bool IsSupplyCritical(int supplyState)
        {
            return supplyState <= 50;
        }

        public void DegradeSupply(MilitaryUnit unit, double degradationRate = 5.0)
        {
            unit.SupplyState = Math.Max(50, unit.SupplyState - (int)degradationRate);
            unit.UpdateSupplyModifier();
        }

        public void DegradeSupply(UnitDeployment deployment, double degradationRate = 5.0)
        {
            deployment.SupplyState = Math.Max(50, deployment.SupplyState - (int)degradationRate);
            deployment.UpdateSupplyModifier();
        }

        public void RestoreSupply(MilitaryUnit unit, double restorationRate = 10.0)
        {
            unit.SupplyState = Math.Min(100, unit.SupplyState + (int)restorationRate);
            unit.UpdateSupplyModifier();
        }

        public void RestoreSupply(UnitDeployment deployment, double restorationRate = 10.0)
        {
            deployment.SupplyState = Math.Min(100, deployment.SupplyState + (int)restorationRate);
            deployment.UpdateSupplyModifier();
        }
    }
}
