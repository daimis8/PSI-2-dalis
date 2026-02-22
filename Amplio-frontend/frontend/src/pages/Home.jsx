import { Outlet } from "react-router-dom";
import Header from "../components/Header";
import "../css/Home.css";

function Home() {
    return (
        <>
            {/* Header with tabs */}
            <Header />

            {/* Content of the currently selected tab/page */}
            <main>
                <Outlet />
            </main>
        </>
    );
}

export default Home;
