using AutoMapper;
using BSEtunes.Application.DTOs;
using BSEtunes.Application.Services;
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
        /// Retrieves a list of featured albums, limited to the specified number of results.
        /// </summary>
        /// <remarks>This endpoint is accessible only to users in the "tunes-users" role. The results are
        /// ordered according to the service's featured album criteria.</remarks>
        /// <param name="limit">The maximum number of featured albums to return. Must be a positive integer. The default value is 10.</param>
        /// <returns>An <see cref="ActionResult{T}">ActionResult</see> containing a collection of <see cref="AlbumDto"/> objects
        /// representing the featured albums. Returns an empty collection if no featured albums are available.</returns>
        [HttpGet("featured")]
        [Authorize(Roles = "tunes-users")]
        public async Task<ActionResult<IEnumerable<AlbumDto>>> GetFeaturedAlbums([FromQuery] int limit = 10)
        {
            var albums = await _service.GetFeaturedAlbumsAsync(limit);
            var dto = _mapper.Map<IEnumerable<AlbumDto>>(albums);
            return Ok(dto);
        }
    }
}
