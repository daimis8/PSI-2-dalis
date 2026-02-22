import { useState } from "react";
import SongSearch from "./songSearch";
import AlbumSearch from "./albumSearch";
import "../css/SearchToggle.css";

function SearchToggle({ onAdd, playlist }) {
  const [searchMode, setSearchMode] = useState("songs"); // "songs" or "albums"

  return (
    <div className="search-container">
      <div className="toggle-buttons">
        <button
          className={`toggle-button ${searchMode === "songs" ? "active" : ""}`}
          onClick={() => setSearchMode("songs")}
        >
           Search by Songs
        </button>
        <button
          className={`toggle-button ${searchMode === "albums" ? "active" : ""}`}
          onClick={() => setSearchMode("albums")}
        >
           Search by Albums
        </button>
      </div>
      
      {searchMode === "songs" ? (
        <SongSearch onAdd={onAdd} playlist={playlist} />
      ) : (
        <AlbumSearch onAdd={onAdd} playlist={playlist} />
      )}
    </div>
  );
}

export default SearchToggle;