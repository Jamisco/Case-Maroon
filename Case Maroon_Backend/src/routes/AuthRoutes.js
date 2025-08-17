import express from "express";
import jwt from "jsonwebtoken";
import { activeGames, activeUsers } from "../models/GameSystems/ServerState.js";
import { Player } from "../models/GameSystems/Player.js";
import { GameManager } from "../models/GameSystems/GameManager.js";

import {
  authenticateToken,
  JWT_SECRET,
} from "../models/GameSystems/ServerState.js";

const router = express.Router();

// In production, use environment variable
const matchmakingQueue = []; // Array to hold players in queue
const pendingMatches = new Map(); // username -> gameId

// Simple login/register (creates user if doesn't exist)
router.post("/login", (req, res) => {
  const { username } = req.body;

  console.log(`Login attempt for user: ${username}`);

  // Basic null/undefined check
  if (!username) {
    return res.json({
      success: false,
      message: "Username is required",
      token: null,
      username: null,
    });
  }

  const cleanUsername = username.trim();

  // ✅ Same validation as Unity C#
  // Regex: Starts with a letter, followed by 2-15 alphanumeric characters (3-16 total)
  const usernamePattern = /^[A-Za-z][A-Za-z0-9]{2,15}$/;

  if (!usernamePattern.test(cleanUsername)) {
    return res.json({
      success: false,
      message:
        "Username must start with a letter and contain only letters and numbers.",
      token: null,
      username: cleanUsername,
    });
  }

  // Add user if they don't exist (simple registration)
  if (!activeUsers.has(cleanUsername)) {
    activeUsers.add(cleanUsername);
  }

  // Create JWT token
  const token = jwt.sign({ username: cleanUsername }, JWT_SECRET, {
    expiresIn: "24h",
  });

  res.json({
    success: true,
    token: token,
    username: cleanUsername,
    message: "Login successful",
  });
});

// Simple login/register (creates user if doesn't exist)
router.post("/logout", (req, res) => {
  const { username } = req.body;

  console.log(`Logout attempt for user: ${username}`);

  // Add user if they don't exist (simple registration)
  if (activeUsers.has(username)) {
    activeUsers.delete(username);
    res.json({
      success: true,
      message: `${username} Logged out`,
    });
  } else {
    res.json({
      success: false,
      message: `${username} was not found`,
    });
  }
});

router.post("/queue/join", authenticateToken, (req, res) => {
  const username = req.user.username;

  console.log(`${username} wants to join queue`);

  // ✅ Already in queue?
  if (matchmakingQueue.some((player) => player.username === username)) {
    const response = {
      success: false,
      message: "You are already in the queue",
    };

    return res.json(response);
  }

  // ✅ Add player to queue
  matchmakingQueue.push({ username, joinedAt: Date.now() });
  console.log(
    `${username} joined queue. Queue size: ${matchmakingQueue.length}`
  );

  const response = {
    success: true,
    message: "Successfully joined the queue",
  };

  tryMatchPlayers(); // Try to match players immediately

  return res.json(response);
});

function tryMatchPlayers() {
  if (matchmakingQueue.length >= 2) {
    const p1 = matchmakingQueue.shift();
    const p2 = matchmakingQueue.shift();

    const player1 = new Player(1, p1.username);
    const player2 = new Player(2, p2.username);

    const gameManager = new GameManager(player1, player2);
    activeGames.set(gameManager.gameId, gameManager);
    let gameId = gameManager.gameId;
    // Add both players to pending matches
    pendingMatches.set(player1.username, { gameId, notified: false });
    pendingMatches.set(player2.username, { gameId, notified: false });
  }
}

// ✅ Leave Queue Route
router.post("/queue/leave", authenticateToken, (req, res) => {
  const username = req.user.username;

  // Remove from normal queue
  const queueIndex = matchmakingQueue.findIndex((p) => p.username === username);
  if (queueIndex !== -1) matchmakingQueue.splice(queueIndex, 1);

  // Remove pending game if exists
  if (pendingMatches.has(username)) {
    const gameId = pendingMatches.get(username);
    const game = activeGames.get(gameId);

    if (game) {
      game.players.forEach((p) => pendingMatches.delete(p.username));
      activeGames.delete(gameId);
      console.log(
        `Pending game ${gameId} removed for players: ${game.players
          .map((p) => p.username)
          .join(", ")}`
      );
    }
  }

  console.log(`${username} left queue. Queue size: ${matchmakingQueue.length}`);

  res.json({
    success: true,
    message: "Left queue successfully",
    playersInQueue: matchmakingQueue.length,
  });
});

// ✅ Get Queue Status Route
router.get("/queue/status", (req, res) => {
  const token = req.headers["authorization"]?.split(" ")[1];

  if (!token) {
    console.log("Queue status request from guest");
    return res.json({
      success: true,
      gameFound: false,
      gameId: null,
      opponent: null,
      queuePosition: 0,
      playersInQueue: matchmakingQueue.length,
    });
  }

  authenticateToken(req, res, () => {
    const username = req.user.username;
    console.log(`Queue status request for user: ${username}`);

    let gameFound = false,
      gameId = null,
      opponent = null,
      queuePosition = 0;

    if (pendingMatches.has(username)) {
      
      const match = pendingMatches.get(username);
      const game = activeGames.get(match.gameId);

      if (game) {
        
         opponent = game.players.find(
          (p) => p.username !== username
        )?.username;
        
        gameFound = true;
        gameId = match.gameId;
        match.notified = true;
        
        let oppNotified = pendingMatches.get(opponent).notified;
        
        // if both players have been notified, remove them from pending matches
        if (oppNotified)
        {
          console.log(
            `Both players notified for gameId: ${gameId} (${username} vs ${opponent})`
          );
          
          pendingMatches.delete(username);
          pendingMatches.delete(opponent);
        }
      }
    } 
    
    res.json({
      success: true,
      playersInQueue: matchmakingQueue.length,
      
      gameFound,
      gameId,
      opponent,
    });
  });
});

// -------------------- Ping --------------------
router.get("/ping", (req, res) => {
  res.status(200).json({
    success: true,
    status: "ok",
    message: "Good Connection",
  });
});

export { router as authRoutes };
