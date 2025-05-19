using System.Collections.Concurrent;
using System.Net;
using BlockedCountriesWepApi.Configurations;
using BlockedCountriesWepApi.Models.Dtos;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace BlockedCountriesWepApi.Services
{

    public class GeoLocationService : IGeoLocationService
    {
        private readonly HttpClient _httpClient;
        private readonly GeoApiConfig _config;
        private readonly ILogger<GeoLocationService> _logger;
        private readonly ConcurrentDictionary<string, GeoLocationResult> _cache = new();

        public GeoLocationService(HttpClient httpClient, IOptions<GeoApiConfig> config, ILogger<GeoLocationService> logger)
        {
            _httpClient = httpClient;
            _config = config.Value;
            _logger = logger;
        }

        public async Task<GeoLocationResult> GetGeoLocationAsync(string ipAddress)
        {
            if (_cache.TryGetValue(ipAddress, out var cachedResult))
            {
                _logger.LogInformation("Retrieved geolocation data for IP {IpAddress} from cache", ipAddress);
                return cachedResult;
            }

            try
            {
               
                if (!IPAddress.TryParse(ipAddress, out _))
                    throw new ArgumentException("Invalid IP address format.");

              
                if (string.IsNullOrWhiteSpace(_config.BaseUrl) || string.IsNullOrWhiteSpace(_config.ApiKey))
                    throw new InvalidOperationException("GeoApi configuration is missing.");

               
                var response = await _httpClient.GetAsync($"{_config.BaseUrl}?apiKey={_config.ApiKey}&ip={ipAddress}");
                response.EnsureSuccessStatusCode();
                var json = await response.Content.ReadAsStringAsync();
                var data = JsonConvert.DeserializeObject<dynamic>(json);

                var result = new GeoLocationResult
                {
                    IpAddress = ipAddress,
                    CountryCode = data?.location?.country_code2?.ToString() ?? string.Empty,
                    CountryName = data?.location?.country_name?.ToString() ?? string.Empty,
                    Isp = data?.network?.asn?.organization?.ToString() ?? data?.network?.company?.name?.ToString() ?? string.Empty
                };

               
                _cache.TryAdd(ipAddress, result);
                _logger.LogInformation("Added geolocation data for IP {IpAddress} to cache", ipAddress);
                return result;
            }
            catch (HttpRequestException ex) when (ex.StatusCode == HttpStatusCode.Unauthorized)
            {
                _logger.LogError(ex, "Invalid API key for IP {IpAddress}", ipAddress);
                throw new Exception("Invalid API key. Please check the API key in configuration.");
            }
            catch (HttpRequestException ex) when (ex.StatusCode == HttpStatusCode.TooManyRequests)
            {
                _logger.LogError(ex, "API rate limit exceeded for IP {IpAddress}", ipAddress);
                throw new Exception("API rate limit exceeded. Please try again later.");
            }
            catch (HttpRequestException ex) when (ex.StatusCode == HttpStatusCode.Locked)
            {
                _logger.LogError(ex, "API access locked for IP {IpAddress}. Status code: 423", ipAddress);
                throw new Exception("API access is temporarily locked. Please try again later or contact support.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to fetch geolocation data for IP {IpAddress}", ipAddress);
                throw new Exception($"Failed to fetch geolocation data: {ex.Message}");
            }
        }
    }
}
