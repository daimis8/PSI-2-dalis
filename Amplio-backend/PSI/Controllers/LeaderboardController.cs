using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PSI.Services.Interfaces;

namespace PSI.Controllers
{
    [Route("leaderboard")]
    [ApiController]
    public class LeaderboardController : ControllerBase
    {
        private readonly ILeaderboardService _leaderboardService;
        
        public LeaderboardController(ILeaderboardService leaderboardService)
        {
            _leaderboardService = leaderboardService;
        }

        [HttpGet("playlists")]
        public async Task<IActionResult> GetPlaylistLeaderboard([FromQuery] int topN = 10)
        {
            var leaderboard = await _leaderboardService.GetPlaylistLeaderboardAsync(topN);
            return Ok(leaderboard);
        }

        [HttpGet("albums")]
        public async Task<IActionResult> GetAlbumLeaderboard([FromQuery] int topN = 10)
        {
            var leaderboard = await _leaderboardService.GetAlbumLeaderboardAsync(topN);
            return Ok(leaderboard);
        }
    }
}
