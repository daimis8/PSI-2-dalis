import { useState, useEffect } from 'react';
import { getAlbumLeaderboard } from '../services/leaderboardAPI';
import "../css/Leaderboard.css";

export default function AlbumLeaderboard({ topN = 5 }) {
    const [leaderboard, setLeaderboard] = useState(null);
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState(null); 

    useEffect(() => {
        async function fetchData() {
            try {
                setLoading(true);
                const data = await getAlbumLeaderboard(topN);
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

    if (loading) return <div>Loading album leaderboard...</div>;
    if (error) return <div>Error: {error}</div>;
    if (!leaderboard || leaderboard.leaderboardItems.length === 0) {
        return <div>No albums yet.</div>;
    }

    return (
        <div className="leaderboard">
            <h2 className = "album-leaderboard-header">Most popular albums</h2>
            <table>
                <thead>
                    <tr>
                        <th>Rank</th>
                        <th>Album</th>
                        <th>Artist</th>
                        <th>Year</th>
                        <th>Popularity</th>
                    </tr>
                </thead>
                <tbody>
                    {leaderboard.leaderboardItems.map((album, index) => (
                        <tr key={album.id}>
                            <td>#{index + 1}</td>
                            <td>{album.name}</td>
                            <td>{album.artist}</td>
                            <td>{album.releaseYear}</td>
                            <td>{album.popularity}</td>
                        </tr>
                    ))}
                </tbody>
            </table>
        </div>
    );
}