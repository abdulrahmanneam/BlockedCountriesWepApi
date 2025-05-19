using BlockedCountriesWepApi.Models.Dtos;
using BlockedCountriesWepApi.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace BlockedCountriesWepApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CountriesController : ControllerBase
    {
        private readonly IBlockingService _blockingService;
        private readonly IInMemoryRepository _repository;
        private readonly ILogger<CountriesController> _logger;


        public CountriesController(IBlockingService blockingService, IInMemoryRepository repository , ILogger<CountriesController> logger)
        {
            _blockingService = blockingService;
            _repository = repository;
            _logger = logger;
        }

      
        [HttpPost("block")]
        public IActionResult BlockCountry([FromBody] BlockCountryRequest request)
        {
            var result = _blockingService.AddBlockedCountry(request.CountryCode);
            return result.Success ? Ok(result) : StatusCode(result.StatusCode, result);
        }

      
        [HttpDelete("block/{countryCode}")]
        public IActionResult UnblockCountry(string countryCode)
        {
            var result = _blockingService.RemoveBlockedCountry(countryCode);
            return result.Success ? Ok(result) : StatusCode(result.StatusCode, result);
        }

       
        [HttpGet("blocked")]
        public IActionResult GetBlockedCountries([FromQuery] int page = 1, [FromQuery] int pageSize = 10, [FromQuery] string? search = null)
        {
            if (page < 1 || pageSize < 1)
                return BadRequest(ApiResponse<PagedResponse<CountryDto>>.Error("Invalid page or pageSize.", 400));

            
            var countries = _repository.BlockedCountries.Values
                .Where(c => string.IsNullOrEmpty(search) || c.Code.Contains(search, StringComparison.OrdinalIgnoreCase) || c.Name.Contains(search, StringComparison.OrdinalIgnoreCase))
                .Select(c => new CountryDto { Code = c.Code, Name = c.Name })
                .ToList();

           
            var totalCount = countries.Count;
            var pagedItems = countries.Skip((page - 1) * pageSize).Take(pageSize);

            var response = new PagedResponse<CountryDto>
            {
                Page = page,
                PageSize = pageSize,
                TotalCount = totalCount,
                Items = pagedItems
            };

            _logger.LogInformation("Retrieved {TotalCount} blocked countries for page {Page}", totalCount, page);
            return Ok(ApiResponse<PagedResponse<CountryDto>>.Ok(response));
        }

        [HttpGet("all-blocked")]
        public IActionResult GetAllBlockedCountries()
        {
            var countries = _repository.BlockedCountries.Values
                .Select(c => new CountryDto { Code = c.Code, Name = c.Name })
                .ToList();

            _logger.LogInformation("Retrieved {Count} blocked countries", countries.Count);
            return Ok(ApiResponse<IEnumerable<CountryDto>>.Ok(countries));
        }

        [HttpPost("temporal-block")]
        public IActionResult TemporalBlockCountry([FromBody] TemporalBlockRequest request)
        {
            var result = _blockingService.AddTemporalBlock(request.CountryCode, request.DurationMinutes);
            return result.Success ? Ok(result) : StatusCode(result.StatusCode, result);
        }
    }
}
