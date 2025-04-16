using DNET.Backend.DataAccess;
namespace DNET.Backend.Api.Services;

public class ResetCodeBackgroundCleaner : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;

    public ResetCodeBackgroundCleaner(IServiceScopeFactory scopeFactory)
    {
        _scopeFactory = scopeFactory;
    }
    
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _scopeFactory.CreateScope();
                await RunTask(scope.ServiceProvider.GetRequiredService<TableReservationsDbContext>());
                
                await Task.Delay(TimeSpan.FromSeconds(60), stoppingToken);
            }
            catch (Exception e)
            {
                Console.WriteLine(DateTime.Now + " " + e);
            }
        }
    }

    private async Task RunTask(TableReservationsDbContext dbContext)
    {
        var expiredResetCodes = dbContext.ResetCodes.Where(c => c.ExpiresAt.AddMinutes(10).ToLocalTime() <= DateTime.Now);
        
        dbContext.ResetCodes.RemoveRange(expiredResetCodes);
        await dbContext.SaveChangesAsync();
    }
}