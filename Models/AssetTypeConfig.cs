using System.ComponentModel.DataAnnotations;

namespace TechWebSol.Models
{
    public class AssetTypeConfig
    {
        public decimal DefaultRadiusKm { get; set; }
        public string DefaultShape { get; set; } = "Circle"; // "Circle", "Polygon"
        public string DefaultCoverageType { get; set; } = "Operational";
        public string Description { get; set; } = string.Empty;
    }

    public static class AssetTypeDefaults
    {
        public static readonly Dictionary<string, AssetTypeConfig> AssetTypeConfigs = new()
        {
            // AIRCRAFT
            { "Fighter", new AssetTypeConfig { DefaultRadiusKm = 100, DefaultShape = "Circle", DefaultCoverageType = "Combat", Description = "Fighter Aircraft" } },
            { "Bomber", new AssetTypeConfig { DefaultRadiusKm = 200, DefaultShape = "Circle", DefaultCoverageType = "Strike", Description = "Bomber Aircraft" } },
            { "Transport", new AssetTypeConfig { DefaultRadiusKm = 50, DefaultShape = "Circle", DefaultCoverageType = "Support", Description = "Transport Aircraft" } },
            { "Reconnaissance", new AssetTypeConfig { DefaultRadiusKm = 150, DefaultShape = "Circle", DefaultCoverageType = "Reconnaissance", Description = "Reconnaissance Aircraft" } },
            { "Helicopter", new AssetTypeConfig { DefaultRadiusKm = 25, DefaultShape = "Circle", DefaultCoverageType = "Patrol", Description = "Helicopter" } },
            { "Drone", new AssetTypeConfig { DefaultRadiusKm = 15, DefaultShape = "Circle", DefaultCoverageType = "Surveillance", Description = "Unmanned Aerial Vehicle" } },
            
            // GROUND VEHICLES
            { "Tank", new AssetTypeConfig { DefaultRadiusKm = 8, DefaultShape = "Circle", DefaultCoverageType = "Combat", Description = "Main Battle Tank" } },
            { "APC", new AssetTypeConfig { DefaultRadiusKm = 5, DefaultShape = "Circle", DefaultCoverageType = "Transport", Description = "Armored Personnel Carrier" } },
            { "IFV", new AssetTypeConfig { DefaultRadiusKm = 6, DefaultShape = "Circle", DefaultCoverageType = "Combat", Description = "Infantry Fighting Vehicle" } },
            { "Truck", new AssetTypeConfig { DefaultRadiusKm = 3, DefaultShape = "Circle", DefaultCoverageType = "Supply", Description = "Military Truck" } },
            { "Humvee", new AssetTypeConfig { DefaultRadiusKm = 4, DefaultShape = "Circle", DefaultCoverageType = "Patrol", Description = "High Mobility Vehicle" } },
            { "Artillery", new AssetTypeConfig { DefaultRadiusKm = 30, DefaultShape = "Circle", DefaultCoverageType = "Fire Support", Description = "Artillery Piece" } },
            { "Mortar", new AssetTypeConfig { DefaultRadiusKm = 8, DefaultShape = "Circle", DefaultCoverageType = "Fire Support", Description = "Mortar System" } },
            
            // NAVAL
            { "Destroyer", new AssetTypeConfig { DefaultRadiusKm = 80, DefaultShape = "Circle", DefaultCoverageType = "Naval", Description = "Naval Destroyer" } },
            { "Frigate", new AssetTypeConfig { DefaultRadiusKm = 60, DefaultShape = "Circle", DefaultCoverageType = "Naval", Description = "Naval Frigate" } },
            { "Submarine", new AssetTypeConfig { DefaultRadiusKm = 40, DefaultShape = "Circle", DefaultCoverageType = "Submarine", Description = "Submarine" } },
            { "Patrol Boat", new AssetTypeConfig { DefaultRadiusKm = 20, DefaultShape = "Circle", DefaultCoverageType = "Patrol", Description = "Patrol Boat" } },
            { "Landing Craft", new AssetTypeConfig { DefaultRadiusKm = 10, DefaultShape = "Circle", DefaultCoverageType = "Amphibious", Description = "Landing Craft" } },
            
            // PERSONNEL
            { "Infantry", new AssetTypeConfig { DefaultRadiusKm = 2, DefaultShape = "Circle", DefaultCoverageType = "Combat", Description = "Infantry Unit" } },
            { "Special Forces", new AssetTypeConfig { DefaultRadiusKm = 1, DefaultShape = "Circle", DefaultCoverageType = "Special Operations", Description = "Special Forces Unit" } },
            { "Reconnaissance Team", new AssetTypeConfig { DefaultRadiusKm = 3, DefaultShape = "Circle", DefaultCoverageType = "Reconnaissance", Description = "Reconnaissance Unit" } },
            { "Engineer", new AssetTypeConfig { DefaultRadiusKm = 2, DefaultShape = "Circle", DefaultCoverageType = "Engineering", Description = "Engineering Unit" } },
            { "Medical", new AssetTypeConfig { DefaultRadiusKm = 1, DefaultShape = "Circle", DefaultCoverageType = "Medical", Description = "Medical Unit" } },
            
            // EQUIPMENT/SYSTEMS
            { "Radar", new AssetTypeConfig { DefaultRadiusKm = 50, DefaultShape = "Circle", DefaultCoverageType = "Detection", Description = "Radar System" } },
            { "Communication", new AssetTypeConfig { DefaultRadiusKm = 25, DefaultShape = "Circle", DefaultCoverageType = "Communication", Description = "Communication Equipment" } },
            { "Sensor", new AssetTypeConfig { DefaultRadiusKm = 10, DefaultShape = "Circle", DefaultCoverageType = "Surveillance", Description = "Surveillance Sensor" } },
            { "Missile Defense", new AssetTypeConfig { DefaultRadiusKm = 40, DefaultShape = "Circle", DefaultCoverageType = "Defense", Description = "Missile Defense System" } },
            { "Command Post", new AssetTypeConfig { DefaultRadiusKm = 15, DefaultShape = "Polygon", DefaultCoverageType = "Command", Description = "Command Post" } },
            { "Supply Depot", new AssetTypeConfig { DefaultRadiusKm = 5, DefaultShape = "Polygon", DefaultCoverageType = "Supply", Description = "Supply Depot" } },
            
            // FORMATIONS
            { "Brigade", new AssetTypeConfig { DefaultRadiusKm = 20, DefaultShape = "Polygon", DefaultCoverageType = "Operational", Description = "Military Brigade" } },
            { "Battalion", new AssetTypeConfig { DefaultRadiusKm = 10, DefaultShape = "Polygon", DefaultCoverageType = "Operational", Description = "Military Battalion" } },
            { "Company", new AssetTypeConfig { DefaultRadiusKm = 5, DefaultShape = "Polygon", DefaultCoverageType = "Tactical", Description = "Military Company" } },
            { "Squad", new AssetTypeConfig { DefaultRadiusKm = 2, DefaultShape = "Circle", DefaultCoverageType = "Tactical", Description = "Military Squad" } },
            
            // SUPPORT
            { "Fuel Truck", new AssetTypeConfig { DefaultRadiusKm = 2, DefaultShape = "Circle", DefaultCoverageType = "Supply", Description = "Fuel Supply Vehicle" } },
            { "Ambulance", new AssetTypeConfig { DefaultRadiusKm = 1, DefaultShape = "Circle", DefaultCoverageType = "Medical", Description = "Medical Vehicle" } },
            { "Repair Vehicle", new AssetTypeConfig { DefaultRadiusKm = 3, DefaultShape = "Circle", DefaultCoverageType = "Maintenance", Description = "Maintenance Vehicle" } },
            { "Crane", new AssetTypeConfig { DefaultRadiusKm = 1, DefaultShape = "Circle", DefaultCoverageType = "Engineering", Description = "Heavy Equipment" } }
        };

        public static readonly List<string> CoverageTypes = new()
        {
            "Operational",
            "Surveillance", 
            "Combat",
            "Reconnaissance",
            "Fire Support",
            "Patrol",
            "Defense",
            "Communication",
            "Supply",
            "Medical",
            "Engineering",
            "Transport",
            "Command",
            "Special Operations",
            "Naval",
            "Submarine",
            "Amphibious",
            "Strike",
            "Support",
            "Tactical",
            "Detection",
            "Maintenance"
        };

        public static readonly List<string> ShapeTypes = new()
        {
            "Circle",
            "Polygon",
            "Rectangle"
        };

        public static AssetTypeConfig? GetConfig(string assetType)
        {
            return AssetTypeConfigs.TryGetValue(assetType, out var config) ? config : null;
        }

        public static List<string> GetAssetTypes()
        {
            return AssetTypeConfigs.Keys.OrderBy(k => k).ToList();
        }
    }
}
