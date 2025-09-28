//using Microsoft.VisualStudio.TestTools.UnitTesting;
//using TechWebSol.Models;
//using TechWebSol.Services;

//namespace TechWebSol.Tests
//{
//    [TestClass]
//    public class MovementServiceTests
//    {
//        private MovementService _movementService;

//        [TestInitialize]
//        public void Setup()
//        {
//            _movementService = new MovementService();
//        }

//        [TestMethod]
//        public void Movement_UnitOnRoad_Moves30KmPerTurn()
//        {
//            // Arrange
//            var unit = new InfantryBattalion 
//            { 
//                MovementPoints = 30, 
//                CurrentTerrain = "Road",
//                SupplyState = 100
//            };

//            // Act
//            var result = _movementService.CalculateMovementCost("Road", 30, unit.SupplyState);

//            // Assert
//            Assert.AreEqual(30, result);
//        }

//        [TestMethod]
//        public void Movement_UnitInForest_Moves15KmPerTurn()
//        {
//            // Arrange
//            var unit = new InfantryBattalion 
//            { 
//                MovementPoints = 30, 
//                CurrentTerrain = "Forest",
//                SupplyState = 100
//            };

//            // Act
//            var result = _movementService.CalculateMovementCost("Forest", 30, unit.SupplyState);

//            // Assert
//            Assert.AreEqual(15, result); // 30 * 0.5 = 15
//        }

//        [TestMethod]
//        public void Movement_UnitInMountain_Moves9KmPerTurn()
//        {
//            // Arrange
//            var unit = new InfantryBattalion 
//            { 
//                MovementPoints = 30, 
//                CurrentTerrain = "Mountain",
//                SupplyState = 100
//            };

//            // Act
//            var result = _movementService.CalculateMovementCost("Mountain", 30, unit.SupplyState);

//            // Assert
//            Assert.AreEqual(9, result); // 30 * 0.3 = 9
//        }

//        [TestMethod]
//        public void Movement_RedSupplyState_ReducesMovementBy50Percent()
//        {
//            // Arrange
//            var unit = new InfantryBattalion 
//            { 
//                MovementPoints = 30, 
//                CurrentTerrain = "Road",
//                SupplyState = 50 // Red supply
//            };

//            // Act
//            var result = _movementService.CalculateMovementCost("Road", 30, unit.SupplyState);

//            // Assert
//            Assert.AreEqual(15, result); // 30 * 1.0 * 0.5 = 15
//        }

//        [TestMethod]
//        public void Movement_AmberSupplyState_ReducesMovementBy25Percent()
//        {
//            // Arrange
//            var unit = new InfantryBattalion 
//            { 
//                MovementPoints = 30, 
//                CurrentTerrain = "Road",
//                SupplyState = 75 // Amber supply
//            };

//            // Act
//            var result = _movementService.CalculateMovementCost("Road", 30, unit.SupplyState);

//            // Assert
//            Assert.AreEqual(22.5, result); // 30 * 1.0 * 0.75 = 22.5
//        }

//        [TestMethod]
//        public void CanMove_UnitHasEnoughMovement_ReturnsTrue()
//        {
//            // Arrange
//            var unit = new InfantryBattalion 
//            { 
//                MovementPoints = 30,
//                RemainingMovement = 20,
//                SupplyState = 100
//            };

//            // Act
//            var result = _movementService.CanMove(unit, 15, "Road");

//            // Assert
//            Assert.IsTrue(result);
//        }

//        [TestMethod]
//        public void CanMove_UnitInsufficientMovement_ReturnsFalse()
//        {
//            // Arrange
//            var unit = new InfantryBattalion 
//            { 
//                MovementPoints = 30,
//                RemainingMovement = 5,
//                SupplyState = 100
//            };

//            // Act
//            var result = _movementService.CanMove(unit, 15, "Road");

//            // Assert
//            Assert.IsFalse(result);
//        }
//    }
//}
