
import express from "express";
import cors from "cors";

import { GameState } from "./models/GameSystems/GameState.js";
import { gameRoutes } from "./routes/GameRoutes.js";
import { authRoutes } from "./routes/AuthRoutes.js";  

const app = express();
const PORT = 3001;

app.use(cors());
app.use(express.json());

const gameState = new GameState();

app.use("/api/auth", authRoutes);
app.use("/api", gameRoutes(gameState));

// ✅ Add this route to return game state for the frontend
app.get("/game-state", (req, res) => {
  res.json({
    units: gameState.getUnits(),
  });
});

app.listen(PORT, () => {
  console.log(`🚀 Backend server running at http://localhost:${PORT}`);
});
