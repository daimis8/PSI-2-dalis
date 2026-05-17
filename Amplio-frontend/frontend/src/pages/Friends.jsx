import { useEffect, useState } from "react";
import {
  acceptFriendRequest,
  blockUser,
  cancelFriendRequest,
  declineFriendRequest,
  getIncomingRequests,
  getOutgoingRequests,
  listFriends,
  removeFriend,
  searchUsers,
  sendFriendRequest,
} from "../services/friendsAPI";

const TABS = [
  { key: "friends", label: "Friends" },
  { key: "incoming", label: "Incoming" },
  { key: "outgoing", label: "Outgoing" },
  { key: "search", label: "Find people" },
];

function Friends() {
  const [activeTab, setActiveTab] = useState("friends");
  const [friends, setFriends] = useState([]);
  const [incoming, setIncoming] = useState([]);
  const [outgoing, setOutgoing] = useState([]);
  const [searchQuery, setSearchQuery] = useState("");
  const [searchResults, setSearchResults] = useState([]);
  const [error, setError] = useState("");
  const [info, setInfo] = useState("");
  const [loading, setLoading] = useState(false);

  const refreshAll = async () => {
    setLoading(true);
    setError("");
    try {
      const [f, i, o] = await Promise.all([
        listFriends(),
        getIncomingRequests(),
        getOutgoingRequests(),
      ]);
      setFriends(f || []);
      setIncoming(i || []);
      setOutgoing(o || []);
    } catch (err) {
      setError(err.message || "Failed to load friends data");
    } finally {
      setLoading(false);
    }
  };

 useEffect(() => {
  refreshAll();
  const interval = setInterval(() => {
    refreshAll();
  }, 5000);
  return () => clearInterval(interval);
}, []);

  const handleSearch = async (event) => {
    event.preventDefault();
    setError("");
    setInfo("");
    if (!searchQuery.trim()) {
      setSearchResults([]);
      return;
    }
    try {
      const results = await searchUsers(searchQuery);
      setSearchResults(results || []);
    } catch (err) {
      setError(err.message || "Search failed");
    }
  };

  const handleSend = async (userId) => {
    setError("");
    try {
      await sendFriendRequest(userId);
      setInfo("Friend request sent");
      await refreshAll();
    } catch (err) {
      setError(err.message || "Failed to send request");
    }
  };

  const handleAccept = async (id) => {
    try {
      await acceptFriendRequest(id);
      await refreshAll();
    } catch (err) {
      setError(err.message);
    }
  };

  const handleDecline = async (id) => {
    try {
      await declineFriendRequest(id);
      await refreshAll();
    } catch (err) {
      setError(err.message);
    }
  };

  const handleCancel = async (id) => {
    try {
      await cancelFriendRequest(id);
      await refreshAll();
    } catch (err) {
      setError(err.message);
    }
  };

  const handleRemove = async (id) => {
    if (!confirm("Remove this friend?")) return;
    try {
      await removeFriend(id);
      await refreshAll();
    } catch (err) {
      setError(err.message);
    }
  };

  const handleBlock = async (id) => {
    if (!confirm("Block this user? Any pending request and friendship will be removed.")) return;
    try {
      await blockUser(id);
      await refreshAll();
    } catch (err) {
      setError(err.message);
    }
  };

  return (
    <div className="my-playlists-content">
      <div className="center-side">
        <h2 className="your-playlists-header">Friends</h2>

        <nav style={{ display: "flex", gap: "1rem", marginBottom: "1rem" }}>
          {TABS.map((t) => (
            <button
              key={t.key}
              className={activeTab === t.key ? "tab active" : "tab"}
              onClick={() => {
                setActiveTab(t.key);
                setError("");
                setInfo("");
              }}
            >
              {t.label}
              {t.key === "incoming" && incoming.length > 0 ? ` (${incoming.length})` : ""}
            </button>
          ))}
        </nav>

        {error && <p className="error-text">{error}</p>}
        {info && <p>{info}</p>}
        {loading && <p>Loading…</p>}

        {activeTab === "friends" && (
          <ul className="playlist-songs">
            {friends.length === 0 && !loading && <li>No friends yet.</li>}
            {friends.map((f) => (
              <li key={f.id} className="queue-song-item">
                <strong className="queue-song-title">{f.username}</strong>
                <button className="queue-vote-button" onClick={() => handleRemove(f.id)}>
                  Remove
                </button>
                <button className="queue-vote-button" onClick={() => handleBlock(f.id)}>
                  Block
                </button>
              </li>
            ))}
          </ul>
        )}

        {activeTab === "incoming" && (
          <ul className="playlist-songs">
            {incoming.length === 0 && !loading && <li>No incoming requests.</li>}
            {incoming.map((r) => (
              <li key={r.id} className="queue-song-item">
                <strong className="queue-song-title">{r.senderUsername}</strong>
                <button className="queue-vote-button" onClick={() => handleAccept(r.id)}>
                  Accept
                </button>
                <button className="queue-vote-button" onClick={() => handleDecline(r.id)}>
                  Decline
                </button>
                <button className="queue-vote-button" onClick={() => handleBlock(r.senderId)}>
                  Block
                </button>
              </li>
            ))}
          </ul>
        )}

        {activeTab === "outgoing" && (
          <ul className="playlist-songs">
            {outgoing.length === 0 && !loading && <li>No outgoing requests.</li>}
            {outgoing.map((r) => (
              <li key={r.id} className="queue-song-item">
                <strong className="queue-song-title">{r.receiverUsername}</strong>
                <button className="queue-vote-button" onClick={() => handleCancel(r.id)}>
                  Cancel
                </button>
              </li>
            ))}
          </ul>
        )}

        {activeTab === "search" && (
          <>
            <form onSubmit={handleSearch} style={{ display: "flex", gap: "0.5rem", marginBottom: "1rem" }}>
              <input
                type="text"
                value={searchQuery}
                onChange={(e) => setSearchQuery(e.target.value)}
                placeholder="Search by username"
              />
              <button type="submit">Search</button>
            </form>
            <ul className="playlist-songs">
              {searchResults.length === 0 && (
                <li>{searchQuery ? "No results" : "Type a username and press Search."}</li>
              )}
              {searchResults.map((u) => (
                <li key={u.id} className="queue-song-item">
                  <strong className="queue-song-title">{u.username}</strong>
                  <button className="queue-vote-button" onClick={() => handleSend(u.id)}>
                    Add friend
                  </button>
                </li>
              ))}
            </ul>
          </>
        )}
      </div>
    </div>
  );
}

export default Friends;
