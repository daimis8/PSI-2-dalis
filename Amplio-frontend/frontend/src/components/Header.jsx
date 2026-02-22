import { NavLink, useNavigate } from "react-router-dom";
import { logout } from "../services/authAPI";
import "../css/Header.css";

function Header() {
    const navigate = useNavigate();

    const handleLogout = () => {
        logout();
        navigate("/login");
    };

    return (
        <header className="app-header">
            <div className="app-logo">
                Amplio
            </div>

            <nav className="app-tabs">
        <NavLink 
            to="/home/playlists"
            className={({ isActive }) => isActive ? "tab active" : "tab"}
        >
            Your Playlists
        </NavLink>

        <NavLink 
            to="/home/leaderboard"
            className={({ isActive }) => isActive ? "tab active" : "tab"}
        >
            Leaderboard
        </NavLink>
        
        <NavLink to="/home/create" className={({isActive}) => isActive ? "tab active" : "tab"}>
        Create Playlist
        </NavLink>

        <NavLink to="/home/join" className={({isActive}) => isActive ? "tab active" : "tab"}>
        Join Playlist
        </NavLink>

                <button 
                    className="tab signout-tab"
                    onClick={handleLogout}
                >
                    Sign out
                </button>
            </nav>
        </header>
    );
}

export default Header;
