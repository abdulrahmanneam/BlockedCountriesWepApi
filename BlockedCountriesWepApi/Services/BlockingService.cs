using System.Text.RegularExpressions;
using BlockedCountriesWepApi.Models.Dtos;
using BlockedCountriesWepApi.Models.Entities;
using BlockedCountriesWepApi.Repositories;

namespace BlockedCountriesWepApi.Services
{
    public class BlockingService : IBlockingService
    {
        private readonly IInMemoryRepository _repository;
        private readonly IGeoLocationService _geoLocationService;
        private readonly ILogger<BlockingService> _logger;

       
        private static readonly Dictionary<string, string> CountryNames = new()
        {
            { "AU", "Australia" },
            { "EG", "Egypt" },
            { "US", "United States" }
          
        };

        public BlockingService(IInMemoryRepository repository, IGeoLocationService geoLocationService, ILogger<BlockingService> logger)
        {
            _repository = repository;
            _geoLocationService = geoLocationService;
            _logger = logger;
        }

        public ApiResponse<object> AddBlockedCountry(string countryCode)
        {
            if (!IsValidCountryCode(countryCode))
                return ApiResponse<object>.Error("Invalid country code. Must be a 2-letter ISO code.", 400);

            var countryCodeUpper = countryCode.ToUpper();
            var country = new Country { Code = countryCodeUpper, Name = GetCountryName(countryCodeUpper) };
            if (!_repository.BlockedCountries.TryAdd(countryCodeUpper, country))
                return ApiResponse<object>.Error("Country is already blocked.", 409);

            _logger.LogInformation("Country {CountryCode} blocked successfully.", countryCode);
            return ApiResponse<object>.Ok(null);
        }

        public ApiResponse<object> RemoveBlockedCountry(string countryCode)
        {
            if (!IsValidCountryCode(countryCode))
                return ApiResponse<object>.Error("Invalid country code. Must be a 2-letter ISO code.", 400);

            if (!_repository.BlockedCountries.TryRemove(countryCode.ToUpper(), out _))
                return ApiResponse<object>.Error("Country is not blocked.", 404);

            _logger.LogInformation("Country {CountryCode} unblocked successfully.", countryCode);
            return ApiResponse<object>.Ok(null);
        }

        public ApiResponse<object> AddTemporalBlock(string countryCode, int durationMinutes)
        {
            if (!IsValidCountryCode(countryCode))
                return ApiResponse<object>.Error("Invalid country code. Must be a 2-letter ISO code.", 400);

            if (durationMinutes < 1 || durationMinutes > 1440)
                return ApiResponse<object>.Error("Duration must be between 1 and 1440 minutes.", 400);

            var countryCodeUpper = countryCode.ToUpper();
            if (_repository.BlockedCountries.TryGetValue(countryCodeUpper, out var existing) && existing.TemporalBlockExpiry.HasValue)
                return ApiResponse<object>.Error("Country is already temporarily blocked.", 409);

            var country = new Country
            {
                Code = countryCodeUpper,
                Name = GetCountryName(countryCodeUpper),
                TemporalBlockExpiry = DateTime.UtcNow.AddMinutes(durationMinutes)
            };

            _repository.BlockedCountries.AddOrUpdate(countryCodeUpper, country, (key, old) => country);
            _logger.LogInformation("Country {CountryCode} temporarily blocked for {DurationMinutes} minutes.", countryCode, durationMinutes);
            return ApiResponse<object>.Ok(null);
        }

        public async Task<ApiResponse<bool>> IsCountryBlockedAsync(string countryCode)
        {
            if (string.IsNullOrWhiteSpace(countryCode))
            {
                _logger.LogWarning("Country code is empty in IsCountryBlockedAsync.");
                return ApiResponse<bool>.Error("Country code is required.", 400);
            }

            var blockedCountry = _repository.GetBlockedCountry(countryCode.ToUpper());
            bool isBlocked = blockedCountry != null && (blockedCountry.TemporalBlockExpiry == null || blockedCountry.TemporalBlockExpiry > DateTime.UtcNow);

            _logger.LogInformation("Checked block status for country {CountryCode}: {IsBlocked}", countryCode, isBlocked);
            return ApiResponse<bool>.Ok(isBlocked);
        }

        private bool IsValidCountryCode(string countryCode)
        {
            return !string.IsNullOrWhiteSpace(countryCode) && Regex.IsMatch(countryCode, @"^[A-Z]{2}$");
        }

        private string GetCountryName(string countryCode)
        {
            return CountryNames.TryGetValue(countryCode.ToUpper(), out var name) ? name : countryCode;
        }
    }
}

