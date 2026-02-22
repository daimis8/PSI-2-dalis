const base = "http://localhost:5001/leaderboard";

export async function getPlaylistLeaderboard(topN) {
  const response = await fetch(`${base}/playlists?topN=${topN}`);

  console.log("Response status:", response.status);
  const text = await response.text();
  console.log("Response body:", text);

  if (!response.ok) throw new Error("Error retrieving playlist leaderboard");

  return JSON.parse(text);
}
export async function getAlbumLeaderboard(topN) {
  const response = await fetch(`${base}/albums?topN=${topN}`);

  console.log("Album Response status:", response.status);
  const text = await response.text();
  console.log("Album Response body:", text);

  if (!response.ok) throw new Error("Error retrieving album leaderboard");

  return JSON.parse(text);
}
