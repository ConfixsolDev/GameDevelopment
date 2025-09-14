namespace WargameBoard.Web.Data
{
    // Infrastructure/Seeding/DbSeeder.cs
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;
    using WargameBoard.Core.Data;
    using WargameBoard.Core.Entities;

    public static class DbSeeder
    {
        public static async Task RunAsync(IServiceProvider services, bool includeTestData = true)
        {
            using var scope = services.CreateScope();
            var sp = scope.ServiceProvider;
            var db = sp.GetRequiredService<WargameDbContext>();
            var logger = sp.GetService<ILoggerFactory>()?.CreateLogger("DbSeeder");

            await db.Database.MigrateAsync();

            // ----------------- Lookups (Base) -----------------
            if (!await db.Sides.AnyAsync())
            {
                db.Sides.AddRange(
                    new Side { Name = "Blue", Color = "#0d6efd" },
                    new Side { Name = "Red", Color = "#dc3545" }
                );
            }

            if (!await db.UnitTypes.AnyAsync())
            {
                db.UnitTypes.AddRange(
                    new UnitType { Name = "Infantry Battalion", Category = "Infantry" },
                    new UnitType { Name = "Tank Regiment", Category = "Armor" },
                    new UnitType { Name = "Artillery Battalion 155mm SP", Category = "Support" },
                    new UnitType { Name = "Engineer Battalion", Category = "Support" }
                );
            }

            if (!await db.MovementProfiles.AnyAsync())
            {
                db.MovementProfiles.AddRange(
                    new MovementProfile { Name = "Infantry", RoadKmph = 30, XCountryKmph = 5, CombatAdvanceKmph = 1 },
                    new MovementProfile { Name = "APC Infantry", RoadKmph = 20, XCountryKmph = 10, CombatAdvanceKmph = 1 },
                    new MovementProfile { Name = "Armor", RoadKmph = 15, XCountryKmph = 10, CombatAdvanceKmph = 1 }
                );
            }

            if (!await db.TerrainTypes.AnyAsync())
            {
                // Use Codes so UI doesn’t depend on numeric IDs
                db.TerrainTypes.AddRange(
                    new TerrainType { Name = "Firm", Code = "terrain-clear", MovementCost = 1, DefenseModifier = 0, Color = "#e8f0fe" },
                    new TerrainType { Name = "Sandy/Desert", Code = "terrain-desert", MovementCost = 2, DefenseModifier = 0, Color = "#f6e9c9" },
                    new TerrainType { Name = "Cuttings", Code = "terrain-mountain", MovementCost = 3, DefenseModifier = 1, Color = "#d0d6e0" },
                    new TerrainType { Name = "Boggy", Code = "terrain-swamp", MovementCost = 4, DefenseModifier = 1, Color = "#c5e6c8" },
                    new TerrainType { Name = "Swamp", Code = "terrain-swamp", MovementCost = 5, DefenseModifier = 2, Color = "#a7d8a8" }
                );
            }

            if (!await db.FortificationTypes.AnyAsync())
            {
                db.FortificationTypes.AddRange(
                    new FortificationType { Name = "Open Field" },
                    new FortificationType { Name = "Open Trenches" },
                    new FortificationType { Name = "Trenches + OHP" },
                    new FortificationType { Name = "RCC OHP" }
                );
            }

            if (!await db.ObstacleTypes.AnyAsync())
            {
                db.ObstacleTypes.AddRange(
                    new ObstacleType { Name = "Minefield" },
                    new ObstacleType { Name = "Mixed Belt" },
                    new ObstacleType { Name = "Layered Belt" }
                );
            }

            if (!await db.TokenGroups.AnyAsync())
            {
                db.TokenGroups.AddRange(
                    new TokenGroup { Name = "Unit", Category = "Unit", DefaultWidthMm = 25, DefaultHeightMm = 25 },
                    new TokenGroup { Name = "Objective", Category = "Objective", DefaultWidthMm = 20, DefaultHeightMm = 20 },
                    new TokenGroup { Name = "Status", Category = "Status", DefaultWidthMm = 15, DefaultHeightMm = 15 }
                );
            }

            await db.SaveChangesAsync();

            // ----------------- Lookups (Add missing from SelectListService) -----------------
            // Sides
            var sideAdds = new (string Name, string Color)[]
            {
                ("Blue Team", "#0d6efd"), ("Red Team", "#dc3545"),
                ("Green Team", "#198754"), ("Yellow Team", "#ffc107"),
                ("Neutral", "#6c757d")
            };
            foreach (var (name, color) in sideAdds)
                if (!await db.Sides.AnyAsync(s => s.Name == name))
                    db.Sides.Add(new Side { Name = name, Color = color });

            // UnitTypes
            var unitTypeAdds = new (string Name, string Category)[]
            {
                ("Infantry", "Infantry"),
                ("Armor", "Armor"),
                ("Artillery", "Support"),
                ("Air Defense", "Support"),
                ("Engineer", "Support"),
                ("Reconnaissance", "Recon")
            };
            foreach (var (name, cat) in unitTypeAdds)
                if (!await db.UnitTypes.AnyAsync(u => u.Name == name))
                    db.UnitTypes.Add(new UnitType { Name = name, Category = cat });

            // MovementProfiles (reasonable defaults)
            var moveAdds = new (string Name, int Road, int Xc, int Combat)[]
            {
                ("Foot Infantry", 30, 5, 1),
                ("Motorized Infantry", 40, 15, 2),
                ("Mechanized Infantry", 25, 12, 2),
                ("Armor", 15, 10, 1),   // won’t duplicate due to Name check
                ("Artillery", 20, 8, 1),
                ("Air Mobile", 120, 120, 10)
            };
            foreach (var (name, rd, xc, ca) in moveAdds)
                if (!await db.MovementProfiles.AnyAsync(m => m.Name == name))
                    db.MovementProfiles.Add(new MovementProfile { Name = name, RoadKmph = rd, XCountryKmph = xc, CombatAdvanceKmph = ca });

            // TerrainTypes (SelectList-style names + stable Codes) — add if missing; also backfill Code if null/empty
            var terrainAdds = new (string Name, string Code, int Cost, int Def, string Color)[]
            {
                ("Clear",   "terrain-clear",   1,  0, "#e8f0fe"),
                ("Forest",  "terrain-forest",  3,  1, "#b7d4a8"),
                ("Mountain","terrain-mountain",4,  2, "#bfb9b0"),
                ("Water",   "terrain-water",   99, -1, "#a4c8f0"), // 99 => effectively impassable to ground
                ("Desert",  "terrain-desert",  2,  0, "#f6e9c9"),
                ("Urban",   "terrain-urban",   2,  2, "#d1d1d1"),
                ("Swamp",   "terrain-swamp",   5,  2, "#a7d8a8")
            };
            foreach (var (name, code, cost, def, color) in terrainAdds)
            {
                var tt = await db.TerrainTypes.FirstOrDefaultAsync(t => t.Name == name);
                if (tt == null)
                {
                    db.TerrainTypes.Add(new TerrainType
                    {
                        Name = name,
                        Code = code,
                        MovementCost = cost,
                        DefenseModifier = def,
                        Color = color
                    });
                }
                else if (string.IsNullOrWhiteSpace(tt.Code))
                {
                    tt.Code = code; // backfill code for existing rows
                }
            }

            // FortificationTypes (add the extra list)
            var fortAdds = new[] { "Bunker", "Trench", "Pillbox", "Fortress", "Barricade" };
            foreach (var name in fortAdds)
                if (!await db.FortificationTypes.AnyAsync(f => f.Name == name))
                    db.FortificationTypes.Add(new FortificationType { Name = name });

            // ObstacleTypes (add the extra list)
            var obstacleAdds = new[] { "Barbed Wire", "Tank Trap", "Anti-Tank Ditch", "Roadblock" };
            foreach (var name in obstacleAdds)
                if (!await db.ObstacleTypes.AnyAsync(o => o.Name == name))
                    db.ObstacleTypes.Add(new ObstacleType { Name = name });

            // TokenGroups (UI-friendly labels)
            var TokenGroupAdds = new (string Name, string Category, int W, int H)[]
            {
                ("Unit Token", "Unit", 25, 25),
                ("Objective Token", "Objective", 20, 20),
                ("Status Token", "Status", 15, 15),
                ("Terrain Token", "Terrain", 20, 20),
                ("Marker Token", "Marker", 10, 10)
            };
            foreach (var (name, cat, w, h) in TokenGroupAdds)
                if (!await db.TokenGroups.AnyAsync(t => t.Name == name))
                    db.TokenGroups.Add(new TokenGroup { Name = name, Category = cat, DefaultWidthMm = w, DefaultHeightMm = h });

            await db.SaveChangesAsync();
            logger?.LogInformation("Seeded lookups and SelectList additions (with TerrainType.Code).");

            // ----------------- Test/Demo Data -----------------
            if (includeTestData)
            {
                // Map
                if (!await db.Hexes.AnyAsync())
                {
                    var terrainIds = await db.TerrainTypes.Select(t => t.Id).ToListAsync();
                    var rnd = new Random(42);
                    for (int q = 0; q < 12; q++)
                    {
                        for (int r = 0; r < 12; r++)
                        {
                            db.Hexes.Add(new Hex
                            {
                                Q = q,
                                R = r,
                                TerrainTypeId = terrainIds[rnd.Next(terrainIds.Count)],
                                KeyFeature = (q == 6 && r == 6) ? "Crossroads" : null
                            });
                        }
                    }
                    await db.SaveChangesAsync();
                }

                // Units + capabilities
                if (!await db.Units.AnyAsync())
                {
                    var blueSideId = await db.Sides.Where(s => s.Name == "Blue").Select(s => s.Id).FirstAsync();
                    var redSideId = await db.Sides.Where(s => s.Name == "Red").Select(s => s.Id).FirstAsync();

                    var infantryTypeId = await db.UnitTypes
                        .Where(u => u.Name.Contains("Infantry"))
                        .Select(u => u.Id)
                        .FirstAsync();

                    var armorTypeId = await db.UnitTypes
                        .Where(u => u.Name == "Armor" || u.Name.Contains("Tank") || u.Category == "Armor")
                        .Select(u => u.Id)
                        .FirstAsync();

                    var infantryMoveId = await db.MovementProfiles
                        .Where(m => m.Name.Contains("Infantry"))
                        .Select(m => m.Id)
                        .FirstAsync();

                    var armorMoveId = await db.MovementProfiles
                        .Where(m => m.Name == "Armor")
                        .Select(m => m.Id)
                        .FirstAsync();

                    var blueInf = new Unit
                    {
                        Name = "Blue Infantry Bn A",
                        SideId = blueSideId,
                        UnitTypeId = infantryTypeId,
                        Personnel = 765,
                        Quality = QualityLevel.Regular,
                        Cohesion = 70,
                        MovementProfileId = infantryMoveId
                    };
                    var redArmor = new Unit
                    {
                        Name = "Red Armor Regt",
                        SideId = redSideId,
                        UnitTypeId = armorTypeId,
                        VehiclesPrimary = 58,
                        Quality = QualityLevel.Regular,
                        Cohesion = 70,
                        MovementProfileId = armorMoveId
                    };

                    db.Units.AddRange(blueInf, redArmor);
                    await db.SaveChangesAsync();

                    db.UnitCapabilities.AddRange(
                        new UnitCapability { UnitId = blueInf.Id, AttackSoft = 6, AttackHard = 3, Defense = 5, IndirectSupport = 3, AtgmCount = 8, MortarsCount = 12 },
                        new UnitCapability { UnitId = redArmor.Id, AttackSoft = 8, AttackHard = 8, Defense = 6, IndirectSupport = 0 }
                    );
                    await db.SaveChangesAsync();
                }

                // Scenario
                if (!await db.Scenarios.AnyAsync())
                {
                    var scen = new Scenario
                    {
                        Name = "Breakthrough Operation",
                        TurnLengthMinutes = 60,
                        MaxTurns = 12,
                        Weather = WeatherType.Clear,
                        Notes = "Red must seize the crossroads by Turn <= 10."
                    };
                    db.Scenarios.Add(scen);
                    await db.SaveChangesAsync();

                    var startBlue = await db.Hexes.FirstAsync();
                    var startRed = await db.Hexes.OrderByDescending(h => h.Id).FirstAsync();

                    var blueInf = await db.Units.FirstAsync(u => u.Name.Contains("Blue Infantry"));
                    var redArmor = await db.Units.FirstAsync(u => u.Name.Contains("Red Armor"));

                    db.ScenarioUnits.AddRange(
                        new ScenarioUnit { ScenarioId = scen.Id, UnitId = blueInf.Id, StartHexId = startBlue.Id, Steps = 4, Posture = Posture.Defensive },
                        new ScenarioUnit { ScenarioId = scen.Id, UnitId = redArmor.Id, StartHexId = startRed.Id, Steps = 6, Posture = Posture.Offensive }
                    );

                    var crossroads = await db.Hexes.FirstOrDefaultAsync(h => h.KeyFeature == "Crossroads");
                    if (crossroads != null)
                    {
                        var blueSideId = await db.Sides.Where(s => s.Name == "Blue").Select(s => s.Id).FirstAsync();

                        db.ScenarioObjectives.Add(new ScenarioObjective
                        {
                            ScenarioId = scen.Id,
                            HexId = crossroads.Id,
                            SideId = blueSideId,
                            VictoryPoints = 5,
                            ConditionKind = VictoryConditionKind.SeizeByTurn,
                            TurnThreshold = 10
                        });
                    }

                    await db.SaveChangesAsync();
                }

                // Tokens
                if (!await db.TokenDesigns.AnyAsync())
                {
                    var unitTokenGroupId = await db.TokenGroups
                        .Where(t => t.Name == "Unit")
                        .Select(t => t.Id)
                        .FirstAsync();

                    var blueSideId = await db.Sides.Where(s => s.Name == "Blue").Select(s => s.Id).FirstAsync();
                    var redSideId = await db.Sides.Where(s => s.Name == "Red").Select(s => s.Id).FirstAsync();

                    var blueUnitToken = new TokenDesign { Name = "Blue Unit Token", TokenGroupId = unitTokenGroupId, DefaultSideId = blueSideId, WidthMm = 25, HeightMm = 25 };
                    var redUnitToken = new TokenDesign { Name = "Red Unit Token", TokenGroupId = unitTokenGroupId, DefaultSideId = redSideId, WidthMm = 25, HeightMm = 25 };
                    db.TokenDesigns.AddRange(blueUnitToken, redUnitToken);
                    await db.SaveChangesAsync();

                    db.TokenPieces.AddRange(
                        new TokenPiece { TokenDesignId = blueUnitToken.Id, SideId = blueSideId, Serial = "BLUE-001", HardwareIdentity = "UID-BLUE-001" },
                        new TokenPiece { TokenDesignId = redUnitToken.Id, SideId = redSideId, Serial = "RED-001", HardwareIdentity = "UID-RED-001" }
                    );
                    await db.SaveChangesAsync();
                }

                // Board & Cells
                if (!await db.Boards.AnyAsync())
                {
                    var board = new Board { Name = "Main Board", Description = "12x12 demo board" };
                    db.Boards.Add(board);
                    await db.SaveChangesAsync();

                    var allHexes = await db.Hexes.Take(144).ToListAsync();
                    int idx = 0;
                    for (int r = 0; r < 12; r++)
                    {
                        for (int c = 0; c < 12; c++)
                        {
                            var hex = allHexes[idx++];
                            db.BoardCells.Add(new BoardCell
                            {
                                BoardId = board.Id,
                                Row = r,
                                Col = c,
                                SensorAddress = $"i2c:{r:D2}-{c:D2}",
                                HexId = hex.Id,
                                Threshold = 0.25
                            });
                        }
                    }
                    await db.SaveChangesAsync();
                }

                // Session
                if (!await db.Sessions.AnyAsync())
                {
                    var scen = await db.Scenarios.FirstAsync();
                    var redSideId = await db.Sides.Where(s => s.Name == "Red").Select(s => s.Id).FirstAsync();

                    var session = new Session
                    {
                        ScenarioId = scen.Id,
                        CreatedAt = DateTime.UtcNow,
                        StartedAt = DateTime.UtcNow,
                        CurrentSideId = redSideId
                    };
                    db.Sessions.Add(session);
                    await db.SaveChangesAsync();

                    var turn1 = new Turn { SessionId = session.Id, Number = 1, StartedAt = DateTime.UtcNow };
                    db.Turns.Add(turn1);
                    await db.SaveChangesAsync();
                }

                logger?.LogInformation("Seeded test/demo data.");
            }
        }
    }
}
