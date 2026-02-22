namespace PSI.DTOs
{
    public record CreatePlaylistRequestDto(
        string Name,
        bool IsPublic,
        Guid? CurrentSongId = null
    );
}