using BlockedCountriesWepApi.Models.Entities;
using System.Collections.Concurrent;

namespace BlockedCountriesWepApi.Services
{
    public interface IInMemoryRepository
    {
        ConcurrentDictionary<string, Country> BlockedCountries { get; }
        ConcurrentBag<BlockedAttemptLog> BlockedAttempts { get; }
        Country? GetBlockedCountry(string countryCode);
    }
}
