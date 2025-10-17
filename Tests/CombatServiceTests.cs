//using Microsoft.VisualStudio.TestTools.UnitTesting;
//using TechWebSol.Models;
//using TechWebSol.Services;

//namespace TechWebSol.Tests
//{
//    [TestClass]
//    public class CombatServiceTests
//    {
//        private CombatService _combatService;
//        private MovementService _movementService;

//        [TestInitialize]
//        public void Setup()
//        {
//            _movementService = new MovementService();
//            _combatService = new CombatService(_movementService);
//        }

//        [TestMethod]
//        public void Combat_2xCombatPower_Inflicts2xCasualties()
//        {
//            // Arrange
//            var attacker = new InfantryBattalion 
//            { 
//                CombatPower = 2.0, 
//                StrengthPercentage = 100,
//                SupplyState = 100,
//                UnitType = "Infantry"
//            };
//            var defender = new InfantryBattalion 
//            { 
//                CombatPower = 1.0, 
//                StrengthPercentage = 100,
//                SupplyState = 100,
//                UnitType = "Infantry"
//            };

//            // Act
//            var result = _combatService.ResolveCombat(attacker, defender, "Plain");

//            // Assert
//            Assert.IsTrue(result.AttackerCasualties < result.DefenderCasualties);
//            Assert.IsTrue(result.AttackerEffectiveness > result.DefenderEffectiveness);
//        }

//        [TestMethod]
//        public void Combat_AfterCasualties_StrengthDecreases()
//        {
//            // Arrange
//            var attacker = new InfantryBattalion 
//            { 
//                CombatPower = 1.0, 
//                StrengthPercentage = 100,
//                SupplyState = 100,
//                UnitType = "Infantry"
//            };
//            var defender = new InfantryBattalion 
//            { 
//                CombatPower = 1.0, 
//                StrengthPercentage = 100,
//                SupplyState = 100,
//                UnitType = "Infantry"
//            };

//            var initialAttackerStrength = attacker.StrengthPercentage;
//            var initialDefenderStrength = defender.StrengthPercentage;

//            // Act
//            var result = _combatService.ResolveCombat(attacker, defender, "Plain");

//            // Assert
//            Assert.IsTrue(attacker.StrengthPercentage < initialAttackerStrength);
//            Assert.IsTrue(defender.StrengthPercentage < initialDefenderStrength);
//        }

//        [TestMethod]
//        public void Combat_RedSupplyState_ReducesEffectiveness()
//        {
//            // Arrange
//            var attacker = new InfantryBattalion 
//            { 
//                CombatPower = 1.0, 
//                StrengthPercentage = 100,
//                SupplyState = 50, // Red supply
//                UnitType = "Infantry"
//            };
//            var defender = new InfantryBattalion 
//            { 
//                CombatPower = 1.0, 
//                StrengthPercentage = 100,
//                SupplyState = 100, // Green supply
//                UnitType = "Infantry"
//            };

//            // Act
//            var result = _combatService.ResolveCombat(attacker, defender, "Plain");

//            // Assert
//            Assert.IsTrue(result.DefenderEffectiveness > result.AttackerEffectiveness);
//        }

//        [TestMethod]
//        public void Combat_InfantryInForest_HasAdvantage()
//        {
//            // Arrange
//            var infantry = new InfantryBattalion 
//            { 
//                CombatPower = 1.0, 
//                StrengthPercentage = 100,
//                SupplyState = 100,
//                UnitType = "Infantry"
//            };
//            var armour = new ArmouredRegiment 
//            { 
//                CombatPower = 1.0, 
//                StrengthPercentage = 100,
//                SupplyState = 100,
//                UnitType = "Armoured"
//            };

//            // Act
//            var result = _combatService.ResolveCombat(infantry, armour, "Forest");

//            // Assert
//            Assert.IsTrue(result.AttackerEffectiveness > result.DefenderEffectiveness);
//        }

//        [TestMethod]
//        public void Combat_StrengthBelow50Percent_DegradesCombatPower()
//        {
//            // Arrange
//            var unit = new InfantryBattalion 
//            { 
//                CombatPower = 1.0, 
//                StrengthPercentage = 30, // Below 50%
//                SupplyState = 100,
//                UnitType = "Infantry"
//            };

//            // Act
//            var effectiveCombatPower = unit.GetEffectiveCombatPower();

//            // Assert
//            Assert.IsTrue(effectiveCombatPower < unit.CombatPower);
//        }
//    }
//}
