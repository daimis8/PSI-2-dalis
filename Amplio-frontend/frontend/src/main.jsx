import React from 'react';
import ReactDOM from 'react-dom/client';
import { BrowserRouter, Routes, Route, Navigate } from 'react-router-dom';
import Login from './pages/Login';
import Register from './pages/Register';
import Home from './pages/Home';
import CreatePlaylist from './pages/CreatePlaylist';
import EnterPlaylistCode from './pages/EnterPlaylistCode';
import Playlist from './pages/Playlist';
import MyPlaylists from './pages/MyPlaylists';
import Leaderboard from './pages/Leaderboard';
import ProtectedRoute from './components/ProtectedRoute';
import { isAuthenticated } from './services/authAPI';
import './css/index.css';

ReactDOM.createRoot(document.getElementById('root')).render(
  <React.StrictMode>
    <BrowserRouter>
      <Routes>
        {/* Redirect root to login or home */}
        <Route 
          path="/" 
          element={isAuthenticated() ? <Navigate to="/home/playlists" replace /> : <Navigate to="/login" replace />} 
        />

        {/* Public routes */}
        <Route path="/login" element={<Login />} />
        <Route path="/register" element={<Register />} />

        {/* Protected layout with nested routes */}
        <Route 
          path="/home" 
          element={
            <ProtectedRoute>
              <Home />
            </ProtectedRoute>
          }
        >
          {/* Nested pages render inside <Outlet /> of Home */}
          <Route path="playlists" element={<MyPlaylists />} />
          <Route path="leaderboard" element={<Leaderboard />} />
          <Route path="create" element={<CreatePlaylist />} />
          <Route path="join" element={<EnterPlaylistCode />} />
        </Route>

        {/* Playlist view - separate from home tabs */}
        <Route 
          path="/playlist/:playlistId" 
          element={
            <ProtectedRoute>
              <Playlist />
            </ProtectedRoute>
          } 
        />
      </Routes>
    </BrowserRouter>
  </React.StrictMode>,
);