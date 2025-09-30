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

        public void UpdateSupplyState(UnitDeployment deployment, string newState)
        {
            deployment.SupplyState = newState;
            deployment.UpdateSupplyModifier();
        }

        public void UpdateSupplyState(UnitDeployment deployment, int newState)
        {
            deployment.SupplyStateInt = newState;
            deployment.SupplyState = newState == 100 ? "Green" : newState == 75 ? "Amber" : "Red";
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
            deployment.SupplyStateInt = Math.Max(50, deployment.SupplyStateInt - (int)degradationRate);
            deployment.SupplyState = deployment.SupplyStateInt == 100 ? "Green" : deployment.SupplyStateInt == 75 ? "Amber" : "Red";
            deployment.UpdateSupplyModifier();
        }

        public void RestoreSupply(MilitaryUnit unit, double restorationRate = 10.0)
        {
            unit.SupplyState = Math.Min(100, unit.SupplyState + (int)restorationRate);
            unit.UpdateSupplyModifier();
        }

        public void RestoreSupply(UnitDeployment deployment, double restorationRate = 10.0)
        {
            deployment.SupplyStateInt = Math.Min(100, deployment.SupplyStateInt + (int)restorationRate);
            deployment.SupplyState = deployment.SupplyStateInt == 100 ? "Green" : deployment.SupplyStateInt == 75 ? "Amber" : "Red";
            deployment.UpdateSupplyModifier();
        }
    }
}
