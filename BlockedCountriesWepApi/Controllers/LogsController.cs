using BlockedCountriesWepApi.Models.Dtos;
using BlockedCountriesWepApi.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace BlockedCountriesWepApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LogsController : ControllerBase
    {
        private readonly IInMemoryRepository _repository;
        private readonly ILogger<LogsController> _logger;

        public LogsController(IInMemoryRepository repository , ILogger<LogsController> logger)
        {
            _repository = repository;
            _logger = logger;
        }

       
        [HttpGet("blocked-attempts")]
        public IActionResult GetBlockedAttempts([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            if (page < 1 || pageSize < 1)
                return BadRequest(ApiResponse<PagedResponse<BlockedAttemptLogDto>>.Error("Invalid page or pageSize.", 400));

            var attempts = _repository.BlockedAttempts
                .Select(a => new BlockedAttemptLogDto
                {
                    IpAddress = a.IpAddress,
                    Timestamp = a.Timestamp,
                    CountryCode = a.CountryCode,
                    IsBlocked = a.IsBlocked,
                    UserAgent = a.UserAgent
                })
                .ToList();

            var totalCount = attempts.Count;
            var pagedItems = attempts.Skip((page - 1) * pageSize).Take(pageSize);

            var response = new PagedResponse<BlockedAttemptLogDto>
            {
                Page = page,
                PageSize = pageSize,
                TotalCount = totalCount,
                Items = pagedItems
            };

            _logger.LogInformation("Retrieved {TotalCount} blocked attempts for page {Page}", totalCount, page);
            return Ok(ApiResponse<PagedResponse<BlockedAttemptLogDto>>.Ok(response));
        }
    }
}
