using BlockedCountriesWepApi.Models.Dtos;
using BlockedCountriesWepApi.Models.Entities;
using BlockedCountriesWepApi.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace BlockedCountriesWepApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class IpController : ControllerBase
    {
        private readonly IGeoLocationService _geoLocationService;
        private readonly IInMemoryRepository _repository;
        private readonly IBlockingService _blockingService;
        private readonly ILogger<IpController> _logger;

        public IpController(IGeoLocationService geoLocationService, IInMemoryRepository repository , IBlockingService blockingService, ILogger<IpController> logger)
        {
            _geoLocationService = geoLocationService;
            _repository = repository;
            _blockingService = blockingService;
            _logger = logger;
        }

     
        [HttpGet("lookup")]
        public async Task<IActionResult> LookupIp([FromQuery] string? ipAddress = null)
        {
            try
            {
                ipAddress ??= HttpContext.Connection.RemoteIpAddress?.ToString();
                if (string.IsNullOrWhiteSpace(ipAddress))
                    return BadRequest(ApiResponse<GeoLocationResult>.Error("Unable to determine IP address.", 400));

                var result = await _geoLocationService.GetGeoLocationAsync(ipAddress);
                return Ok(ApiResponse<GeoLocationResult>.Ok(result));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<GeoLocationResult>.Error(ex.Message, 500));
            }
        }

        [HttpGet("check-block")]
        public async Task<IActionResult> CheckBlock([FromQuery] string ipAddress)
        {
            
            try
            {
                if (string.IsNullOrWhiteSpace(ipAddress))
                {
                    _logger.LogWarning("IP address not provided in check-block request.");
                    return BadRequest(new { success = false, message = "IP address is required.", statusCode = 400 });
                }

                _logger.LogInformation("Checking block status for IP {IpAddress}", ipAddress);
                var geoLocation = await _geoLocationService.GetGeoLocationAsync(ipAddress);
                var blockResponse = await _blockingService.IsCountryBlockedAsync(geoLocation.CountryCode);

                if (!blockResponse.Success)
                    return StatusCode(blockResponse.StatusCode, new
                    {
                        success = false,
                        errorMessage = blockResponse.ErrorMessage,
                        statusCode = blockResponse.StatusCode
                    });

               
                var log = new BlockedAttemptLog
                {
                    IpAddress = ipAddress,
                    Timestamp = DateTime.UtcNow,
                    CountryCode = geoLocation.CountryCode,
                    IsBlocked = blockResponse.Data,
                    UserAgent = HttpContext.Request.Headers["User-Agent"].ToString()
                };
                _repository.BlockedAttempts.Add(log);
                _logger.LogInformation("Logged attempt for IP {IpAddress}, Country {CountryCode}, Blocked: {IsBlocked}", ipAddress, geoLocation.CountryCode, blockResponse.Data);

                return Ok(new
                {
                    success = true,
                    data = new
                    {
                        IpAddress = geoLocation.IpAddress,
                        IsBlocked = blockResponse.Data,
                        CountryCode = geoLocation.CountryCode,
                        CountryName = geoLocation.CountryName,
                        Isp = geoLocation.Isp,
                    },
                    statusCode = 200
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking block status for IP {IpAddress}", ipAddress);
                return StatusCode(500, new { success = false, errorMessage = ex.Message, statusCode = 500 });
            }
        }
    }
}
