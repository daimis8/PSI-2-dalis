using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PSI.DTOs;
using PSI.Exceptions;
using PSI.Services.Interfaces;
using System.Security.Claims;

namespace PSI.Controllers
{
    [ApiController]
    [Authorize]
    [Route("friends")]
    public class FriendsController : ControllerBase
    {
        private readonly IFriendService _friendService;

        public FriendsController(IFriendService friendService)
        {
            _friendService = friendService;
        }

        private Guid CurrentUserId =>
            Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        [HttpGet]
        public async Task<IActionResult> GetFriends() =>
            Ok(await _friendService.GetFriendsAsync(CurrentUserId));

        [HttpGet("search")]
        public async Task<IActionResult> Search([FromQuery] string q) =>
            Ok(await _friendService.SearchUsersAsync(CurrentUserId, q ?? string.Empty));

        [HttpGet("requests/incoming")]
        public async Task<IActionResult> Incoming() =>
            Ok(await _friendService.GetIncomingRequestsAsync(CurrentUserId));

        [HttpGet("requests/outgoing")]
        public async Task<IActionResult> Outgoing() =>
            Ok(await _friendService.GetOutgoingRequestsAsync(CurrentUserId));

        [HttpPost("requests")]
        public async Task<IActionResult> SendRequest([FromBody] SendFriendRequestDto body)
        {
            try
            {
                var dto = await _friendService.SendRequestAsync(CurrentUserId, body.ReceiverId);
                return Created($"/friends/requests/{dto.Id}", dto);
            }
            catch (SelfFriendRequestException ex) { return BadRequest(new { message = ex.Message }); }
            catch (DuplicateFriendRequestException ex) { return Conflict(new { message = ex.Message }); }
            catch (AlreadyFriendsException ex) { return Conflict(new { message = ex.Message }); }
            catch (BlockedRelationshipException ex) { return BadRequest(new { message = ex.Message }); }
            catch (FriendRequestRateLimitException ex) { return StatusCode(429, new { message = ex.Message }); }
            catch (KeyNotFoundException ex) { return NotFound(new { message = ex.Message }); }
        }

        [HttpPost("requests/{id:guid}/accept")]
        public async Task<IActionResult> Accept(Guid id)
        {
            try { return Ok(await _friendService.AcceptRequestAsync(CurrentUserId, id)); }
            catch (FriendRequestNotFoundException ex) { return NotFound(new { message = ex.Message }); }
            catch (BlockedRelationshipException ex) { return BadRequest(new { message = ex.Message }); }
        }

        [HttpPost("requests/{id:guid}/decline")]
        public async Task<IActionResult> Decline(Guid id)
        {
            try { await _friendService.DeclineRequestAsync(CurrentUserId, id); return NoContent(); }
            catch (FriendRequestNotFoundException ex) { return NotFound(new { message = ex.Message }); }
        }

        [HttpDelete("requests/{id:guid}")]
        public async Task<IActionResult> Cancel(Guid id)
        {
            try { await _friendService.CancelRequestAsync(CurrentUserId, id); return NoContent(); }
            catch (FriendRequestNotFoundException ex) { return NotFound(new { message = ex.Message }); }
        }

        [HttpDelete("{friendId:guid}")]
        public async Task<IActionResult> Remove(Guid friendId)
        {
            try { await _friendService.RemoveFriendAsync(CurrentUserId, friendId); return NoContent(); }
            catch (FriendshipNotFoundException ex) { return NotFound(new { message = ex.Message }); }
        }

        [HttpPost("blocks/{userId:guid}")]
        public async Task<IActionResult> Block(Guid userId)
        {
            try { await _friendService.BlockUserAsync(CurrentUserId, userId); return NoContent(); }
            catch (ArgumentException ex) { return BadRequest(new { message = ex.Message }); }
            catch (KeyNotFoundException ex) { return NotFound(new { message = ex.Message }); }
        }

        [HttpDelete("blocks/{userId:guid}")]
        public async Task<IActionResult> Unblock(Guid userId)
        {
            await _friendService.UnblockUserAsync(CurrentUserId, userId);
            return NoContent();
        }
    }
}
