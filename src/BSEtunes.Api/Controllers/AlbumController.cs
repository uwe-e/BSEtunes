using AutoMapper;
using BSEtunes.Application.DTOs;
using BSEtunes.Application.Services;
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
        /// <param name="service">The service that conains the data</param>
        /// <param name="mapper">The mapper that maps to the approbiate DTO</param>
        public AlbumController(IAlbumService service, IMapper mapper)
        {
            _service = service;
            _mapper = mapper;
        }
        /// <summary>
        /// Get an album by its Id
        /// </summary>
        /// <param name="id">The Id</param>
        /// <returns>The album</returns>
        [HttpGet]
        [Route("{id:int}")]
        public async Task<ActionResult<AlbumDto>> GetAlbumById(int id)
        {
            var album = await _service.GetAlbumByIdAsync(id);
            var dto = _mapper.Map<AlbumDto>(album);
            return Ok(album);
        }
    }
}
