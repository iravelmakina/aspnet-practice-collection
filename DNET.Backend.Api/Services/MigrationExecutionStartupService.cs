using DNET.Backend.DataAccess;
using Microsoft.EntityFrameworkCore;

namespace DNET.Backend.Api.Services;

public class MigrationExecutionStartupService : IHostedService
{
    private readonly IServiceProvider _serviceProvider;
    
    public MigrationExecutionStartupService(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }
    
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        Console.WriteLine("Starting database migration...");

        using (var scope = _serviceProvider.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<TableReservationsDbContext>();
            
            await dbContext.Database.MigrateAsync(cancellationToken);
        }

        Console.WriteLine("Database migration completed.");
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}
