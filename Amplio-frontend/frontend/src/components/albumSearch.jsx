import { useState, useEffect } from "react";
import "../css/SongSearch.css";
import { getAllAlbums } from "../services/albumAPI";
import AlbumView from "./AlbumView";
import "../css/AlbumSearch.css"

function AlbumSearch({ onAdd, playlist }) {
  const [searchQuery, setSearchQuery] = useState("");
  const [albums, setAlbums] = useState([]);
  const [selectedAlbum, setSelectedAlbum] = useState(null);

  useEffect(() => {
    const fetchAlbums = async () => {
      try {
        const data = await getAllAlbums();
        setAlbums(data);
      } catch (error) {
        console.error("Error fetching albums:", error);
      }
    };
    fetchAlbums();
  }, []);

  const filteredAlbums = albums.filter((album) =>
    album.name.toLowerCase().includes(searchQuery.toLowerCase()) ||
    album.artist.toLowerCase().includes(searchQuery.toLowerCase())
  );

  if (selectedAlbum) {
    return (
      <AlbumView 
        album={selectedAlbum} 
        onAdd={onAdd}
        playlist={playlist}
        onBack={() => setSelectedAlbum(null)}
      />
    );
  }

  return (
    <div className="left-panel">
      <h3 className="songSearch-header">Album library</h3>
      <br />
      <input
        type="text"
        placeholder="Search for albums..."
        className="search-input"
        value={searchQuery}
        onChange={(e) => setSearchQuery(e.target.value)}
      />
      <br />
      <br />
      <ul className="album-list">
        {filteredAlbums.map((album) => (
          <li 
            key={album.id} 
            className="album-item"
            onClick={() => setSelectedAlbum(album)}
          >
              <div className="album-info">
                <p className="album-title">{album.name}</p>
                <p className="album-artist">{album.artist}</p>
                <p className="album-year">{album.releaseYear}</p>
              </div>
              <button className="view-album-button">View Songs →</button>
          </li>
        ))}
      </ul>
    </div>
  );
}

export default AlbumSearch;