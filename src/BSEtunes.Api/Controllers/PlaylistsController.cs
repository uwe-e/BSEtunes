using AutoMapper;
using BSEtunes.Application.DTOs;
using BSEtunes.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BSEtunes.Api.Controllers
{
    /// <summary>
    /// Provides API endpoints for managing and retrieving playlists, including support for paginated queries and
    /// user-based access control.
    /// </summary>
    /// <remarks>This controller requires authorization for certain endpoints and relies on dependency
    /// injection for playlist operations and object mapping. Pagination metadata is included in response headers for
    /// paged queries. All endpoints are accessible under the route 'api/playlists'.</remarks>
    [ApiController]
    [Route("api/playlists")]
    public class PlaylistsController : ControllerBase   
    {
        private readonly IPlaylistsService _service;
        private readonly IMapper _mapper;

        /// <summary>
        /// Initializes a new instance of the PlaylistsController class with the specified playlist service and object
        /// mapper.
        /// </summary>
        /// <param name="service">The service used to manage playlist operations. Cannot be null.</param>
        /// <param name="mapper">The mapper used to convert between domain models and data transfer objects. Cannot be null.</param>
        public PlaylistsController(IPlaylistsService service, IMapper mapper)
        {
            _service = service;
            _mapper = mapper;
        }

        /// <summary>
        /// Retrieves a paged list of playlists owned by the specified user.
        /// </summary>
        /// <remarks>Pagination metadata is included in the response headers: X-Total-Count,
        /// X-Page-Number, X-Page-Size, and X-Total-Pages. Only users with the 'tunes-users' role are authorized to
        /// access this endpoint.</remarks>
        /// <param name="owner">The unique identifier of the playlist owner. Can be a username or user ID. Cannot be null or empty.</param>
        /// <param name="pageNumber">The page number to retrieve. Must be greater than 0. Defaults to 1.</param>
        /// <param name="pageSize">The number of playlists to include per page. Must be between 1 and 100. Defaults to 10.</param>
        /// <returns>An ActionResult containing a PagedResultDto of PlaylistDto objects for the specified owner and page. Returns
        /// a BadRequest result if pagination parameters are out of range.</returns>
        [HttpGet]
        [Authorize(Roles = "tunes-users")]
        [Route("paged")]
        public async Task<ActionResult<PagedResultDto<PlaylistDto>>> GetPagedPlaylistsByOwnerAsync(
            [FromQuery] string owner,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10)
        {
            // Validate pagination parameters
            if (pageNumber < 1)
            {
                return BadRequest("Page number must be greater than 0.");
            }

            if (pageSize < 1 || pageSize > 100)
            {
                return BadRequest("Page size must be between 1 and 100.");
            }

            var pagedResult = await _service.GetPagedPlaylistsByOwnerAsync(owner, pageNumber, pageSize);
            // Map to DTO
            var dto = new PagedResultDto<PlaylistDto>
            {
                Items = _mapper.Map<IEnumerable<PlaylistDto>>(pagedResult.Items),
                TotalCount = pagedResult.TotalCount,
                PageNumber = pagedResult.PageNumber,
                PageSize = pagedResult.PageSize,
                TotalPages = pagedResult.TotalPages,
                HasPreviousPage = pagedResult.HasPreviousPage,
                HasNextPage = pagedResult.HasNextPage
            };

            // Add pagination headers for better API experience
            Response.Headers.Append("X-Total-Count", pagedResult.TotalCount.ToString());
            Response.Headers.Append("X-Page-Number", pagedResult.PageNumber.ToString());
            Response.Headers.Append("X-Page-Size", pagedResult.PageSize.ToString());
            Response.Headers.Append("X-Total-Pages", pagedResult.TotalPages.ToString());
            
            return Ok(dto);
        }
        /// <summary>
        /// Retrieves a playlist by its identifier for the authenticated user.
        /// </summary>
        /// <remarks>The owner is determined from the authenticated user's email claim. Only users with the 'tunes-users' role are authorized to
        /// access this endpoint.</remarks>
        /// <param name="playlistId">The unique identifier of the playlist.</param>
        /// <returns>Returns the playlist matching the specified identifier and owned by the authenticated user. Returns
        /// Unauthorized if the user claim is missing, or NotFound if the playlist does not exist or does not belong to the user.</returns>
        [HttpGet("{playlistId:int}")]
        [Authorize(Roles = "tunes-users")]
        public async Task<ActionResult<PagedResultDto<PlaylistDto>>> GetPagedPlaylistByOwnerAndIdAsync(int playlistId)
        {
            var userEmail = User.Claims
                .FirstOrDefault(c => c.Type == "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress")
                ?.Value;
            
            if (string.IsNullOrEmpty(userEmail))
            {
                return Unauthorized();
            }

            var playlist = await _service.GetPlaylistByOwnerAndIdAsync(userEmail, playlistId);
            if (playlist == null)
            {
                return NotFound();
            }
            var dto = _mapper.Map<PlaylistDto>(playlist);
            return Ok(dto);
        }
    }
}
