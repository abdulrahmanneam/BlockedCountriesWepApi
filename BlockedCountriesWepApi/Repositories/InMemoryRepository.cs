using BlockedCountriesWepApi.Models.Entities;
using BlockedCountriesWepApi.Services;
using System.Collections.Concurrent;

namespace BlockedCountriesWepApi.Repositories
{
    public class InMemoryRepository : IInMemoryRepository
    {
        public ConcurrentDictionary<string, Country> BlockedCountries { get; } = new();
        public ConcurrentBag<BlockedAttemptLog> BlockedAttempts { get; } = new();

        public Country? GetBlockedCountry(string countryCode)
        {
            BlockedCountries.TryGetValue(countryCode.ToUpper(), out var country);
            return country;
        }
    }
}
