import { authenticatedFetch } from "./authAPI";

const base = "http://localhost:5001";

async function jsonOrThrow(response, fallbackMessage) {
  if (!response.ok) {
    let message = fallbackMessage;
    try {
      const data = await response.json();
      if (data?.message) message = data.message;
    } catch {
      // body wasn't JSON; keep the fallback
    }
    const err = new Error(message);
    err.status = response.status;
    throw err;
  }
  if (response.status === 204) return null;
  return await response.json();
}

export async function searchUsers(query) {
  const q = encodeURIComponent((query || "").trim());
  const response = await authenticatedFetch(`${base}/friends/search?q=${q}`);
  return await jsonOrThrow(response, "Search failed");
}

export async function listFriends() {
  const response = await authenticatedFetch(`${base}/friends`);
  return await jsonOrThrow(response, "Failed to load friends");
}

export async function getIncomingRequests() {
  const response = await authenticatedFetch(`${base}/friends/requests/incoming`);
  return await jsonOrThrow(response, "Failed to load incoming requests");
}

export async function getOutgoingRequests() {
  const response = await authenticatedFetch(`${base}/friends/requests/outgoing`);
  return await jsonOrThrow(response, "Failed to load outgoing requests");
}

export async function sendFriendRequest(receiverId) {
  const response = await authenticatedFetch(`${base}/friends/requests`, {
    method: "POST",
    body: JSON.stringify({ receiverId }),
  });
  return await jsonOrThrow(response, "Failed to send friend request");
}

export async function acceptFriendRequest(requestId) {
  const response = await authenticatedFetch(
    `${base}/friends/requests/${requestId}/accept`,
    { method: "POST" }
  );
  return await jsonOrThrow(response, "Failed to accept request");
}

export async function declineFriendRequest(requestId) {
  const response = await authenticatedFetch(
    `${base}/friends/requests/${requestId}/decline`,
    { method: "POST" }
  );
  return await jsonOrThrow(response, "Failed to decline request");
}

export async function cancelFriendRequest(requestId) {
  const response = await authenticatedFetch(
    `${base}/friends/requests/${requestId}`,
    { method: "DELETE" }
  );
  return await jsonOrThrow(response, "Failed to cancel request");
}

export async function removeFriend(friendId) {
  const response = await authenticatedFetch(`${base}/friends/${friendId}`, {
    method: "DELETE",
  });
  return await jsonOrThrow(response, "Failed to remove friend");
}

export async function blockUser(userId) {
  const response = await authenticatedFetch(`${base}/friends/blocks/${userId}`, {
    method: "POST",
  });
  return await jsonOrThrow(response, "Failed to block user");
}

export async function unblockUser(userId) {
  const response = await authenticatedFetch(`${base}/friends/blocks/${userId}`, {
    method: "DELETE",
  });
  return await jsonOrThrow(response, "Failed to unblock user");
}

export async function inviteFriendToPlaylist(playlistId, friendId) {
  const response = await authenticatedFetch(
    `${base}/playlist/${playlistId}/invite/${friendId}`,
    { method: "POST" }
  );
  return await jsonOrThrow(response, "Failed to invite friend");
}
