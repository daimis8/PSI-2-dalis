import { useState } from "react";
import { useNavigate, Link } from "react-router-dom";
import { register } from "../services/authAPI";
import "../css/Auth.css";

function Register() {
    const [username, setUsername] = useState("");
    const [password, setPassword] = useState("");
    const [confirmPassword, setConfirmPassword] = useState("");
    const [error, setError] = useState("");
    const [loading, setLoading] = useState(false);
    const navigate = useNavigate();

    const handleSubmit = async (e) => {
        e.preventDefault();
        setError("");

        if (password !== confirmPassword) {
            setError("Passwords don't match");
            return;
        }

        setLoading(true);

        try {
            await register(username, password);
            navigate("/home/playlists");
        } catch (err) {
            setError(err.message);
            setLoading(false);
        }
    };

    return (
        <div className="auth-container">
            <div className="auth-brand">
                <h1 className="auth-logo">Amplio</h1>
                <p className="auth-tagline">The Best Jam Session Hub</p>
            </div>
            
            <div className="auth-box">
                <h1>Create Account</h1>
                <form onSubmit={handleSubmit} className="auth-form">
                    <input
                        type="text"
                        placeholder="Username"
                        className="auth-input"
                        value={username}
                        onChange={(e) => setUsername(e.target.value)}
                        required
                    />
                    <input
                        type="password"
                        placeholder="Password"
                        className="auth-input"
                        value={password}
                        onChange={(e) => setPassword(e.target.value)}
                        required
                    />
                    <input
                        type="password"
                        placeholder="Confirm Password"
                        className="auth-input"
                        value={confirmPassword}
                        onChange={(e) => setConfirmPassword(e.target.value)}
                        required
                    />
                    <button type="submit" className="auth-button" disabled={loading}>
                        {loading ? "Registering..." : "Register"}
                    </button>
                </form>
                {error && <p className="auth-error">{error}</p>}
                <p className="auth-footer">
                    Already have an account? <Link to="/login">Login here</Link>
                </p>
            </div>
        </div>
    );
}

export default Register;