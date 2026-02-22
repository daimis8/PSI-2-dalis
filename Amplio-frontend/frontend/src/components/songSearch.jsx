import { useState, useEffect } from "react";
import Song from "../components/song";
import "../css/SongSearch.css";
import { getAllSongs } from "../services/songAPI";

function SongSearch({ onAdd, playlist }) {
  const [searchQuery, setSearchQuery] = useState("");
  const [songs, setSongs] = useState([]);

  useEffect(() => {
    const fetchSongs = async () => {
      try {
        const data = await getAllSongs();
        setSongs(data);
      } catch (error) {
        console.error("Error fetching songs:", error);
      }
    };
    fetchSongs();
  }, []);

  const filteredSongs = songs.filter((song) =>
    song.title.toLowerCase().includes(searchQuery.toLowerCase())
  );

  return (
    <div className="left-panel">
      <h3 className = "songSearch-header">Song library</h3>
      <br></br>
      <input
        type="text"
        placeholder="Search for songs..."
        className="search-input"
        value={searchQuery}
        onChange={(e) => setSearchQuery(e.target.value)}
      />
      <br></br>
      <br></br>
      <ul>
        {filteredSongs.map((song) => (
          <li key={song.id}>
            <Song
              key ={song.id}
              song={song}
              onAdd={()=>onAdd(song.id)}
              isInPlaylist={playlist.some((s) => s.id === song.id)}
            />
          </li>
        ))}
      </ul>
    </div>
  );
}
export default SongSearch

