import AlbumLeaderboard from '../components/albumLeaderboard';
import PlaylistLeaderboard from '../components/playlistLeaderboard';
import "../css/Leaderboardspage.css"
function Leaderboard() {
    return (
        <>
        <div className="content-leaderboard-page">
            <div className="center-side">
                    <div  className = "leaderboard-content-1">
                      <AlbumLeaderboard/>
                    </div>
                    <br></br>
                    <div  className = "leaderboard-content-2">
                      <PlaylistLeaderboard/>
                    </div>
            </div>
        </div>
        </>
    );
}

export default Leaderboard;