using Microsoft.EntityFrameworkCore;
using SeaBattle.Domain.Models;

namespace SeaBattle.Persistence;

public class ApplicationContext : DbContext
{
    public ApplicationContext(DbContextOptions<ApplicationContext> options) : base(options)
    {
    }

    public DbSet<AppUser> Users { get; set; }
    public DbSet<Coordinate> Coordinates { get; set; }
    public DbSet<CoordinateType> CoordinateTypes { get; set; }
    public DbSet<Game> Games { get; set; }
    public DbSet<GameField> GameFields { get; set; }
    public DbSet<GameInvitation> GameInvitations { get; set; }
    public DbSet<Point> Points { get; set; }
    public DbSet<ShipCoordinate> ShipCoordinates { get; set; }
    public DbSet<UserGames> UserGames { get; set; }
    public DbSet<WarShip> WarShips { get; set; }
    public DbSet<SupportShip> SupportShips { get; set; }
    public DbSet<HybridShip> HybridShips { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AppUser>()
            .Property(u => u.AppUserId)
            .ValueGeneratedOnAdd();

        modelBuilder.Entity<Coordinate>()
            .Property(c => c.CoordinateId)
            .ValueGeneratedOnAdd();

        modelBuilder.Entity<CoordinateType>()
            .Property(ct => ct.CoordinateTypeId)
            .ValueGeneratedNever();

        modelBuilder.Entity<Game>()
            .Property(g => g.GameId)
            .ValueGeneratedOnAdd();

        modelBuilder.Entity<GameField>()
            .Property(gf => gf.GameFieldId)
            .ValueGeneratedOnAdd();

        modelBuilder.Entity<GameInvitation>()
            .Property(gi => gi.GameInvitationId)
            .ValueGeneratedOnAdd();

        modelBuilder.Entity<ShipCoordinate>()
            .Property(sc => sc.ShipCoordinateId)
            .ValueGeneratedOnAdd();

        modelBuilder.Entity<ShipType>()
            .Property(sc => sc.ShipTypeId)
            .ValueGeneratedNever();

        modelBuilder.Entity<UserGames>()
            .Property(ug => ug.UserGamesId)
            .ValueGeneratedOnAdd();
        
        modelBuilder.Entity<Point>()
            .Property(p => p.PointId)
            .ValueGeneratedNever();

        modelBuilder.Entity<WarShip>()
            .Property(ws => ws.ShipId)
            .ValueGeneratedOnAdd();

        modelBuilder.Entity<SupportShip>()
            .Property(ss => ss.ShipId)
            .ValueGeneratedOnAdd();

        modelBuilder.Entity<HybridShip>()
            .Property(hs => hs.ShipId)
            .ValueGeneratedOnAdd();

        modelBuilder.Entity<CoordinateType>().HasData(
            new CoordinateType { CoordinateTypeId = 1, Type = "Empty" },
            new CoordinateType { CoordinateTypeId = 2, Type = "Filled" },
            new CoordinateType { CoordinateTypeId = 3, Type = "Hit" },
            new CoordinateType { CoordinateTypeId = 4, Type = "Missed" },
            new CoordinateType { CoordinateTypeId = 5, Type = "Destroyed" }
        );

        modelBuilder.Entity<ShipType>().HasData(
            new ShipType { ShipTypeId = 1, Type = "War" },
            new ShipType { ShipTypeId = 2, Type = "Support" },
            new ShipType { ShipTypeId = 3, Type = "Hybrid" }
        );

        base.OnModelCreating(modelBuilder);
    }
}