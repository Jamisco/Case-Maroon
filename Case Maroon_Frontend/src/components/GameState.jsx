import React, { useState, useEffect } from "react";

function GameState() {
  const [units, setUnits] = useState([]);

  useEffect(() => {
    const fetchGameState = () => {
      fetch("http://localhost:3001/game-state")
        .then((res) => {
          if (!res.ok) throw new Error(`HTTP error: ${res.status}`);
          return res.json();
        })
        .then((data) => {
          console.log("✅ Game state fetched:", data);
          setUnits(data.units);
        })
        .catch((err) => {
          console.error("❌ Error fetching game state:", err);
          setUnits([]);
        });
    };

    fetchGameState(); // initial fetch
    const interval = setInterval(fetchGameState, 2000); // poll every 2s
    return () => clearInterval(interval);
  }, []);

  return (
    <div>
      <h2>Units on Map</h2>
      <ul>
        {units.length > 0 ? (
          units.map((unit) => (
            <li key={unit.id}>
              {unit.type} — Position: ({unit.position.x}, {unit.position.y})
            </li>
          ))
        ) : (
          <li>No units available</li>
        )}
      </ul>
    </div>
  );
}

export default GameState;
