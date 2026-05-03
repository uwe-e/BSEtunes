using AutoMapper;
using BSEtunes.Application.Services;
using BSEtunes.Contracts.DTOs.Albums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BSEtunes.Api.Controllers
{
    [ApiController]
    [Route("api/genres")]
    public class GenreController : ControllerBase
    {
        private readonly IGenreService _genreService;
        private readonly IMapper _mapper;

        public GenreController(IGenreService genreService, IMapper mapper)
        {
            _genreService = genreService;
            _mapper = mapper;
        }

        /// <summary>
        /// Lists all genres for which albums are available.
        /// </summary>
        /// <remarks>Only authenticated users with the 'tunes-users' role can access this endpoint.</remarks>
        /// <returns>A list of available genres.</returns>
        [HttpGet]
        [Authorize(Roles = "tunes-users")]
        public async Task<ActionResult<IReadOnlyList<GenreDto>>> GetAvailableGenresAsync()
        {
            var genres = await _genreService.GetAvailableGenresAsync();
            var dto = _mapper.Map<IReadOnlyList<GenreDto>>(genres);
            return Ok(dto);
        }
    }
}