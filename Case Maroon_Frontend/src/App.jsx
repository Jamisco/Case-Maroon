// src/App.jsx
import React, { useEffect, useState } from "react";
import UnityApp from "./components/UnityApp";
import GameStateDisplay from "./components/GameState";

function App() {
  const [units, setUnits] = useState([]);
  const [selectedGrid, setSelectedGrid] = useState(null);

  useEffect(() => {
    fetch("http://localhost:3001/game-state")
      .then((res) => res.json())
      .then((data) => setUnits(data.units))
      .catch((err) => console.error("Failed to fetch game state:", err));
  }, []);

  return (
    <div
      style={{
        display: "flex",
        flexDirection: "column",
        alignItems: "center",
        justifyContent: "center",
        height: "100vh",
        width: "100vw",
        backgroundColor: "#000",
        color: "#fff",
      }}
    >
      <UnityApp />
      <GameStateDisplay units={units} />
      {selectedGrid && (
        <div style={{ marginTop: "10px" }}>
          <h3>Selected Grid Position:</h3>
          <p>X: {selectedGrid.x}, Y: {selectedGrid.y}</p>
        </div>
      )}
    </div>
  );
}

export default App;
