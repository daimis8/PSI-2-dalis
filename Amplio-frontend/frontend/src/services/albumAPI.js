const base = "http://localhost:5001/albums";

export async function getAllAlbums() {
  const response = await fetch(`${base}`);
  if (!response.ok) {
    console.log("Could not retrieve albums");
    throw new Error("Could not retrieve albums");
  }

  return await response.json();
}

export async function getAlbumById(albumId) {
  const response = await fetch(`${base}/${albumId}`);
  if (!response.ok) {
    console.log("Album not found");
    throw new Error("Album not found");
  }

  return await response.json();
}
