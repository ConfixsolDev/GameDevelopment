// ==============================
// WargameDbContext.cs
// ==============================
using Microsoft.EntityFrameworkCore;
using WargameBoard.Core.Entities; // adjust if your entities live elsewhere

namespace WargameBoard.Core.Data
{
    public class WargameDbContext : DbContext
    {
        public WargameDbContext(DbContextOptions<WargameDbContext> options) : base(options) { }

        // ---- Admin / Lookups
        public DbSet<Side> Sides => Set<Side>();
        public DbSet<UnitType> UnitTypes => Set<UnitType>();
        public DbSet<MovementProfile> MovementProfiles => Set<MovementProfile>();
        public DbSet<TerrainType> TerrainTypes => Set<TerrainType>();
        public DbSet<FortificationType> FortificationTypes => Set<FortificationType>();
        public DbSet<ObstacleType> ObstacleTypes => Set<ObstacleType>();
        public DbSet<TokenGroup> TokenGroups => Set<TokenGroup>();

        // ---- Map
        public DbSet<Hex> Hexes => Set<Hex>();
        public DbSet<HexFeature> HexFeatures => Set<HexFeature>();

        // ---- Forces
        public DbSet<Unit> Units => Set<Unit>();
        public DbSet<UnitCapability> UnitCapabilities => Set<UnitCapability>();

        // ---- Scenario
        public DbSet<Scenario> Scenarios => Set<Scenario>();
        public DbSet<ScenarioUnit> ScenarioUnits => Set<ScenarioUnit>();
        public DbSet<ScenarioObjective> ScenarioObjectives => Set<ScenarioObjective>();
        public DbSet<ObjectiveControlLog> ObjectiveControlLogs => Set<ObjectiveControlLog>();

        // ---- Tokens
        public DbSet<TokenDesign> TokenDesigns => Set<TokenDesign>();
        public DbSet<TokenPiece> TokenPieces => Set<TokenPiece>();

        // ---- Hardware / Board
        public DbSet<Board> Boards => Set<Board>();
        public DbSet<BoardCell> BoardCells => Set<BoardCell>();
        public DbSet<TouchEvent> TouchEvents => Set<TouchEvent>();

        // ---- Runtime
        public DbSet<Session> Sessions => Set<Session>();
        public DbSet<Turn> Turns => Set<Turn>();
        public DbSet<Placement> Placements => Set<Placement>();
        public DbSet<MoveEvent> MoveEvents => Set<MoveEvent>();
        public DbSet<SessionAssignment> SessionAssignments => Set<SessionAssignment>();

