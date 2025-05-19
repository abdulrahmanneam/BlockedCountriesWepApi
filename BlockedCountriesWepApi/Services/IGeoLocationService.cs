using BlockedCountriesWepApi.Models.Dtos;

namespace BlockedCountriesWepApi.Services
{
    public interface IGeoLocationService
    {
        Task<GeoLocationResult> GetGeoLocationAsync(string ipAddress);
    }
}
