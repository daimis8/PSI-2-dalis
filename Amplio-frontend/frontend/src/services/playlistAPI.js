import { authenticatedFetch } from "./authAPI";

const base = "http://localhost:5001";

export async function createPlaylist(name, isPublic = false) {
  const response = await authenticatedFetch(`${base}/playlist`, {
    method: "POST",
    body: JSON.stringify({
      name,
      isPublic,
      currentSongId: null,
    }),
  });

  if (!response.ok) {
    throw new Error("Failed to create playlist");
  }

  return await response.json();
}

export async function getPlaylistSongs(playlistId) {
  const response = await authenticatedFetch(`${base}/playlist/${playlistId}`);

  if (!response.ok) {
    throw new Error("Playlist not found");
  }

  return await response.json();
}

export async function getPlaylistName(playlistId) {
  const response = await authenticatedFetch(
    `${base}/playlist/${playlistId}/name`
  );

  if (response.status === 401) {
    window.location.href = "/login";
    throw new Error("Session expired");
  }

  if (!response.ok) {
    throw new Error("Could not get name of playlist");
  }

  const text = await response.text();

  try {
    const data = JSON.parse(text);
    return typeof data === "string"
      ? data
      : data.name || data.value || "Unnamed Playlist";
  } catch (e) {
    // If it's not JSON, it's plain text
    return text.replace(/^"|"$/g, ""); // Remove quotes if present
  }
}

export async function addSongToPlaylist(playlistId, songId) {
  const response = await authenticatedFetch(
    `${base}/playlist/${playlistId}/add/${songId}`,
    {
      method: "POST",
    }
  );

  if (!response.ok) {
    throw new Error("Song could not be added to playlist");
  }

  return await response.json();
}

export async function deleteSongFromPlaylist(playlistId, songId) {
  const response = await authenticatedFetch(
    `${base}/playlist/${playlistId}/delete/${songId}`,
    {
      method: "DELETE",
    }
  );

  if (!response.ok) {
    throw new Error("Song could not be deleted");
  }

  return await response.json();
}

export async function upvoteSongInPlaylist(playlistId, songId) {
  const response = await authenticatedFetch(
    `${base}/playlist/${playlistId}/vote/${songId}`,
    {
      method: "POST",
    }
  );

  if (!response.ok) {
    throw new Error("Song could not be upvoted");
  }

  return await response.json();
}

export async function setCurrentSong(playlistId) {
  const response = await authenticatedFetch(
    `${base}/playlist/${playlistId}/play`,
    {
      method: "POST",
    }
  );

  if (!response.ok) {
    throw new Error("Could not set current song");
  }

  return await response.json();
}

export async function getCurrentSong(playlistId) {
  const response = await authenticatedFetch(
    `${base}/playlist/${playlistId}/current`
  );

  if (!response.ok) {
    throw new Error("Current song not found");
  }

  return await response.json();
}

export async function registerPlaylistVisit(playlistId) {
  const response = await authenticatedFetch(
    `${base}/playlist/${playlistId}/visit`,
    {
      method: "POST",
    }
  );

  if (!response.ok) {
    throw new Error("Could not register visit");
  }

  return await response.json();
}

export async function clearCurrentSong(playlistId) {
  const response = await authenticatedFetch(
    `${base}/playlist/${playlistId}/current`,
    {
      method: "DELETE",
    }
  );

  if (!response.ok) {
    throw new Error("Could not clear current song");
  }

  return await response.json();
}

export async function getMyPlaylists() {
  const response = await authenticatedFetch(`${base}/playlist/personal`);

  if (!response.ok) {
    throw new Error("Failed to fetch user's playlists");
  }

  return await response.json();
}
