using DNET.Backend.DataAccess;
using Microsoft.EntityFrameworkCore;

namespace DNET.Backend.Api.Tests;

public static class Utils
{
    public static TableReservationsDbContext CreateInMemoryDatabaseContext()
    {
        var options = new DbContextOptionsBuilder<TableReservationsDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .EnableDetailedErrors()
            .Options;

        var databaseContext = new TableReservationsDbContext(options);

        databaseContext.Database.EnsureCreated();

        return databaseContext;
    }
}