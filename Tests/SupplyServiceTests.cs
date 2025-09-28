//using Microsoft.VisualStudio.TestTools.UnitTesting;
//using TechWebSol.Models;
//using TechWebSol.Services;

//namespace TechWebSol.Tests
//{
//    [TestClass]
//    public class SupplyServiceTests
//    {
//        private SupplyService _supplyService;

//        [TestInitialize]
//        public void Setup()
//        {
//            _supplyService = new SupplyService();
//        }

//        [TestMethod]
//        public void Supply_RedState_ReducesEffectiveness()
//        {
//            // Arrange
//            var unit = new InfantryBattalion { SupplyState = 50 }; // Red supply

//            // Act
//            unit.UpdateSupplyModifier();

//            // Assert
//            Assert.AreEqual(0.5, unit.SupplyModifier);
//        }

//        [TestMethod]
//        public void Supply_AmberState_ReducesEffectiveness()
//        {
//            // Arrange
//            var unit = new InfantryBattalion { SupplyState = 75 }; // Amber supply

//            // Act
//            unit.UpdateSupplyModifier();

//            // Assert
//            Assert.AreEqual(0.75, unit.SupplyModifier);
//        }

//        [TestMethod]
//        public void Supply_GreenState_FullEffectiveness()
//        {
//            // Arrange
//            var unit = new InfantryBattalion { SupplyState = 100 }; // Green supply

//            // Act
//            unit.UpdateSupplyModifier();

//            // Assert
//            Assert.AreEqual(1.0, unit.SupplyModifier);
//        }

//        [TestMethod]
//        public void Supply_UpdateSupplyState_ChangesModifier()
//        {
//            // Arrange
//            var unit = new InfantryBattalion { SupplyState = 100 };

//            // Act
//            _supplyService.UpdateSupplyState(unit, 50);

//            // Assert
//            Assert.AreEqual(50, unit.SupplyState);
//            Assert.AreEqual(0.5, unit.SupplyModifier);
//        }

//        [TestMethod]
//        public void Supply_DegradeSupply_ReducesSupplyState()
//        {
//            // Arrange
//            var unit = new InfantryBattalion { SupplyState = 100 };

//            // Act
//            _supplyService.DegradeSupply(unit, 10.0);

//            // Assert
//            Assert.AreEqual(90, unit.SupplyState);
//        }

//        [TestMethod]
//        public void Supply_DegradeSupply_DoesNotGoBelow50()
//        {
//            // Arrange
//            var unit = new InfantryBattalion { SupplyState = 60 };

//            // Act
//            _supplyService.DegradeSupply(unit, 20.0);

//            // Assert
//            Assert.AreEqual(50, unit.SupplyState); // Should not go below 50
//        }

//        [TestMethod]
//        public void Supply_RestoreSupply_IncreasesSupplyState()
//        {
//            // Arrange
//            var unit = new InfantryBattalion { SupplyState = 70 };

//            // Act
//            _supplyService.RestoreSupply(unit, 15.0);

//            // Assert
//            Assert.AreEqual(85, unit.SupplyState);
//        }

//        [TestMethod]
//        public void Supply_RestoreSupply_DoesNotGoAbove100()
//        {
//            // Arrange
//            var unit = new InfantryBattalion { SupplyState = 95 };

//            // Act
//            _supplyService.RestoreSupply(unit, 20.0);

//            // Assert
//            Assert.AreEqual(100, unit.SupplyState); // Should not go above 100
//        }

//        [TestMethod]
//        public void Supply_IsSupplyCritical_ReturnsTrueForRed()
//        {
//            // Arrange
//            var unit = new InfantryBattalion { SupplyState = 50 };

//            // Act
//            var result = _supplyService.IsSupplyCritical(unit.SupplyState);

//            // Assert
//            Assert.IsTrue(result);
//        }

//        [TestMethod]
//        public void Supply_IsSupplyCritical_ReturnsFalseForGreen()
//        {
//            // Arrange
//            var unit = new InfantryBattalion { SupplyState = 100 };

//            // Act
//            var result = _supplyService.IsSupplyCritical(unit.SupplyState);

//            // Assert
//            Assert.IsFalse(result);
//        }
//    }
//}
