import { useState } from "react";
import { uploadSongs } from "../services/songAPI";

function LoadDummyDataButton() {
  const [loading, setLoading] = useState(false);
  const [message, setMessage] = useState("");

  const handleLoadData = async () => {
    setLoading(true);
    setMessage("");
    
    try 
    {
      const result = await uploadSongs();
      setMessage(" Dummy data loaded successfully!");
      console.log("Upload result:", result);
    } 
    catch (err) 
    {
      console.error("Failed to load dummy data:", err);
      setMessage(" Failed to load dummy data");
    } 
    finally 
    {
      setLoading(false);
    }
  };

  return (
    <div className="load-dummy-data">
      <button 
        onClick={handleLoadData}
        disabled={loading}
        className="button-amplio">
        {loading ? "Loading..." : "Load dummy data"}
      </button>
      {message && <p className="load-message">{message}</p>}
    </div>
  );
}
export default LoadDummyDataButton;