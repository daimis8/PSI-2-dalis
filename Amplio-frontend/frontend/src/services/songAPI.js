const base = "http://localhost:5001";

export async function getAllSongs() {
  const response = await fetch(`${base}/songs`);
  if (!response.ok) throw new Error("Error retrieving songs");
  return await response.json();
}

export async function getSongLink(songId) {
  const response = await fetch(`${base}/songs/play/${songId}`);
  if (!response.ok) throw new Error("Error retrieving songs");
  return await response.json();
}

export async function getSongById(songId) {
  console.log("sending request to get song by id");
  const response = await fetch(`${base}/songs/${songId}`);
  console.log("Response received: ", response);
  if (!response.ok) {
    console.log("Song not found");
    throw new Error("Song not found");
  }
  return await response.json();
}

export async function uploadSongs() {
  console.log("sending request to upload songs");
  const response = await fetch(`${base}/songs/upload`, { method: "POST" });
  console.log("Response received: ", response);
  if (!response.ok) {
    console.log("Failed to upload songs");
    throw new Error("Failed to upload songs");
  }
  return await response.json();
}
