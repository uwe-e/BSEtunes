using AutoMapper;
using BSEtunes.Application.DTOs;
using BSEtunes.Application.Services;
using BSEtunes.Domain.Entities;
using BSEtunes.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BSEtunes.Api.Controllers
{
    /// <summary>
    /// Provides API endpoints for managing albums.
    /// </summary>
    /// <remarks>This controller handles HTTP requests related to album operations, such as retrieving album
    /// details by ID.</remarks>
    [ApiController]
    [Route("api/albums")]
    public class AlbumController : ControllerBase
    {
        private readonly IAlbumService _service;
        private readonly IMapper _mapper;

        /// <summary>
        /// Constructor for AlbumController
        /// </summary>
        /// <param name="service">The service that contains the data</param>
        /// <param name="mapper">The mapper that maps to the approbiate DTO</param>
        public AlbumController(IAlbumService service, IMapper mapper)
        {
            _service = service;
            _mapper = mapper;
        }
        /// <summary>
        /// Retrieves the details of an album by its Id
        /// </summary>
        /// <remarks>This endpoint requires the caller to be authorized with the 'tunes-users' role.</remarks>
        /// <param name="id">The Id</param>
        /// <returns>The album</returns>
        [HttpGet]
        [Authorize(Roles = "tunes-users")]
        [Route("{id:int}")]
        public async Task<ActionResult<AlbumDto>> GetAlbumById(int id)
        {
            var album = await _service.GetAlbumByIdAsync(id);
            if (album == null)
            {
                return NotFound();
            }
            var dto = _mapper.Map<AlbumDto>(album);
            return Ok(dto);
        }
        /// <summary>
        /// Retrieves the album cover image for the specified album identifier.
        /// </summary>
        /// <remarks>The image is returned with its MIME type.</remarks>
        /// <param name="albumId">A unique identifier for the album.</param>
        /// <param name="asThumbnail">true to retrieve a thumbnail version of the cover image; otherwise, false to retrieve the full-size image.
        /// The default is false.</param>
        /// <returns>An image file containing the album cover in JPEG format. Returns a thumbnail or full-size image based on the
        /// value of asThumbnail.</returns>
        [HttpGet]
        //[Authorize(Roles = "tunes-users")]
        [Route("{albumId:Guid}/cover/{asThumbnail:bool=false}")]
        public async Task<ActionResult> GetAlbumCoverImage(Guid albumId, bool asThumbnail = false)
        {
            var coverImage = await _service.GetAlbumCoverImageAsync(albumId, asThumbnail);
            if (coverImage == null)
            {
                return NotFound();
            }

            string mimeType = "image/jpeg";
            if (coverImage.Extension != null)
            {
                var contentTypeProvider = new Microsoft.AspNetCore.StaticFiles.FileExtensionContentTypeProvider();
                if (contentTypeProvider.TryGetContentType(coverImage.Extension, out var contentType))
                {
                    mimeType = contentType;
                }
            }

            if (coverImage.LastModified.HasValue)
            {
                Response.Headers.LastModified = coverImage.LastModified.Value.ToUniversalTime().ToString("R");
            }
            return File(coverImage.Blob, mimeType);
        }
        /// <summary>
        /// Retrieves a sorted list of albums without filtering.
        /// </summary>
        /// <remarks>
        /// Valid sortBy values: Random, Title, TitleDesc, Artist, ArtistDesc, Year, YearDesc, Newest, NewestDesc.
        /// Values are case-insensitive.
        /// </remarks>
        /// <param name="sortBy">The sort option for ordering albums. Default is Random.</param>
        /// <param name="limit">Maximum number of albums to return. Default is 10.</param>
        /// <returns>A collection of sorted albums.</returns>
        [HttpGet]
        [Authorize(Roles = "tunes-users")]
        public async Task<ActionResult<IEnumerable<AlbumDto>>> GetSortedAlbums(
            [FromQuery] AlbumSortOption sortBy = AlbumSortOption.Random,
            [FromQuery] int limit = 10)
        {
            var albums = await _service.GetSortedAlbumsAsync(sortBy, limit);
            var dto = _mapper.Map<IEnumerable<AlbumDto>>(albums);
            return Ok(dto);
        }
        /// <summary>
        /// Retrieves a pageable, filterable, and sortable list of albums.
        /// </summary>
        /// <remarks>
        /// Valid sortBy values: Random, Title, TitleDesc, Artist, ArtistDesc, Year, YearDesc, Newest, NewestDesc.
        /// Values are case-insensitive.
        /// 
        /// Example request:
        /// GET /api/albums/paged?genre=Rock&amp;artistName=Beatles&amp;yearFrom=1960&amp;yearTo=1970&amp;sortBy=Year&amp;pageNumber=1&amp;pageSize=20
        /// </remarks>
        /// <param name="genre">Filter by genre (partial match, case-insensitive).</param>
        /// <param name="artistName">Filter by artist name (partial match, case-insensitive).</param>
        /// <param name="yearFrom">Filter by minimum album year (inclusive).</param>
        /// <param name="yearTo">Filter by maximum album year (inclusive).</param>
        /// <param name="sortBy">The sort option for ordering albums. Default is Random.</param>
        /// <param name="pageNumber">The page number to retrieve (1-based). Default is 1.</param>
        /// <param name="pageSize">Number of albums per page. Default is 10.</param>
        /// <returns>A paginated collection of filtered and sorted albums with pagination metadata.</returns>
        [HttpGet]
        [Authorize(Roles = "tunes-users")]
        [Route("paged")]
        public async Task<ActionResult<PagedResultDto<AlbumDto>>> GetPagedAlbums(
            [FromQuery] string? genre = null,
            [FromQuery] string? artistName = null,
            [FromQuery] int? yearFrom = null,
            [FromQuery] int? yearTo = null,
            [FromQuery] AlbumSortOption sortBy = AlbumSortOption.Random,
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

            // Build filter options
            var filterOptions = new AlbumFilterOptions
            {
                Genre = genre,
                ArtistName = artistName,
                YearFrom = yearFrom,
                YearTo = yearTo
            };

            // Get paged results
            var pagedResult = await _service.GetPagedAlbumsAsync(filterOptions, sortBy, pageNumber, pageSize);

            // Map to DTO
            var dto = new PagedResultDto<AlbumDto>
            {
                Items = _mapper.Map<IEnumerable<AlbumDto>>(pagedResult.Items),
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
    }
}
