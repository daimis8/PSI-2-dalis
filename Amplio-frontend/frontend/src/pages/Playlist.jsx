import {useState, useEffect, useRef} from "react"
import {useParams, useNavigate} from "react-router-dom"
import SearchToggle from "../components/searchToggle"
import { 
    getPlaylistSongs, 
    addSongToPlaylist, 
    deleteSongFromPlaylist, 
    upvoteSongInPlaylist,
    setCurrentSong,
    getCurrentSong,
    registerPlaylistVisit,
    clearCurrentSong,
    getPlaylistName
} from "../services/playlistAPI";
import "../css/Playlist.css"
import { getSongLink } from "../services/songAPI";

function Playlist()
{
    const {playlistId} = useParams();
    const navigate = useNavigate();
    const [playlistSongs, setPlaylistSongs] = useState([]);
    const [playlistName, setPlaylistName] = useState("");
    const [error, setError] = useState("");
    const [loading, setLoading] = useState(true);
    const [currentSong, setCurrentSongState] = useState(null);
    const [copied, setCopied] = useState(false);
    const playerRef = useRef(null);
    const [isPlayerReady, setIsPlayerReady] = useState(false);
    const isSettingCurrentSong = useRef(false);
    const hasInitiallyFetched = useRef(false);
    const lastKnownCurrentSong = useRef(null);
    const hasRegisteredVisit = useRef(false);

    useEffect(() => {
        async function recordVisit() {
            if (!hasRegisteredVisit.current) {
                try {
                    await registerPlaylistVisit(playlistId);
                    console.log("Visit registered for playlist:", playlistId);
                    hasRegisteredVisit.current = true;
                } catch (error) {
                    console.error("Failed to register visit:", error);
                }
            }
        }
        
        recordVisit();
    }, [playlistId]);

    // Fetch playlist name once on mount
useEffect(() => {
    async function fetchPlaylistName() {
        try {
            const name = await getPlaylistName(playlistId);
            setPlaylistName(name);
        } catch (error) {
            console.error("Failed to fetch playlist name:", error);
            setPlaylistName("Unnamed Playlist");
        }
    }
    
    fetchPlaylistName();
}, [playlistId]);

    useEffect(() => {
        async function fetchPlaylistData() {
            try {
                const fetched = await getPlaylistSongs(playlistId);
                const songsOnly = fetched.map(entry => entry.song);
                setPlaylistSongs(songsOnly);
                
                console.log("🔄 Polling - Queue has", songsOnly.length, "songs");
                
                try {
                    const current = await getCurrentSong(playlistId);
                    console.log("🎵 Current song from backend:", current?.title || "null");
                    setCurrentSongState(current);
                    lastKnownCurrentSong.current = current;
                } catch (err) {
                    console.log("❌ No current song in backend");
                    setCurrentSongState(null);
                    lastKnownCurrentSong.current = null;
                }
                
                hasInitiallyFetched.current = true;
                setLoading(false);
            }
            catch(err) {
                console.error(err);
                setError("Failed to load playlist data");
                setLoading(false);
            }
        }
        
        fetchPlaylistData();
        const interval = setInterval(fetchPlaylistData, 2000);
        return() => clearInterval(interval);
    }, [playlistId]);

    const handleAddSong = async(songId) =>
    {
        try{
            await addSongToPlaylist(playlistId, songId);
            const updated = await getPlaylistSongs(playlistId);
            const songsOnly = updated.map(entry => entry.song);
            setPlaylistSongs(songsOnly);
            console.log(songsOnly);
        }
        catch(err)
        {
            console.error(err);
            setError("Failed to add song to playlist");
        }
    };

    const handleUpvote = async(songId) => {
        try {
            await upvoteSongInPlaylist(playlistId, songId);
        } catch(err) {
            console.error("Failed to upvote song:", err);
            setError("Failed to upvote song");
        }
    };

    const handleCopyCode = () => {
        navigator.clipboard.writeText(playlistId);
        setCopied(true);
        setTimeout(() => setCopied(false), 2000);
    };

    useEffect(() => {
        if (!window.YT) {
            const tag = document.createElement('script');
            tag.src = 'https://www.youtube.com/iframe_api';
            const firstScriptTag = document.getElementsByTagName('script')[0];
            firstScriptTag.parentNode.insertBefore(tag, firstScriptTag);
        }

        window.onYouTubeIframeAPIReady = () => {
            setIsPlayerReady(true);
        };

        if (window.YT && window.YT.Player) {
            setIsPlayerReady(true);
        }
    }, []);

    useEffect(() => {
        const tryStartPlaying = async () => {
            console.log("⚡ tryStartPlaying check:", {
                hasInitiallyFetched: hasInitiallyFetched.current,
                currentSong: currentSong?.title,
                queueLength: playlistSongs.length,
                lastKnown: lastKnownCurrentSong.current?.title
            });
            
            if (!hasInitiallyFetched.current) {
                console.log("⏳ Not fetched yet, skipping");
                return;
            }
            
            if (playlistSongs.length === 0 && !currentSong) {
                console.log("📭 Playlist completely empty, nothing to play");
                return;
            }
            
            const shouldStartNewSong = !currentSong && 
                                       !lastKnownCurrentSong.current && 
                                       playlistSongs.length > 0 && 
                                       !isSettingCurrentSong.current;
            
            console.log("🤔 Should start new song?", shouldStartNewSong);
            
            if (shouldStartNewSong) {
                isSettingCurrentSong.current = true;
                console.log("🚀 Attempting to start new song...");
                try {
                    const newCurrentSong = await setCurrentSong(playlistId);
                    console.log("✅ New current song set:", newCurrentSong.title);
                    setCurrentSongState(newCurrentSong);
                    lastKnownCurrentSong.current = newCurrentSong;
                    
                    const updated = await getPlaylistSongs(playlistId);
                    const songsOnly = updated.map(entry => entry.song);
                    setPlaylistSongs(songsOnly);
                } catch (err) {
                    console.log("❌ Could not set current song:", err.message);
                    if (err.message.includes("empty")) {
                        setCurrentSongState(null);
                        lastKnownCurrentSong.current = null;
                    }
                } finally {
                    isSettingCurrentSong.current = false;
                }
            }
        };

        tryStartPlaying();
    }, [playlistSongs.length, currentSong, playlistId]);

    useEffect(() => {
        if (!currentSong || !isPlayerReady) return;

        const initPlayer = async () => {
            try {
                const link = currentSong.link?.value || currentSong.link;

                if (!link) {
                    console.error("No link found in current song");
                    return;
                }

                let videoId = link;
                const youtubeRegex = /(?:youtube\.com\/(?:[^\/]+\/.+\/|(?:v|e(?:mbed)?)\/|.*[?&]v=)|youtu\.be\/)([^"&?\/\s]{11})/;
                const match = link.match(youtubeRegex);
                if (match) {
                    videoId = match[1];
                }

                if (playerRef.current) {
                    playerRef.current.destroy();
                }

                playerRef.current = new window.YT.Player('youtube-player', {
                    height: '300',
                    width: '100%',
                    videoId: videoId,
                    playerVars: {
                        autoplay: 1,
                        controls: 1,
                        modestbranding: 1,
                    },
                    events: {
                        onReady: (event) => {
                            console.log("Player ready, starting playback");
                        },
                        onStateChange: (event) => {
                            if (event.data === window.YT.PlayerState.ENDED) {
                                handleSongEnded();
                            }
                        },
                    },
                });
            } catch (err) {
                console.error("Failed to initialize YouTube player:", err);
            }
        };

        initPlayer();

        return () => {
            if (playerRef.current) {
                playerRef.current.destroy();
            }
        };
    }, [currentSong?.id, isPlayerReady]);

    const handleSongEnded = async () => {
        console.log("🏁 Song ended - clearing current song");
        console.log("   Queue length:", playlistSongs.length);
        
        // Clear on backend first
        try {
            await clearCurrentSong(playlistId);
            console.log("✅ Backend current song cleared");
        } catch (err) {
            console.error("❌ Failed to clear current song on backend:", err);
        }
        
        // Then clear locally
        setCurrentSongState(null);
        lastKnownCurrentSong.current = null;
        isSettingCurrentSong.current = false;
    };

    const handleSkipSong = () => {
        if (currentSong) {
            handleSongEnded();
        }
    };

    if (loading) 
        return <p>Loading...</p>;
    if (error)
        return <p className="error-text">{error}</p>

    return(
        <>
        <div className="playlist-header">
    <button className="back-button" onClick={() => navigate("/home/playlists")}>
        ← Back to home page
    </button>

    <h2 className="playlist-title">You are listening to: {playlistName || "Loading..."}</h2>

    <div className="playlist-code-box">
        <span className="share-label">Share code with others to join you:  </span>
        <code className="playlist-code">{playlistId}</code>
        <span>   </span>
        <button className="copy-button" onClick={handleCopyCode}>
            {copied ? "✓" : "Copy"}
        </button>
    </div>
</div>


        <div className = "content">
            <div className = "left-side">
                <SearchToggle
                    onAdd={handleAddSong}
                    playlist={playlistSongs}
                />
            </div>
            <div className = "right-side">
                
                {currentSong && (
                    <div className="audio-player">
                        <h3>Now Playing</h3>
                        <p className="now-playing-text">
                            <strong>{currentSong.title}</strong> - {currentSong.artist}
                        </p>
                        <br></br>
                        <div id="youtube-player"></div>
                        
                        <div className="audio-buttons">
                            <button 
                                className="skip-button"
                                onClick={handleSkipSong}>
                                Skip Song
                            </button>
                        </div>
                    </div>
                )}
                <h2>Playlist Queue</h2>
                {playlistSongs.length === 0 && <p>No songs in queue. {currentSong ? 'Playing current song...' : 'Add songs to get started!'}</p>}
                <ul className="playlist-songs">
                {playlistSongs.map((song, index) => (
                    <li
                    key={song.id}
                    className="queue-song-item">
                        <span className="queue-song-rank">#{index + 1}</span>
                        <strong className="queue-song-title">{song.title}</strong>  
                        <span className="queue-song-artist">{song.artist}</span>
                        <span className="queue-song-genres">{song.genres.join(", ")}</span>
                    
                    <button 
                        className="queue-vote-button"
                        onClick={() => handleUpvote(song.id)}>
                        Vote
                    </button>
                    </li>
                ))}
                </ul>
                
            </div>
        </div>
        </>
    )
}
export default Playlist