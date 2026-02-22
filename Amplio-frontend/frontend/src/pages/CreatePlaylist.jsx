import { useState } from "react";
import { useNavigate } from "react-router-dom";
import { createPlaylist } from "../services/playlistAPI";
import "../css/CreatePlaylist.css";

function CreatePlaylist() {
    const [name, setName] = useState("");
    const [isPublic, setIsPublic] = useState(false);
    const [error, setError] = useState("");
    const [loading, setLoading] = useState(false);
    const [code, setCode] = useState("");
    const navigate = useNavigate();

    const handleCreate = async (e) => {
        e.preventDefault();
        setError("");
        setLoading(true);

        try {
            const playlist = await createPlaylist(name, isPublic);
            console.log("Playlist created:", playlist);
            setCode(playlist.id);
            setLoading(false);
        } catch (err) {
            console.error(err);
            // Don't show error if redirected to login
            if (err.message !== 'Session expired') {
                setError(err.message || "The playlist could not be created. Please try again.");
            }
            setLoading(false);
        }
    };

    return (
        <div className="create-wrapper">
            <h1 className="create-header">Create your playlist</h1>

            <form onSubmit={handleCreate} className="create-content">
                <input
                    type="text"
                    placeholder="Enter playlist name here"
                    className="search-input"
                    value={name}
                    onChange={(e) => setName(e.target.value)}
                    required
                />

                <div className="checkbox-container">
                    <label>
                        <input
                            type="checkbox"
                            checked={isPublic}
                            onChange={(e) => setIsPublic(e.target.checked)}
                        />
                        <span className="checkbox-label">
                            Mark playlist as public <br />
                            (Playlist will be shown in leaderboard and others <br />
                            will be able to join it)
                        </span>
                    </label>
                </div>

                <button type="submit" className="button-create" disabled={loading}>
                    {loading ? "Creating..." : "Create playlist"}
                </button>
            </form>

            {error && <div className="error-text">{error}</div>}

            {code && (
                <div className="success-create">
                    <br />
                    <p className="created-text">Playlist created successfully!</p>
                    <p className="created-text">Your unique playlist code is: {code}</p>
                    <br />
                    <button
                        className="button-create"
                        onClick={() => navigate(`/playlist/${code}`)}
                    >
                        Go to playlist {name}
                    </button>
                </div>
            )}
        </div>
    );
}

export default CreatePlaylist;