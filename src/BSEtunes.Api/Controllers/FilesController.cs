using BSEtunes.Application.Services;
using BSEtunes.Infrastructure.Configuration;
using BSEtunes.Infrastructure.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.Options;
using System.Runtime.Versioning;

namespace BSEtunes.Api.Controllers
{
    /// <summary>
    /// Provides API endpoints for accessing and streaming audio files associated with tracks.
    /// </summary>
    /// <remarks>This controller is intended for use in environments where access to audio files is managed
    /// via impersonation and file share configuration. All endpoints require authorization and are supported only on
    /// Windows platforms. The controller relies on dependency injection for its collaborators and is designed for use
    /// within ASP.NET Core applications.</remarks>
    /// <param name="logger">The logger used to record diagnostic and operational information for this controller.</param>
    /// <param name="fileAccessor">The file accessor used to perform file operations with impersonation support.</param>
    /// <param name="trackService">The service used to retrieve track metadata and information.</param>
    /// <param name="fileShareOptions">The configuration options specifying file share settings, such as the base share path.</param>
    [ApiController]
    [Route("api/[controller]")]
    public class FilesController(
        ILogger<FilesController> logger,
        ImpersonatedFileAccessor fileAccessor,
        ITrackService trackService,
        IOptions<FileShareOptions> fileShareOptions) : ControllerBase
    {
        private readonly ImpersonatedFileAccessor _fileAccessor = fileAccessor;
        private readonly ILogger<FilesController> _logger = logger;
        private readonly ITrackService _trackService = trackService;
        private readonly IOptions<FileShareOptions> _fileShareOptions = fileShareOptions;
        private readonly FileExtensionContentTypeProvider _contentTypeProvider = new FileExtensionContentTypeProvider();

        /// <summary>
        /// Streams an audio file by track GUID.
        /// </summary>
        /// <param name="trackGuid">The unique identifier for the track.</param>
        /// <returns>A stream containing the audio file.</returns>
        [HttpGet]
        [HttpHead]
        [Authorize(Roles = "tunes-users")]
        [Route("audio/{trackGuid:Guid}")]
        [SupportedOSPlatform("windows")]
        public async Task<IActionResult> GetAudioFile(Guid trackGuid)
        {
            try
            {
                // Get track information from database
                var track = await _trackService.GetTrackByGuidAsync(trackGuid);
                if (track == null)
                {
                    _logger.LogWarning("Track not found: {TrackGuid}", trackGuid);
                    return NotFound($"Track with GUID {trackGuid} not found.");
                }

                if (string.IsNullOrEmpty(track.FilePath))
                {
                    _logger.LogWarning("Track has no file path: {TrackGuid}", trackGuid);
                    return NotFound("Track file path is not available.");
                }

                // Combine SharePath with relative FilePath
                var fullFilePath = Path.IsPathRooted(track.FilePath)
                    ? track.FilePath
                    : Path.Combine(_fileShareOptions.Value.SharePath, track.FilePath);

                return await _fileAccessor.ExecuteAsync(() => Task.Run(() =>
                {
                    if (!System.IO.File.Exists(fullFilePath))
                    {
                        _logger.LogError("File not found at path: {FilePath}", fullFilePath);
                        return (IActionResult)NotFound("Audio file not found on server.");
                    }

                    // Determine content type
                    var extension = Path.GetExtension(track.FilePath);
                    if (!_contentTypeProvider.TryGetContentType(extension, out var contentType))
                    {
                        contentType = "application/octet-stream";
                    }

                    // Set response headers
                    Response.Headers.ContentDisposition = $"inline; filename=\"{Path.GetFileName(track.FilePath)}\"";
                    Response.Headers.AcceptRanges = "bytes";

                    // For HEAD requests, return only headers without opening the file
                    if (HttpContext.Request.Method == HttpMethods.Head)
                    {
                        var fileInfo = new FileInfo(fullFilePath);
                        Response.Headers.ContentLength = fileInfo.Length;
                        Response.ContentType = contentType;
                        return Ok();
                    }

                    var fileStream = new FileStream(
                        fullFilePath,
                        FileMode.Open,
                        FileAccess.Read,
                        FileShare.Read,
                        bufferSize: 4096,
                        useAsync: true);

                    return File(fileStream, contentType, enableRangeProcessing: true);
                }));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error streaming track file for track {TrackGuid}", trackGuid);
                return StatusCode(500, "An error occurred while streaming the track file.");
            }
        }
    }
}
