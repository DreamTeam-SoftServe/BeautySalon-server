using Domain.Entities;
using Domain.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace API.BackgroundJobs
{
    public class AutoCloseBookingsService : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<AutoCloseBookingsService> _logger;

        public AutoCloseBookingsService(IServiceScopeFactory scopeFactory, ILogger<AutoCloseBookingsService> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);
            _logger.LogInformation("Background Service for auto-closing records launched.");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await ProcessOutdatedBookings();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error during automatic closing of records.");
                }

                await Task.Delay(TimeSpan.FromHours(1), stoppingToken);
            }
        }

        private async Task ProcessOutdatedBookings()
        {
            using var scope = _scopeFactory.CreateScope();
            var repository = scope.ServiceProvider.GetRequiredService<IRepository<ServiceAppointment>>();

            var allAppointments = await repository.GetAllAsync();

            var threshold = DateTime.UtcNow.AddHours(-4);

            var outdatedAppointments = allAppointments.Where(a =>
                a.Start_date < threshold &&
                (a.Status.ToString() == "CONFIRMED" || a.Status.ToString() == "IN_PROGRESS")).ToList();

            if (outdatedAppointments.Any())
            {
                foreach (var app in outdatedAppointments)
                {
                    app.Status = Domain.Enum.AppointmentStatus.COMPLETED;
                    await repository.UpdateAsync(app.Id, app);
                }
                _logger.LogInformation($"Automatically closed records: {outdatedAppointments.Count}");
            }
        }
    }
}