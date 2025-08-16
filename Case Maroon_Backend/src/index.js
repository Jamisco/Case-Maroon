
import express from "express";
import cors from "cors";

import { GameManager } from "./models/GameSystems/GameManager.js";
import { gameRoutes } from "./routes/GameRoutes.js";
import { authRoutes } from "./routes/AuthRoutes.js";  
import { activeGames } from "./models/GameSystems/ServerState.js";

const app = express();
const PORT = 3001;

app.use(cors());
app.use(express.json());

app.use("/api/auth", authRoutes);
app.use("/api/game", gameRoutes(activeGames));

app.listen(PORT, () => {
  console.log(`🚀 Backend server running at http://localhost:${PORT}`);
});
