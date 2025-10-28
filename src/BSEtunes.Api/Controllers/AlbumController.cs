using AutoMapper;
using BSEtunes.Application.DTOs;
using BSEtunes.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace BSEtunes.Api.Controllers
{
    [ApiController]
    [Route("api/albums")]
    public class AlbumController : ControllerBase
    {
        private readonly IAlbumService _service;
        private readonly IMapper _mapper;

        public AlbumController(IAlbumService service, IMapper mapper)
        {
            _service = service;
            _mapper = mapper;
        }

        [HttpGet]
        [Route("{id:int}")]
        public async Task<IActionResult> GetAlbumById(int id)
        {
            var album = await _service.GetAlbumByIdAsync(id);
            var dto = _mapper.Map<AlbumDto>(album);
            return Ok(album);
        }
    }
}
