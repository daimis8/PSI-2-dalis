using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PSI.DTOs;
using PSI.Exceptions;
using PSI.Services.Interfaces;
using System.Security.Claims;


namespace PSI.Controllers
{
    [Route("playlist")]
    [Authorize]
    [ApiController]

    public class PlaylistsController : ControllerBase
    {
        private readonly IPlaylistService _playlistService;

        public PlaylistsController(IPlaylistService playlistService)
        {
            _playlistService = playlistService;
        }

        [HttpPost]
        public async Task<IActionResult> CreatePlaylist([FromBody] CreatePlaylistRequestDto request)
        {
            var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            var playlist = await _playlistService.CreatePlaylistAsync(
                request.Name,
                request.IsPublic,
                request.CurrentSongId,
                userId
            );

            return Created($"/playlist/{playlist.Id}", new
            {
                playlist.Id,
                playlist.Name,
                playlist.IsPublic
            });
        }

        [HttpGet("{playlistId:guid}")]
        public async Task<IActionResult> GetSongsInPlaylist(Guid playlistId)
        {
            try
            {
                var songs = await _playlistService.GetSongsInPlaylistAsync(playlistId);
                return Ok(songs);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
        }
        [HttpGet("{playlistId:guid}/name")]
        public async Task<IActionResult> GetPlaylistName(Guid playlistId)
        {
            try
            {
                var playlist = await _playlistService.GetPlaylistByIdAsync(playlistId);
                return Ok(playlist.Name);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
        }

        [HttpPost("{playlistId:guid}/add/{songId:guid}")]
        public async Task<IActionResult> AddSongToPlaylist(Guid playlistId, Guid songId)
        {
            try
            {
                var songs = await _playlistService.AddSongToPlaylistAsync(playlistId, songId);
                return Ok(songs);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
        }

        [HttpDelete("{playlistId:guid}/delete/{songId:guid}")]
        public async Task<IActionResult> RemoveSongFromPlaylist(Guid playlistId, Guid songId)
        {
            try
            {
                await _playlistService.RemoveSongFromPlaylistAsync(playlistId, songId);
                return Ok();
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
        }

        [HttpPost("{playlistId:guid}/vote/{songId:guid}")]
        public async Task<IActionResult> UpvoteSongInPlaylist(Guid playlistId, Guid songId)
        {
            try
            {
                var songs = await _playlistService.UpvoteSongInPlaylistAsync(playlistId, songId);
                return Ok(songs);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
        }

        [HttpPost("{playlistId:guid}/play")]
        public async Task<IActionResult> SetCurrentSong(Guid playlistId)
        {
            try
            {
                var currentSong = await _playlistService.SetCurrentSongAsync(playlistId);
                return Ok(currentSong);
            }
            catch (PlaylistOperationException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpGet("{playlistId:guid}/current")]
        public async Task<IActionResult> GetCurrentSong(Guid playlistId)
        {
            try
            {
                var currentSong = await _playlistService.GetCurrentSongAsync(playlistId);
                return currentSong != null ? Ok(currentSong) : NoContent();
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
        }
        [HttpPost("{playlistId:guid}/visit")]
        public async Task<IActionResult> RegisterVisit(Guid playlistId)
        {
            try
            {
                await _playlistService.IncreasePlaylistPopularityAsync(playlistId);
                return Ok(new { message = "Playlist visit registered." });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
        }

        [HttpDelete("{playlistId:guid}/current")]
        public async Task<IActionResult> ClearCurrentSong(Guid playlistId)
        {
            try
            {
                await _playlistService.ClearCurrentSongAsync(playlistId);
                return Ok(new { message = "Current song cleared" });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
        }

        [HttpGet("personal")]
        public async Task<IActionResult> GetMyPlaylists()
        {
            var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            try
            {
                var playlists = await _playlistService.GetPlaylistsByOwnerAsync(userId);

                return Ok(playlists.Select(p => new
                {
                    p.Id,
                    p.Name,
                    p.IsPublic
                }));
            }
            catch (KeyNotFoundException)
            {
                return Ok(new List<object>()); 
            }
        }

    }
}
