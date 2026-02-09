using AutoMapper;
using BSEtunes.Application.Services;
using BSEtunes.Contracts.DTOs.Albums;
using BSEtunes.Contracts.DTOs.Common;
using BSEtunes.Domain.Entities;
using Microsoft.AspNetCore.Mvc;

namespace BSEtunes.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SearchController : ControllerBase
    {
        private readonly ISearchService _searchService;
        private readonly IMapper _mapper;

        public SearchController(ISearchService searchService, IMapper mapper)
        {
            _searchService = searchService;
            _mapper = mapper;
        }

        [HttpGet("albums")]
        public async Task<ActionResult<PagedResult<AlbumDto>>> SearchAlbums(
            [FromQuery] string query,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 20)
        {
            if (string.IsNullOrWhiteSpace(query))
            {
                return BadRequest("Search query cannot be empty");
            }

            if (pageNumber < 1)
            {
                return BadRequest("Page must be greater than 0");
            }

            if (pageSize < 1 || pageSize > 100)
            {
                return BadRequest("Page size must be between 1 and 100");
            }

            var pagedResult = await _searchService.GetAlbumSearchAsync(query, pageSize, pageNumber);
            
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

        [HttpGet("tracks")]
        public async Task<ActionResult<PagedResult<TrackDto>>> SearchTracks(
            [FromQuery] string query,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 20)
        {
            if (string.IsNullOrWhiteSpace(query))
            {
                return BadRequest("Search query cannot be empty");
            }

            if (pageNumber < 1)
            {
                return BadRequest("Page must be greater than 0");
            }

            if (pageSize < 1 || pageSize > 100)
            {
                return BadRequest("Page size must be between 1 and 100");
            }

            var pagedResult = await _searchService.GetTrackSearchAsync(query, pageSize, pageNumber);
            
            var dto = new PagedResultDto<TrackDto>
            {
                Items = _mapper.Map<IEnumerable<TrackDto>>(pagedResult.Items),
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
