namespace TechWebSol.Constants
{
    /// <summary>
    /// Military Unit Organization Levels (Size/Hierarchy)
    /// </summary>
    public enum OrganizationLevel
    {
        Squad = 1,          // 8-13 personnel
        Platoon = 2,        // 26-64 personnel
        Company = 3,        // 80-250 personnel (Coy)
        Battalion = 4,      // 300-1,000 personnel (Bn)
        Regiment = 5,       // 1,000-3,000 personnel (Regt)
        Brigade = 6,        // 3,000-5,000 personnel (Brig/Bde)
        Division = 7,       // 10,000-25,000 personnel (Div)
        Corps = 8,          // 20,000-45,000 personnel
        Army = 9            // 50,000+ personnel
    }

    /// <summary>
    /// Military Unit Types (Functional Classification)
    /// </summary>
    public enum UnitType
    {
        Infantry,           // Foot soldiers
        Armoured,          // Tanks and armored vehicles
        Mechanized,        // Infantry with armored transport
        Artillery,         // Long-range fire support
        Aviation,          // Helicopters and aircraft
        AirDefense,        // Anti-aircraft systems
        Engineers,         // Combat engineering
        Signals,           // Communications
        Logistics,         // Supply and transport
        Medical,           // Medical support
        Reconnaissance,    // Scout/Recon units
        SpecialForces,     // Elite special operations
        AirborneParatroop, // Airborne/Paratrooper
        Marines,           // Naval infantry/marines
        Cavalry,           // Mounted/mechanized cavalry
        HeadquartersCommand, // Command and control
        Intelligence,      // Intelligence gathering
        MilitaryPolice,   // MP units
        CBRN,             // Chemical/Biological/Radiological/Nuclear
        Maintenance,       // Vehicle maintenance
        Cyber             // Cyber warfare
    }

    /// <summary>
    /// Military Unit Symbols - NATO APP-6 inspired
    /// Using Unicode and CSS to create military symbols
    /// </summary>
    public static class MilitarySymbols
    {
        // Organization Level Symbols (Markers)
        public static readonly Dictionary<OrganizationLevel, string> LevelSymbols = new()
        {
            { OrganizationLevel.Squad, "●" },           // Single dot
            { OrganizationLevel.Platoon, "●●" },        // Two dots
            { OrganizationLevel.Company, "●●●" },       // Three dots (vertical)
            { OrganizationLevel.Battalion, "╎" },        // Single vertical line
            { OrganizationLevel.Regiment, "╎╎" },       // Two vertical lines
            { OrganizationLevel.Brigade, "✕" },         // X symbol
            { OrganizationLevel.Division, "✕✕" },       // XX symbol
            { OrganizationLevel.Corps, "✕✕✕" },        // XXX symbol
            { OrganizationLevel.Army, "✕✕✕✕" }         // XXXX symbol
        };

        // Organization Level Abbreviations
        public static readonly Dictionary<OrganizationLevel, string> LevelAbbreviations = new()
        {
            { OrganizationLevel.Squad, "Sqd" },
            { OrganizationLevel.Platoon, "Plt" },
            { OrganizationLevel.Company, "Coy" },
            { OrganizationLevel.Battalion, "Bn" },
            { OrganizationLevel.Regiment, "Regt" },
            { OrganizationLevel.Brigade, "Brig" },
            { OrganizationLevel.Division, "Div" },
            { OrganizationLevel.Corps, "Corps" },
            { OrganizationLevel.Army, "Army" }
        };

        // Unit Type Icons (Font Awesome classes)
        public static readonly Dictionary<UnitType, string> UnitTypeIcons = new()
        {
            { UnitType.Infantry, "fa-person-rifle" },
            { UnitType.Armoured, "fa-shield" },
            { UnitType.Mechanized, "fa-truck-pickup" },
            { UnitType.Artillery, "fa-bullseye" },
            { UnitType.Aviation, "fa-helicopter" },
            { UnitType.AirDefense, "fa-shield-halved" },
            { UnitType.Engineers, "fa-hammer" },
            { UnitType.Signals, "fa-satellite-dish" },
            { UnitType.Logistics, "fa-boxes-stacked" },
            { UnitType.Medical, "fa-staff-snake" },
            { UnitType.Reconnaissance, "fa-binoculars" },
            { UnitType.SpecialForces, "fa-user-secret" },
            { UnitType.AirborneParatroop, "fa-parachute-box" },
            { UnitType.Marines, "fa-anchor" },
            { UnitType.Cavalry, "fa-horse-head" },
            { UnitType.HeadquartersCommand, "fa-flag" },
            { UnitType.Intelligence, "fa-eye" },
            { UnitType.MilitaryPolice, "fa-shield-halved" },
            { UnitType.CBRN, "fa-radiation" },
            { UnitType.Maintenance, "fa-wrench" },
            { UnitType.Cyber, "fa-laptop-code" }
        };

        // Unit Type Short Names
        public static readonly Dictionary<UnitType, string> UnitTypeNames = new()
        {
            { UnitType.Infantry, "Inf" },
            { UnitType.Armoured, "Armoured" },
            { UnitType.Mechanized, "Mech" },
            { UnitType.Artillery, "Arty" },
            { UnitType.Aviation, "Avn" },
            { UnitType.AirDefense, "ADA" },
            { UnitType.Engineers, "Eng" },
            { UnitType.Signals, "Sig" },
            { UnitType.Logistics, "Log" },
            { UnitType.Medical, "Med" },
            { UnitType.Reconnaissance, "Recce" },
            { UnitType.SpecialForces, "SF" },
            { UnitType.AirborneParatroop, "Airborne" },
            { UnitType.Marines, "Marines" },
            { UnitType.Cavalry, "Cav" },
            { UnitType.HeadquartersCommand, "HQ" },
            { UnitType.Intelligence, "Intel" },
            { UnitType.MilitaryPolice, "MP" },
            { UnitType.CBRN, "CBRN" },
            { UnitType.Maintenance, "Maint" },
            { UnitType.Cyber, "Cyber" }
        };

        /// <summary>
        /// Get display label for a military unit
        /// Format: "Brig Armoured 29" or "Bn Inf 15"
        /// </summary>
        public static string GetUnitLabel(OrganizationLevel orgLevel, UnitType unitType, string unitName)
        {
            var levelAbbr = LevelAbbreviations[orgLevel];
            var typeAbbr = UnitTypeNames[unitType];
            return $"{levelAbbr} {typeAbbr} {unitName}";
        }

        /// <summary>
        /// Get CSS classes for unit icon based on force type
        /// </summary>
        public static string GetForceTypeCssClass(string? forceType)
        {
            return forceType?.ToLower() switch
            {
                "blue land" => "force-blue-land",
                "fox land" => "force-fox-land", 
                "un" => "force-un",
                _ => "force-blue-land" // Default
            };
        }
    }
}

