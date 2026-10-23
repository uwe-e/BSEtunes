using AutoMapper;
using BSEtunes.Api.Extensions;
using BSEtunes.Application.Services;
using BSEtunes.Contracts.DTOs.History;
using BSEtunes.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BSEtunes.Api.Controllers
{
    /// <summary>
    /// Provides API endpoints for managing play history.
    /// </summary>
    [ApiController]
    [Route("api/history")]
    public class HistoryController : ControllerBase
    {
        private readonly IHistoryService _service;
        private readonly IMapper _mapper;

        /// <summary>
        /// Constructor for HistoryController
        /// </summary>
        /// <param name="service">The service that manages history data</param>
        /// <param name="mapper">The mapper that maps to the appropriate DTO</param>
        public HistoryController(IHistoryService service, IMapper mapper)
        {
            _service = service;
            _mapper = mapper;
        }

        /// <summary>
        /// Creates a new play history record.
        /// </summary>
        /// <remarks>Records when a track was played by a user.</remarks>
        /// <param name="createDto">The history details to create</param>
        /// <returns>The created history record</returns>
        [HttpPost]
        [Authorize(Roles = "tunes-users")]
        public async Task<ActionResult<HistoryDto>> CreateHistory([FromBody] CreateHistoryDto createDto)
        {
            if (createDto == null)
            {
                return BadRequest("History data is required.");
            }

            var userEmail = User.GetUserEmail();

            if (string.IsNullOrEmpty(userEmail))
            {
                return Unauthorized();
            }

            var history = new HistoryEntity
            {
                Owner = userEmail,
            };

            var entity = _mapper.Map(createDto, history);
            var createdEntity = await _service.AddPlayHistoryAsync(entity);
            var dto = _mapper.Map<HistoryDto>(createdEntity);

            return CreatedAtAction(nameof(GetHistoryById), new { id = dto.Id }, dto);
        }

        /// <summary>
        /// Retrieves a specific history record by its ID.
        /// </summary>
        /// <param name="id">The history record ID</param>
        /// <returns>The history record</returns>
        [HttpGet]
        [Authorize(Roles = "tunes-users")]
        [Route("{id:int}")]
        public async Task<ActionResult<HistoryDto>> GetHistoryById(int id)
        {
            var entity = await _service.GetHistoryByIdAsync(id);
            if (entity == null)
            {
                return NotFound();
            }

            var dto = _mapper.Map<HistoryDto>(entity);
            return Ok(dto);
        }

        /// <summary>
        /// Retrieves the play history for a specific user.
        /// </summary>
        /// <param name="owner">The username/owner</param>
        /// <param name="limit">Maximum number of records to return (default: 50)</param>
        /// <returns>A collection of history records for the user</returns>
        [HttpGet]
        [Authorize(Roles = "tunes-users")]
        [Route("user/{owner}")]
        public async Task<ActionResult<IEnumerable<HistoryDto>>> GetUserHistory(
            string owner,
            [FromQuery] int limit = 50)
        {
            if (string.IsNullOrWhiteSpace(owner))
            {
                return BadRequest("Owner is required.");
            }

            if (limit < 1 || limit > 500)
            {
                return BadRequest("Limit must be between 1 and 500.");
            }

            var entities = await _service.GetUserHistoryAsync(owner, limit);
            var dtos = _mapper.Map<IEnumerable<HistoryDto>>(entities);

            return Ok(dtos);
        }

        /// <summary>
        /// Retrieves the most recent play history across all users.
        /// </summary>
        /// <param name="limit">Maximum number of records to return (default: 50)</param>
        /// <returns>A collection of recent history records</returns>
        [HttpGet]
        [Authorize(Roles = "tunes-users")]
        [Route("recent")]
        public async Task<ActionResult<IEnumerable<HistoryDto>>> GetRecentHistory(
            [FromQuery] int limit = 50)
        {
            if (limit < 1 || limit > 500)
            {
                return BadRequest("Limit must be between 1 and 500.");
            }

            var entities = await _service.GetRecentHistoryAsync(limit);
            var dtos = _mapper.Map<IEnumerable<HistoryDto>>(entities);

            return Ok(dtos);
        }
    }
}