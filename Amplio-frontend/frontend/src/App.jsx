import { useState } from 'react'
import './css/App.css'
import {useNavigate} from "react-router-dom"
import LoadDummyDataButton from './components/dummyDataButton';
import AlbumLeaderboard from './components/albumLeaderboard';
import PlaylistLeaderboard from './components/playlistLeaderboard';


function App() {
  const navigate = useNavigate();

  return (
    <div className="app-page">
      <div className="left-side">
        <div className = "login-content">
          <h1 className = "title-amplio">Amplio</h1>
          <p className = "subtitle">The greatest jam session hub</p>
          <div className = "buttons">

            <button className = "button-amplio" onClick={() => navigate("/join")}>
            Join an existing playlist with code
            </button>

            <button className="button-amplio" onClick = {() => navigate("/create")}>
            Create a new playlist
            </button>
          </div>
          <br></br>
          <div>
            <LoadDummyDataButton/>
          </div>
        </div>
      </div>
      <div className="right-side">
        <div  className = "leaderboard-content-1">
          <AlbumLeaderboard/>
        </div>
        <div  className = "leaderboard-content-2">
          <PlaylistLeaderboard/>
        </div>
      </div>
    </div>
  )
}

export default App
