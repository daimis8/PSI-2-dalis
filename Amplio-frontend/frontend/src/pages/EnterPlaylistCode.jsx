import { useState } from "react";
import { getPlaylistSongs } from "../services/playlistAPI";
import { useNavigate } from "react-router-dom";
import "../css/JoinPlaylist.css";

function EnterPlaylistCode() {
    const [searchQuery, setSearchQuery] = useState("");
    const [error, setError] = useState("");
    const [loading, setLoading] = useState(false);
    const navigate = useNavigate();

    const handleSearch = async (e) => {
        e.preventDefault();
        setError("");
        setLoading(true);
        
        try {
            await getPlaylistSongs(searchQuery);
            setLoading(false);
            navigate(`/playlist/${searchQuery}`);
        } catch (err) {
            setLoading(false);
            // Don't show error if redirected to login
            if (err.message !== 'Session expired') {
                setError("The unique id does not belong to any playlist. Please try again.");
            }
            console.error(err);
            setSearchQuery("");
        }
    };

    return (
        <div className="join-wrapper">
            <h1 className="join-header">Enter the playlist code below:</h1>
            <form onSubmit={handleSearch} className="join-content">
                <input
                    type="text"
                    placeholder="Enter the unique playlist code"
                    className="search-input"
                    value={searchQuery}
                    onChange={(e) => setSearchQuery(e.target.value)}
                    required
                />
                <button
                    type="submit"
                    className="button-join"
                    disabled={loading}
                >
                    {loading ? "Checking code..." : "Enter code"}
                </button>
            </form>
            {error && <p className="error-join">{error}</p>}
        </div>
    );
}

export default EnterPlaylistCode;