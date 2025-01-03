using Microsoft.EntityFrameworkCore;
using MyAPI.Models;

namespace MyAPI.Helper
{
    public class SheducleService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly SEP490_G67Context _context;
        public SheducleService(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    //var now = DateTime.Now;
                    //var nextRun = now.Date.AddDays(1); // 00:00:00 ngày kế tiếp
                    //var delay = nextRun - now;
                  
                    await Task.Delay(TimeSpan.FromSeconds(20), stoppingToken);

                    using (var scope = _serviceProvider.CreateScope())
                    {
                        var dbContext = scope.ServiceProvider.GetRequiredService<SEP490_G67Context>();

                        await dbContext.Database.ExecuteSqlRawAsync(@"
                        DELETE FROM PromotionUser 
                        WHERE promotion_id IN 
                        (
                            SELECT pu.promotion_id 
                            FROM PromotionUser pu 
                            JOIN Promotion p ON pu.promotion_id = p.id
                            WHERE p.end_date < GETDATE()
                        );

                        DELETE FROM Promotion 
                        WHERE id IN 
                        (
                            SELECT id 
                            FROM Promotion 
                            WHERE end_date < GETDATE()
                        );
                    ");
                        //var user = await dbContext.Users.Where(x => x.ActiveCode != null && x.Status == false).ToListAsync();
                        //dbContext.RemoveRange(user);
                        //await dbContext.SaveChangesAsync();
                    }

                    Console.WriteLine($"Cleanup completed at {DateTime.Now}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error in CleanupPromotionService: {ex.Message}");
                }
            }
        }
    }
}
