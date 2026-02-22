using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PSI.DTOs;
using PSI.Services.Interfaces;

namespace PSI.Controllers
{
    [Route("albums")]
    [ApiController]
    public class AlbumController : ControllerBase
    {
        private readonly IAlbumService _albumService;
        
        public AlbumController(IAlbumService albumService)
        {
            _albumService = albumService;
        }
        
        [HttpGet]
        public async Task<IActionResult> GetAllAlbums()
        {
            var albums = await _albumService.GetAllAlbumsAsync();
            return Ok(albums);
        }

        [HttpGet("{albumId:guid}")]
        public async Task<IActionResult> GetAlbumById(Guid albumId)
        {
            var album = await _albumService.GetAlbumByIdAsync(albumId);
            return album != null ? Ok(album) : NotFound("Album not found");
        }

        [HttpPost]
        public async Task<IActionResult> CreateAlbum([FromBody] AlbumDto request)
        {
            if (string.IsNullOrWhiteSpace(request.Name) || string.IsNullOrWhiteSpace(request.Artist))
                return BadRequest("Album name and artist cannot be empty.");

            var album = await _albumService.CreateAlbumAsync(request.Name, request.Artist, request.ReleaseYear);
            return Created($"/albums/{album.Id}", new { album.Id, album.Name, album.Artist, album.ReleaseYear });
        }

        [HttpPost("{albumId:guid}/songs/{songId:guid}")]
        public async Task<IActionResult> AddSongToAlbum(Guid albumId, Guid songId)
        {
            try
            {
                await _albumService.AddSongToAlbumAsync(albumId, songId);
                return Ok(new { message = "Song added to album" });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
        }
    }
}
