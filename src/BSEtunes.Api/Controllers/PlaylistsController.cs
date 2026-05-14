using AutoMapper;
using BSEtunes.Api.Extensions;
using BSEtunes.Application.Services;
using BSEtunes.Contracts.DTOs.Common;
using BSEtunes.Contracts.DTOs.Playlists;
using BSEtunes.Domain.Entities;
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
    /// <remarks>
    /// Initializes a new instance of the PlaylistsController class with the specified playlist service and object
    /// mapper.
    /// </remarks>
    /// <param name="service">The service used to manage playlist operations. Cannot be null.</param>
    /// <param name="mapper">The mapper used to convert between domain models and data transfer objects. Cannot be null.</param>
    [ApiController]
    [Route("api/playlists")]
    public class PlaylistsController(IPlaylistsService service, IMapper mapper) : ControllerBase   
    {
        /// <summary>
        /// Retrieves a paged list of playlists owned by the specified user.
        /// </summary>
        /// <remarks>Pagination metadata is included in the response headers: X-Total-Count,
        /// X-Page-Number, X-Page-Size, and X-Total-Pages. Only authenticated users can
        /// access this endpoint.</remarks>
        /// <param name="pageNumber">The page number to retrieve. Must be greater than 0. Defaults to 1.</param>
        /// <param name="pageSize">The number of playlists to include per page. Must be between 1 and 100. Defaults to 10.</param>
        /// <returns>An ActionResult containing a PagedResultDto of PlaylistDto objects for the specified owner and page. Returns
        /// a BadRequest result if pagination parameters are out of range.</returns>
        [HttpGet]
        [Authorize(Roles = "tunes-users")]
        [Route("paged")]
        public async Task<ActionResult<PagedResultDto<PlaylistDto>>> GetPagedPlaylistsByOwnerAsync(
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

            var userEmail = User.GetUserEmail();
            
            if (string.IsNullOrEmpty(userEmail))
            {
                return Unauthorized();
            }

            var pagedResult = await service.GetPagedPlaylistsByOwnerAsync(userEmail, pageNumber, pageSize);
            // Map to DTO
            var dto = new PagedResultDto<PlaylistDto>
            {
                Items = mapper.Map<IEnumerable<PlaylistDto>>(pagedResult.Items),
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
        /// <remarks>The owner is determined from the authenticated user's email claim. Only users that are in a privileged role are authorized to
        /// access this endpoint.</remarks>
        /// <param name="playlistId">The unique identifier of the playlist.</param>
        /// <returns>Returns the playlist matching the specified identifier and owned by the authenticated user. Returns
        /// Unauthorized if the user claim is missing, or NotFound if the playlist does not exist or does not belong to the user.</returns>
        [HttpGet("{playlistId:int}", Name = "GetPlaylistById")]
        [Authorize(Roles = "tunes-users")]
        public async Task<ActionResult<PlaylistDto>> GetPlaylistByIdAsync(int playlistId)
        {
            var userEmail = User.GetUserEmail();

            if (string.IsNullOrEmpty(userEmail))
            {
                return Unauthorized();
            }

            var playlist = await service.GetPlaylistByOwnerAndIdAsync(userEmail, playlistId);
            if (playlist == null)
            {
                return NotFound();
            }
            var dto = mapper.Map<PlaylistDto>(playlist);
            return Ok(dto);
        }
        /// <summary>
        /// Retrieves a paged list of entries for the specified playlist.
        /// </summary>
        /// <remarks>Pagination metadata is included in the response headers: X-Total-Count,
        /// X-Page-Number, X-Page-Size, and X-Total-Pages. Only authenticated users can
        /// access this endpoint.</remarks>
        /// <param name="playlistId">The unique identifier of the playlist for which entries are requested.</param>
        /// <param name="pageNumber">The page number to retrieve. Must be greater than or equal to 1. Defaults to 1.</param>
        /// <param name="pageSize">The maximum number of entries to include in a single page. Must be greater than 0. Defaults to 50.</param>
        /// <returns>An ActionResult containing a paged result of playlist entries. Returns an unauthorized response if the user
        /// is not authenticated.</returns>
        [HttpGet("{playlistId:int}/entries")]
        [Authorize(Roles = "tunes-users")]
        public async Task<ActionResult<PagedResultDto<PlaylistEntryDto>>> GetPagedPlaylistEntriesByIdAsync(
            int playlistId,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 50)
        {
            var userEmail = User.GetUserEmail();

            if (string.IsNullOrEmpty(userEmail))
            {
                return Unauthorized();
            }

            // Validate pagination parameters
            if (pageNumber < 1)
            {
                return BadRequest("Page number must be greater than 0.");
            }

            if (pageSize < 1 || pageSize > 1000)
            {
                return BadRequest("Page size must be between 1 and 1000.");
            }

            var pagedResult = await service.GetPagedPlaylistEntriesByIdAsync(playlistId, userEmail, pageNumber, pageSize);
            // Map to DTO
            var dto = new PagedResultDto<PlaylistEntryDto>
            {
                Items = mapper.Map<IEnumerable<PlaylistEntryDto>>(pagedResult.Items),
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
        /// Retrieves all track identifiers for the specified playlist.
        /// </summary>
        /// <remarks>Only authenticated users with the 'tunes-users' role can access this endpoint. 
        /// The user must be the owner of the playlist.</remarks>
        /// <param name="playlistId">The unique identifier of the playlist.</param>
        /// <param name="randomize">Indicates whether to randomize the order of track IDs.</param>
        /// <returns>Returns a list of track identifiers for the specified playlist. Returns 
        /// Unauthorized if the user claim is missing.</returns>
        [HttpGet("{playlistId:int}/trackids")]
        [Authorize(Roles = "tunes-users")]
        public async Task<ActionResult<List<int>>> GetTrackIdsByPlaylistIdAsync(
            int playlistId,
            [FromQuery] bool randomize = false)
        {
            var userEmail = User.GetUserEmail();

            if (string.IsNullOrEmpty(userEmail))
            {
                return Unauthorized();
            }
            
            var trackIds = await service.GetTrackIdsByPlaylistIdAsync(playlistId, randomize);
            
            return Ok(trackIds);
        }
        /// <summary>
        /// Creates a new playlist for the authenticated user.
        /// </summary>
        /// <remarks>Only authenticated users with the 'tunes-users' role can access this endpoint.
        /// The playlist will be created with the authenticated user as the owner.</remarks>
        /// <param name="createPlaylistDto">The data transfer object containing playlist creation details.</param>
        /// <returns>Returns the created playlist with a 201 Created status and location header. Returns
        /// Unauthorized if the user claim is missing, or BadRequest if the model validation fails.</returns>
        [HttpPost]
        [Authorize(Roles = "tunes-users")]
        public async Task<ActionResult<PlaylistDto>> CreatePlaylistAsync([FromBody] CreatePlaylistDto createPlaylistDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var userEmail = User.GetUserEmail();

            if (string.IsNullOrEmpty(userEmail))
            {
                return Unauthorized();
            }

            var playlist = new PlaylistEntity
            {
                Owner = userEmail,
                Guid = Guid.NewGuid()
            };
            mapper.Map(createPlaylistDto, playlist);
            var createdPlaylist = await service.CreatePlaylistAsync(playlist);
            var dto = mapper.Map<PlaylistDto>(createdPlaylist);

            return CreatedAtRoute(
                "GetPlaylistById",
                new { playlistId = createdPlaylist.Id },
                dto);
        }
        /// <summary>
        /// Deletes a playlist by its identifier for the authenticated user.
        /// </summary>
        /// <remarks>Only authenticated users with the 'tunes-users' role can access this endpoint.
        /// The user must be the owner of the playlist to delete it.</remarks>
        /// <param name="playlistId">The unique identifier of the playlist to delete.</param>
        /// <returns>Returns NoContent (204) if the playlist was successfully deleted. Returns
        /// Unauthorized if the user claim is missing, or NotFound if the playlist does not exist or does not belong to the user.</returns>
        [HttpDelete("{playlistId:int}")]
        [Authorize(Roles = "tunes-users")]
        public async Task<ActionResult> DeletePlaylistAsync(int playlistId)
        {
            var userEmail = User.GetUserEmail();

            if (string.IsNullOrEmpty(userEmail))
            {
                return Unauthorized();
            }

            var playlist = await service.GetPlaylistByOwnerAndIdAsync(userEmail, playlistId);
            if (playlist == null)
            {
                return NotFound();
            }

            var deleted = await service.DeletePlaylistAsync(playlistId, userEmail);
            if (!deleted)
            {
                return NotFound();
            }
            
            return NoContent();
        }
        /// <summary>
        /// Appends one or more track entries to an existing playlist.
        /// </summary>
        /// <remarks>Only authenticated users with the 'tunes-users' role can access this endpoint.
        /// The user must be the owner of the playlist to append entries.</remarks>
        /// <param name="playlistId">The unique identifier of the playlist to append entries to.</param>
        /// <param name="appendEntriesDto">The data transfer object containing track IDs to append.</param>
        /// <returns>Returns NoContent (204) if entries were successfully appended. Returns
        /// Unauthorized if the user claim is missing, NotFound if the playlist does not exist or does not belong to the user,
        /// or BadRequest if the model validation fails.</returns>
        [HttpPost("{playlistId:int}/entries")]
        [Authorize(Roles = "tunes-users")]
        public async Task<ActionResult> AppendPlaylistEntriesAsync(
            int playlistId,
            [FromBody] AppendPlaylistEntriesDto appendEntriesDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var userEmail = User.GetUserEmail();

            if (string.IsNullOrEmpty(userEmail))
            {
                return Unauthorized();
            }

            var playlist = await service.GetPlaylistByOwnerAndIdAsync(userEmail, playlistId);
            if (playlist == null)
            {
                return NotFound();
            }

            await service.AppendPlaylistEntriesAsync(playlistId, appendEntriesDto.TrackIds);

            return NoContent();
        }
        /// <summary>
        /// Deletes a specific entry from a playlist.
        /// </summary>
        /// <remarks>Only authenticated users with the 'tunes-users' role can access this endpoint.
        /// The user must be the owner of the playlist to delete entries from it.</remarks>
        /// <param name="playlistId">The unique identifier of the playlist.</param>
        /// <param name="entryId">The unique identifier of the playlist entry to delete.</param>
        /// <returns>Returns NoContent (204) if the entry was successfully deleted. Returns
        /// Unauthorized if the user claim is missing, NotFound if the playlist or entry does not exist or does not belong to the user.</returns>
        [HttpDelete("{playlistId:int}/entries/{entryId:int}")]
        [Authorize(Roles = "tunes-users")]
        public async Task<ActionResult> DeletePlaylistEntryAsync(int playlistId, int entryId)
        {
            var userEmail = User.GetUserEmail();

            if (string.IsNullOrEmpty(userEmail))
            {
                return Unauthorized();
            }

            var playlist = await service.GetPlaylistByOwnerAndIdAsync(userEmail, playlistId);
            if (playlist == null)
            {
                return NotFound();
            }

            var deletedCount = await service.DeletePlaylistEntriesAsync(playlistId, [entryId], userEmail);
            if (deletedCount == 0)
            {
                return NotFound();
            }

            return NoContent();
        }

        /// <summary>
        /// Deletes multiple entries from a playlist.
        /// </summary>
        /// <remarks>Only authenticated users with the 'tunes-users' role can access this endpoint.
        /// The user must be the owner of the playlist to delete entries from it.
        /// Returns the number of successfully deleted entries.</remarks>
        /// <param name="playlistId">The unique identifier of the playlist.</param>
        /// <param name="deleteEntriesDto">The data transfer object containing entry IDs to delete.</param>
        /// <returns>Returns Ok with the count of deleted entries if successful. Returns
        /// Unauthorized if the user claim is missing, NotFound if the playlist does not exist or does not belong to the user,
        /// or BadRequest if the model validation fails or no entries were deleted.</returns>
        [HttpDelete("{playlistId:int}/entries")]
        [Authorize(Roles = "tunes-users")]
        public async Task<ActionResult<int>> DeletePlaylistEntriesAsync(
            int playlistId,
            [FromBody] DeletePlaylistEntriesDto deleteEntriesDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var userEmail = User.GetUserEmail();

            if (string.IsNullOrEmpty(userEmail))
            {
                return Unauthorized();
            }

            var playlist = await service.GetPlaylistByOwnerAndIdAsync(userEmail, playlistId);
            if (playlist == null)
            {
                return NotFound();
            }

            var deletedCount = await service.DeletePlaylistEntriesAsync(playlistId, deleteEntriesDto.EntryIds, userEmail);

            if (deletedCount == 0)
            {
                return NotFound("No entries were found or deleted.");
            }

            return Ok(deletedCount);
        }
        /// <summary>
        /// Retrieves all playlists owned by the authenticated user.
        /// </summary>
        /// <remarks>Only authenticated users with the 'tunes-users' role can access this endpoint.
        /// Returns a lightweight collection of playlists without entries or cover album details.</remarks>
        /// <returns>Returns a collection of playlist summaries owned by the authenticated user. Returns
        /// Unauthorized if the user claim is missing.</returns>
        [HttpGet("all")]
        [Authorize(Roles = "tunes-users")]
        public async Task<ActionResult<IReadOnlyList<PlaylistSummaryDto>>> GetAllPlaylistsByOwnerAsync()
        {
            var userEmail = User.GetUserEmail();
            
            if (string.IsNullOrEmpty(userEmail))
            {
                return Unauthorized();
            }

            var playlists = await service.GetPlaylistsByOwnerAsync(userEmail);
            var dto = mapper.Map<IReadOnlyList<PlaylistSummaryDto>>(playlists);
            
            return Ok(dto);
        }
        /// <summary>
        /// Updates the order of playlist entries by providing a complete sorted list of entry IDs.
        /// </summary>  
        /// <remarks>Only authenticated users with the 'tunes-users' role can access this endpoint.
        /// The user must be the owner of the playlist to reorder entries. All entry IDs in the request
        /// will have their sort order updated based on their position in the provided list.</remarks>
        /// <param name="playlistId">The unique identifier of the playlist.</param>
        /// <param name="reorderEntriesDto">The data transfer object containing entry IDs in the desired order.</param>
        /// <returns>Returns NoContent (204) if the reordering was successful. Returns
        /// Unauthorized if the user claim is missing, NotFound if the playlist does not exist or does not belong to the user,
        /// or BadRequest if the model validation fails.</returns>
        [HttpPut("{playlistId:int}/entries/reorder")]
        [Authorize(Roles = "tunes-users")]
        public async Task<ActionResult> ReorderPlaylistEntriesAsync(
            int playlistId,
            [FromBody] ReorderPlaylistEntriesDto reorderEntriesDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var userEmail = User.GetUserEmail();

            if (string.IsNullOrEmpty(userEmail))
            {
                return Unauthorized();
            }

            var playlist = await service.GetPlaylistByOwnerAndIdAsync(userEmail, playlistId);
            if (playlist == null)
            {
                return NotFound();
            }

            var updated = await service.ReorderPlaylistEntriesAsync(playlistId, reorderEntriesDto.EntryIds, userEmail);
            if (!updated)
            {
                return BadRequest("Failed to reorder entries. Ensure all entry IDs are valid.");
            }

            return NoContent();
        }
    }

}
