using BlockedCountriesWepApi.Models.Dtos;

namespace BlockedCountriesWepApi.Services
{
    public interface IBlockingService
    {
        ApiResponse<object> AddBlockedCountry(string countryCode);
        ApiResponse<object> RemoveBlockedCountry(string countryCode);
        ApiResponse<object> AddTemporalBlock(string countryCode, int durationMinutes);
        Task<ApiResponse<bool>> IsCountryBlockedAsync(string countryCode);
    }
}
