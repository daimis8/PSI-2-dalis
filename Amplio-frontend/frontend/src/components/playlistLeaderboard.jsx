import { useState, useEffect } from 'react';
import { getPlaylistLeaderboard } from '../services/leaderboardAPI';
import "../css/Leaderboard.css";

export default function PlaylistLeaderboard({ topN = 5 }) {
    const [leaderboard, setLeaderboard] = useState(null);
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState(null);

    useEffect(() => {
        async function fetchData() {
            try {
                setLoading(true);
                const data = await getPlaylistLeaderboard(topN);
                setLeaderboard(data);
                setError(null);
            } catch (err) {
                console.error(err);
                setError(err.message);
            } finally {
                setLoading(false);
            }
        }
        fetchData();
    }, [topN]);

    if (loading) return <div>Loading playlist leaderboard...</div>;
    if (error) return <div>Error: {error}</div>;
    if (!leaderboard || leaderboard.leaderboardItems.length === 0) {
        return <div>No public playlists yet. Create one and get some visits!</div>;
    }

    return (
        <div className="leaderboard">
            <h2 className = "playlist-leaderboard-header">Most visited playlists</h2>
            <table>
                <thead>
                    <tr>
                        <th>Rank</th>
                        <th>Name</th>
                        <th>Unique Id</th>
                        <th>Popularity</th>
                    </tr>
                </thead>
                <tbody>
                    {leaderboard.leaderboardItems.map((playlist, index) => (
                        <tr key={playlist.id}>
                            <td>#{index + 1}</td>
                            <td>{playlist.name}</td>
                            <td>{playlist.id}</td>
                            <td>{playlist.popularity}</td>
                        </tr>
                    ))}
                </tbody>
            </table>
        </div>
    );
}