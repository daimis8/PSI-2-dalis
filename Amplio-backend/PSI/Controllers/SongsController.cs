using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PSI.Services.Interfaces;
using System.Text.Json;

namespace PSI.Controllers
{
    [Route("songs")]
    [ApiController]
    public class SongsController : ControllerBase
    {
        private readonly ISongService _songService;

        public SongsController(ISongService songService)
        {
            _songService = songService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllSongs()
        {
            var allSongs = await _songService.GetAllSongsAsync();
            return Ok(allSongs);
        }

        [HttpGet("{songId:guid}")]
        public async Task<IActionResult> GetSongById(Guid songId)
        {
            var song = await _songService.GetSongByIdAsync(songId);
            return song is not null ? Ok(song) : NotFound();
        }

        [HttpGet("play/{songId:guid}")]
        public async Task<IActionResult> GetSongLink(Guid songId)
        {
            var song = await _songService.GetSongByIdAsync(songId);
            if (song == null)
            {
                return NotFound("Song not found");
            }

            return Ok(new { link = song.Link });
        }

        [HttpPost("upload")]
        public async Task<IActionResult> UploadSongs()
        {
            try
            {
                var songs = await _songService.ImportSongsFromFileAsync();
                return Ok(songs);
            }
            catch (FileNotFoundException ex)
            {
                Console.WriteLine($"File not found: {ex.Message}");
                return NotFound(new { error = "Songs file not found", details = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                Console.WriteLine($"Invalid operation: {ex.Message}"); 
                return BadRequest(new { error = "Failed to import songs", details = ex.Message });
            }
            catch (JsonException ex)
            {
                Console.WriteLine($"JSON error: {ex.Message}"); 
                return BadRequest(new { error = "Invalid JSON format in songs file", details = ex.Message });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"UNEXPECTED ERROR: {ex.Message}"); 
                Console.WriteLine($"Stack trace: {ex.StackTrace}"); 
                return StatusCode(500, new { error = "An unexpected error occurred", details = ex.Message });
            }
        }
    }
}
