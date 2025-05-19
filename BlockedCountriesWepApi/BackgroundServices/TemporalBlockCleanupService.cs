using BlockedCountriesWepApi.Services;

namespace BlockedCountriesWepApi.BackgroundServices
{
    public class TemporalBlockCleanupService : BackgroundService
    {
        private readonly IInMemoryRepository _repository;

        public TemporalBlockCleanupService(IInMemoryRepository repository)
        {
            _repository = repository;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                var now = DateTime.UtcNow;
                var expired = _repository.BlockedCountries
                    .Where(c => c.Value.TemporalBlockExpiry.HasValue && c.Value.TemporalBlockExpiry <= now)
                    .Select(c => c.Key)
                    .ToList();

                foreach (var countryCode in expired)
                {
                    _repository.BlockedCountries.TryRemove(countryCode, out _);
                }

                await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
            }
        }
    }
}
