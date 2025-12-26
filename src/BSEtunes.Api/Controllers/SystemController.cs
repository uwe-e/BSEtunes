using BSEtunes.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace BSEtunes.Api.Controllers
{
    /// <summary>
    /// Provides API endpoints for system health and status checks.
    /// </summary>
    [ApiController]
    [Route("api/system")]
    public class SystemController : Controller
    {
        private readonly ISystemService _systemService;

        /// <summary>
        /// Constructor for SystemController
        /// </summary>
        /// <param name="systemService">The service for system health and status checks</param>
        public SystemController(ISystemService systemService)
        {
            _systemService = systemService;
        }

        /// <summary>
        /// Checks if the host system including the database is accessible and ready.
        /// </summary>
        /// <returns>Returns 200 OK if the system is accessible, otherwise 503 Service Unavailable.</returns>
        [HttpGet]
        [Route("is-host-accessible")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
        public async Task<IActionResult> IsHostAccessible()
        {
            var isDatabaseAccessible = await _systemService.IsDatabaseAccessibleAsync();
            if (!isDatabaseAccessible)
            {
                return StatusCode(StatusCodes.Status503ServiceUnavailable, new
                {
                    status = "unavailable",
                    message = "Database is not accessible"
                });
            }
            return Ok(isDatabaseAccessible);
            //return Ok(new
            //{
            //    status = "ok",
            //    message = "System is accessible and ready"
            //});
        }
    }
}