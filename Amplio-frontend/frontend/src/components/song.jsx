import "../css/Song.css";
import { getAlbumById } from "../services/albumAPI";
import React, { useState, useEffect } from "react";


function Song({ song, onAdd, isInPlaylist }) {

  const [albumName, setAlbumName] = useState("");

  useEffect(() => {
    const handleGetAlbumName = async () => {
      try {
        const result = await getAlbumById(song.albumId);
        console.log("album by id result:", result);
        setAlbumName(result.name);
      } catch (err) {
        console.error("failed to album by id ", err);
      }
    };

    handleGetAlbumName();
  }, [song.albumId]); 
  
  return (
    <div className="song-card">
      <div className="song-info">
        <p className="song-title">{song.title}</p>
        <p className="song-artist">{song.artist}</p>
        <p className = "song-album">{albumName}</p>
        <p className="song-genres">{song.genres.join(", ")}</p>
      </div>
      <button className = "add-button" onClick={onAdd} disabled={isInPlaylist}>
        {isInPlaylist ? "Added" : "Add"}
      </button>
    </div>
  );
}

export default Song;