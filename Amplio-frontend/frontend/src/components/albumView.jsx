import { useState, useEffect } from "react";
import Song from "./song";
import "../css/AlbumSearch.css"
import "../css/AlbumView.css"
import { getAlbumById } from "../services/albumAPI";

function AlbumView({ album, onAdd, playlist, onBack }) {
  const [albumDetails, setAlbumDetails] = useState(null);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    const fetchAlbumDetails = async () => {
      try {
        const data = await getAlbumById(album.id);
        setAlbumDetails(data);
        setLoading(false);
      } catch (error) {
        console.error("Error fetching album details:", error);
        setLoading(false);
      }
    };
    fetchAlbumDetails();
  }, [album.id]);

  if (loading) return <div className="left-panel">Loading album...</div>;
  if (!albumDetails) return <div className="left-panel">Album not found</div>;

  return (
    <div className="left-panel">
      <button className="back-button" onClick={onBack}>
        ← Back to Albums
      </button>
      <br />
      <br />
      <h2>Album details</h2>
      <div className="album-details">
        <h3>{albumDetails.name}</h3>
        <p className="album-artist">{albumDetails.artist}</p>
        <p className="album-year">{albumDetails.releaseYear}</p>
      </div>
      <br />
      <h4>Songs in this album:</h4>
      <br />
      <ul>
        {albumDetails.songs && albumDetails.songs.length > 0 ? (
          albumDetails.songs.map((song) => (
            <li key={song.id}>
              <Song
                song={song}
                onAdd={() => onAdd(song.id)}
                isInPlaylist={playlist.some((s) => s.id === song.id)}
              />
            </li>
          ))
        ) : (
          <p>No songs in this album</p>
        )}
      </ul>
    </div>
  );
}

export default AlbumView;