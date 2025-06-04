using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using air_reservation.Data_Access_Layer;
using air_reservation.Models.Reservation_Model_;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

public class ReservationCleanupService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;

    public ReservationCleanupService(IServiceScopeFactory scopeFactory)
    {
        _scopeFactory = scopeFactory;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            using (var scope = _scopeFactory.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

                var expiredReservations = await dbContext.Reservations
                    .Where(r => r.Status == ReservationStatus.Pending && r.ExpiresAt <= DateTime.UtcNow)
                    .ToListAsync();

                foreach (var reservation in expiredReservations)
                {
                    reservation.Status = ReservationStatus.Cancelled;
                    dbContext.Entry(reservation).State = EntityState.Modified; // ✅ Mark as modified
                    Console.WriteLine($"Reservation {reservation.Id} was cancelled due to timeout.");
                }


                await dbContext.SaveChangesAsync();
            }

            await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken); // ✅ Check every minute
        }
    }
}