        protected override void OnModelCreating(ModelBuilder b)
        {
            // ---------- Uniques / Indexes ----------
            b.Entity<Side>().HasIndex(x => x.Name).IsUnique();
            b.Entity<UnitType>().HasIndex(x => x.Name).IsUnique();
            b.Entity<TerrainType>().HasIndex(x => x.Name).IsUnique();
            b.Entity<Hex>().HasIndex(x => new { x.Q, x.R }).IsUnique();

            // Provider-safe unique index for HardwareIdentity
            var tpIdx = b.Entity<TokenPiece>().HasIndex(x => x.HardwareIdentity).IsUnique();
            if (Database.IsSqlServer())
            {
                tpIdx.HasFilter("[HardwareIdentity] IS NOT NULL"); // SQL Server supports filtered indexes
            }
            // SQLite: NULLs are treated as distinct, so unique index is fine without filter

            // ---------- Unit 1:1 Capability ----------
            b.Entity<UnitCapability>().HasKey(uc => uc.UnitId);
            b.Entity<Unit>()
                .HasOne(u => u.Capability)
                .WithOne(c => c.Unit)
                .HasForeignKey<UnitCapability>(c => c.UnitId)
                .OnDelete(DeleteBehavior.NoAction);

            // ---------- MoveEvent relationships ----------
            b.Entity<MoveEvent>()
                .HasOne(me => me.FromHex)
                .WithMany(h => h.MoveEventsFrom)
                .HasForeignKey(me => me.FromHexId)
                .OnDelete(DeleteBehavior.NoAction);

            b.Entity<MoveEvent>()
                .HasOne(me => me.ToHex)
                .WithMany(h => h.MoveEventsTo)
                .HasForeignKey(me => me.ToHexId)
                .OnDelete(DeleteBehavior.NoAction);

            b.Entity<MoveEvent>()
                .HasOne(me => me.Session)
                .WithMany(s => s.MoveEvents) // add ICollection<MoveEvent> MoveEvents {get;set;} to Session
                .HasForeignKey(me => me.SessionId)
                .OnDelete(DeleteBehavior.NoAction);

            b.Entity<MoveEvent>()
                .HasOne(me => me.Turn)
                .WithMany(t => t.MoveEvents)
                .HasForeignKey(me => me.TurnId)
                .OnDelete(DeleteBehavior.NoAction);

            // ---------- ObjectiveControlLog (break multi-cascade paths) ----------
            b.Entity<ObjectiveControlLog>()
                .HasOne(l => l.Session)
                .WithMany(s => s.ObjectiveLogs)
                .HasForeignKey(l => l.SessionId)
                .OnDelete(DeleteBehavior.NoAction); // IMPORTANT: NoAction to avoid multiple cascade paths

            b.Entity<ObjectiveControlLog>()
                .HasOne(l => l.Turn)
                .WithMany(t => t.ObjectiveLogs)
                .HasForeignKey(l => l.TurnId)
                .OnDelete(DeleteBehavior.NoAction); // IMPORTANT: NoAction (not SetNull) to avoid cascade chains via Turn

            b.Entity<ObjectiveControlLog>()
                .HasOne(l => l.Objective)
                .WithMany(o => o.Logs)
                .HasForeignKey(l => l.ObjectiveId)
                .OnDelete(DeleteBehavior.NoAction);

            b.Entity<ObjectiveControlLog>()
                .HasOne(l => l.Side)
                .WithMany()
                .HasForeignKey(l => l.SideId)
                .OnDelete(DeleteBehavior.NoAction);

            // ---------- BoardCell ----------
            b.Entity<BoardCell>()
                .HasOne(c => c.Board)
                .WithMany(bd => bd.Cells)
                .HasForeignKey(c => c.BoardId)
                .OnDelete(DeleteBehavior.NoAction);

            b.Entity<BoardCell>()
                .HasOne(c => c.Hex)
                .WithMany(h => h.BoardCells)
                .HasForeignKey(c => c.HexId)
                .OnDelete(DeleteBehavior.SetNull);

            // ---------- Placement (keep history) ----------
            b.Entity<Placement>()
                .HasOne(p => p.Session)
                .WithMany(s => s.Placements)
                .HasForeignKey(p => p.SessionId)
                .OnDelete(DeleteBehavior.NoAction);

            b.Entity<Placement>()
                .HasOne(p => p.Turn)
                .WithMany(t => t.Placements)
                .HasForeignKey(p => p.TurnId)
                .OnDelete(DeleteBehavior.NoAction);

            b.Entity<Placement>()
                .HasOne(p => p.Hex)
                .WithMany(h => h.Placements)
                .HasForeignKey(p => p.HexId)
                .OnDelete(DeleteBehavior.NoAction);

            b.Entity<Placement>()
                .HasOne(p => p.TokenPiece)
                .WithMany()
                .HasForeignKey(p => p.TokenPieceId)
                .OnDelete(DeleteBehavior.NoAction);

            // ---------- ScenarioUnit / ScenarioObjective ----------
            b.Entity<ScenarioUnit>()
                .HasOne(su => su.Scenario)
                .WithMany(s => s.ScenarioUnits)
                .HasForeignKey(su => su.ScenarioId)
                .OnDelete(DeleteBehavior.NoAction);

            b.Entity<ScenarioUnit>()
                .HasOne(su => su.StartHex)
                .WithMany(h => h.ScenarioUnits)
                .HasForeignKey(su => su.StartHexId)
                .OnDelete(DeleteBehavior.NoAction);

            b.Entity<ScenarioUnit>()
                .HasOne(su => su.Unit)
                .WithMany()
                .HasForeignKey(su => su.UnitId)
                .OnDelete(DeleteBehavior.NoAction);

            b.Entity<ScenarioObjective>()
                .HasOne(so => so.Scenario)
                .WithMany(s => s.ScenarioObjectives)
                .HasForeignKey(so => so.ScenarioId)
                .OnDelete(DeleteBehavior.NoAction);

            b.Entity<ScenarioObjective>()
                .HasOne(so => so.Hex)
                .WithMany(h => h.ScenarioObjectives)
                .HasForeignKey(so => so.HexId)
                .OnDelete(DeleteBehavior.NoAction);

            b.Entity<ScenarioObjective>()
                .HasOne(so => so.Side)
                .WithMany()
                .HasForeignKey(so => so.SideId)
                .OnDelete(DeleteBehavior.NoAction);

            // ---------- SessionAssignment ----------
            b.Entity<SessionAssignment>()
                .HasOne(a => a.Session)
                .WithMany(s => s.Assignments)
                .HasForeignKey(a => a.SessionId)
                .OnDelete(DeleteBehavior.NoAction);

            b.Entity<SessionAssignment>()
                .HasOne(a => a.TokenPiece)
                .WithMany()
                .HasForeignKey(a => a.TokenPieceId)
                .OnDelete(DeleteBehavior.NoAction);

            b.Entity<SessionAssignment>()
                .HasOne(a => a.ScenarioUnit)
                .WithMany()
                .HasForeignKey(a => a.ScenarioUnitId)
                .OnDelete(DeleteBehavior.SetNull);

            b.Entity<SessionAssignment>()
                .HasOne(a => a.HexFeature)
                .WithMany()
                .HasForeignKey(a => a.HexFeatureId)
                .OnDelete(DeleteBehavior.SetNull);

            b.Entity<SessionAssignment>()
                .HasOne(a => a.ScenarioObjective)
                .WithMany()
                .HasForeignKey(a => a.ScenarioObjectiveId)
                .OnDelete(DeleteBehavior.SetNull);
        }
    }
}
