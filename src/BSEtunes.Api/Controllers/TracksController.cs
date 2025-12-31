using AutoMapper;
using BSEtunes.Application.DTOs;
using BSEtunes.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BSEtunes.Api.Controllers
{
    /// <summary>
    /// Provides API endpoints for managing tracks.
    /// </summary>
    /// <remarks>This controller handles HTTP requests related to track operations, such as retrieving tracks
    /// details by ID.</remarks>
    [ApiController]
    [Route("api/tracks")]
    public class TracksController : Controller
    {
        private readonly ITrackService _trackService;
        private readonly IMapper _mapper;

        /// <summary>
        /// Initializes a new instance of the TracksController class with the specified track service.
        /// </summary>
        /// <param name="trackService">The service used to manage and retrieve track data. Cannot be null.</param>
        public TracksController(ITrackService trackService, IMapper mapper)
        {
            _trackService = trackService;
            _mapper = mapper;
        }

        /// <summary>
        /// Retrieves the details of a track by its unique identifier.
        /// </summary>
        /// <param name="id">The unique identifier of the track.</param>
        /// <returns>An <see cref="ActionResult{TrackDto}"/> containing the track details if found; otherwise, a NotFound result.</returns>
        [HttpGet("{id}")]
        [Authorize(Roles = "tunes-users")]
        public async Task<ActionResult<TrackDto>> GetTrack(int id)
        {
            var track = await _trackService.GetTrackByIdAsync(id);
            if (track == null)
            {
                return NotFound();
            }
            var dto = _mapper.Map<TrackDto>(track);
            return Ok(dto);
        }

        /// <summary>
        /// Retrieves a list of track IDs filtered by genre.
        /// </summary>
        /// <param name="genreid">The ID of the genre to filter tracks by. Optional - if not provided, returns all tracks.</param>
        /// <returns>A collection of track IDs that belong to the specified genre, or all tracks if no genre is specified.</returns>            [HttpGet]
        [HttpGet("genre/{genreid:int?}")]
        [Authorize(Roles = "tunes-users")]
        public async Task<IActionResult> GetTrackIdsByFilter(int? genreid = null)
        {
            var trackIds = await _trackService.GetTrackIdsByFilter(genreid);
            if (trackIds == null)
            {
                return NotFound();
            }
            return Ok(trackIds);
        }
    }
}
