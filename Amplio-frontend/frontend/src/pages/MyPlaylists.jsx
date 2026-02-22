import { useEffect, useState } from "react";
import { useNavigate } from "react-router-dom";
import { getMyPlaylists } from "../services/playlistAPI";
import "../css/MyPlaylists.css"

function MyPlaylists() {
    const [playlists, setPlaylists] = useState([]);
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState("");
    const navigate = useNavigate();

    useEffect(() => {
        const fetchPlaylists = async () => {
            try {
                const data = await getMyPlaylists();
                setPlaylists(data);
            } catch (err) {
                console.error(err);
                // Don't show error if redirected to login
                if (err.message !== 'Session expired') {
                    setError(err.message || "Failed to load your playlists");
                }
            } finally {
                setLoading(false);
            }
        };

        fetchPlaylists();
    }, []);

    const handleOpen = (id) => {
        navigate(`/playlist/${id}`);
    };

    return (
        <div className="my-playlists-content">
            <div className="center-side">
                <h2 className = "your-playlists-header">Your Playlists</h2>
                <br></br>
                <br></br>

                {loading && <p>Loading...</p>}
                {error && <p className="error-text">{error}</p>}

                {playlists.length === 0 && !loading && !error && (
                    <p>You have no playlists yet. Create one!</p>
                )}

                <ul className="playlist-songs">
                    {playlists.map((p, index) => (
                        <li key={p.id} className="queue-song-item">
                            <span className="queue-song-rank">#{index + 1}</span>
                            <strong className="queue-song-title">{p.name}</strong>
                            <span className="queue-song-artist">{p.isPublic ? "Public" : "Private"}</span>
                            <button 
                                className="queue-vote-button"
                                onClick={() => handleOpen(p.id)}
                            >
                                Open
                            </button>
                        </li>
                    ))}
                </ul>
            </div>
        </div>
    );
}

export default MyPlaylists;