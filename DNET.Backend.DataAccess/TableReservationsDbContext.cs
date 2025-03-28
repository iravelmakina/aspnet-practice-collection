using System.Reflection;
using DNET.Backend.DataAccess.Domain;
using Microsoft.EntityFrameworkCore;
namespace DNET.Backend.DataAccess;

public class TableReservationsDbContext : DbContext
{
    public DbSet<TableEntity> Tables { get; set; }
    public DbSet<LocationEntity> Locations { get; set; }
    public DbSet<HostEntity> Hosts { get; set; }
    public DbSet<ReservationEntity> Reservations { get; set; }
    public DbSet<ReservationDetailEntity> ReservationDetails { get; set; }
    public DbSet<ClientEntity> Clients { get; set; }
    
    public TableReservationsDbContext(DbContextOptions<TableReservationsDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());

        base.OnModelCreating(modelBuilder);
    }
}