const base = "http://localhost:5001";

// Store token after login/register
export function saveToken(token) {
  localStorage.setItem("authToken", token);
}

// Get token for API requests
export function getToken() {
  return localStorage.getItem("authToken");
}

// Remove token on logout
export function clearToken() {
  localStorage.removeItem("authToken");
}

// Check if user is logged in
export function isAuthenticated() {
  const token = getToken();
  if (!token) return false;

  try {
    const payload = JSON.parse(atob(token.split(".")[1]));
    const exp = payload.exp * 1000;
    return Date.now() < exp;
  } catch {
    return false;
  }
}

// Get user info from token
export function getUserInfo() {
  const token = getToken();
  if (!token) return null;

  try {
    const payload = JSON.parse(atob(token.split(".")[1]));
    return {
      userId:
        payload[
          "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier"
        ],
      username:
        payload["http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name"],
    };
  } catch {
    return null;
  }
}

// Register new user
export async function register(username, password) {
  const response = await fetch(`${base}/auth/register`, {
    method: "POST",
    headers: {
      "Content-Type": "application/json",
    },
    body: JSON.stringify({ username, password }),
  });

  if (!response.ok) {
    const error = await response
      .json()
      .catch(() => ({ message: "Registration failed" }));
    throw new Error(error.message || "Registration failed");
  }

  const { token } = await response.json();
  console.log("🔑 TOKEN RECEIVED:", token);
  saveToken(token);
  console.log("✅ TOKEN SAVED:", localStorage.getItem("authToken"));
  return token;
}

// Login user
export async function login(username, password) {
  const response = await fetch(`${base}/auth/login`, {
    method: "POST",
    headers: {
      "Content-Type": "application/json",
    },
    body: JSON.stringify({ username, password }),
  });

  if (!response.ok) {
    const error = await response
      .json()
      .catch(() => ({ message: "Login failed" }));
    throw new Error(error.message || "Invalid username or password");
  }

  const { token } = await response.json();
  console.log("🔑 TOKEN RECEIVED:", token);
  saveToken(token);
  console.log("✅ TOKEN SAVED:", localStorage.getItem("authToken"));
  return token;
}

// Logout user
export function logout() {
  clearToken();
}

// Helper function to make authenticated API requests
export async function authenticatedFetch(url, options = {}) {
  const token = getToken();

  console.log("📡 Making authenticated request to:", url);
  console.log("🔑 Using token:", token ? "Token exists" : "NO TOKEN");

  if (!token) {
    console.error("❌ NO TOKEN - WOULD REDIRECT");
    // TEMPORARILY COMMENTED OUT
    // window.location.href = '/login';
    throw new Error("Not authenticated");
  }

  const headers = {
    ...options.headers,
    Authorization: `Bearer ${token}`,
    "Content-Type": "application/json",
  };

  console.log("📤 Request headers:", headers);

  const response = await fetch(url, { ...options, headers });

  console.log("📥 Response status:", response.status);

  // If 401, clear token and redirect
  if (response.status === 401) {
    console.error("❌ 401 UNAUTHORIZED - WOULD REDIRECT");
    console.log("Token that failed:", token);
    clearToken();
    // TEMPORARILY COMMENTED OUT
    // window.location.href = '/login';
    throw new Error("Session expired");
  }

  return response;
}
