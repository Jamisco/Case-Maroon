import express from "express";
import jwt from "jsonwebtoken";

import {
  QueueJoinResponse,
  QueueJoinSchema,
  validateQueueJoin,
} from "../models/Miscellaneous/index.js";

const router = express.Router();

// In production, use environment variable
const JWT_SECRET = "your-secret-key";
const users = new Set(); // Simple storage - just usernames
const matchmakingQueue = []; // Array to hold players in queue

function authenticateToken(req, res, next) {
  const authHeader = req.headers["authorization"];
  const token = authHeader && authHeader.split(" ")[1]; // Expecting "Bearer <token>"

  if (!token) {
    return res.status(401).json({ message: "No token provided" });
  }

  jwt.verify(token, JWT_SECRET, (err, user) => {
    if (err) {
      return res.status(403).json({ message: "Invalid token" });
    }
    req.user = user;
    next();
  });
}

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
  if (!users.has(cleanUsername)) {
    users.add(cleanUsername);
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
  if (users.has(username)) {
    users.delete(username);
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

  // Check if user is already in queue
  const alreadyInQueue = matchmakingQueue.some(
    (player) => player.username === username
  );

  if (alreadyInQueue) {
    const response = {
      ...QueueJoinResponse,
      success: false,
      message: "You are already in the queue",
      queuePosition:
        matchmakingQueue.findIndex((player) => player.username === username) +
        1,
      playersInQueue: matchmakingQueue.length,
    };

    if (!validateQueueJoin(response)) {
      console.error("Validation failed:", validateQueueJoin.errors);
    }

    return res.json(response);
  }

  // Add player to queue
  const queueEntry = {
    username: username,
    joinedAt: Date.now(),
  };

  matchmakingQueue.push(queueEntry);

  const response = {
    ...QueueJoinResponse,
    success: true,
    message: "Successfully joined the queue",
    queuePosition: matchmakingQueue.length, // user just joined at the end
    playersInQueue: matchmakingQueue.length,
  };

  if (!validateQueueJoin(response)) {
    console.error("Validation failed:", validateQueueJoin.errors);
  }

  console.log(
    `${username} joined queue. Queue size: ${matchmakingQueue.length}`
  );
  return res.json(response);
});

// ✅ Leave Queue Route
router.post("/queue/leave", authenticateToken, (req, res) => {
  const username = req.user.username;

  const initialLength = matchmakingQueue.length;
  const queueIndex = matchmakingQueue.findIndex(
    (player) => player.username === username
  );

  if (queueIndex === -1) {
    return res.json({
      success: false,
      message: "You are not in the queue",
      playersInQueue: matchmakingQueue.length,
    });
  }

  // Remove from queue
  matchmakingQueue.splice(queueIndex, 1);

  console.log(`${username} left queue. Queue size: ${matchmakingQueue.length}`);

  res.json({
    success: true,
    message: "Left queue successfully",
    playersInQueue: matchmakingQueue.length,
  });
});

// ✅ Get Queue Status Route
router.get("/queue/status", authenticateToken, (req, res) => {
   
  const username = req.user.username;
  console.log(`Queue status request for user: ${username}`);

  const queueIndex = matchmakingQueue.findIndex(
    (player) => player.username === username
  );
  const isInQueue = queueIndex !== -1;

  res.json({
    success: true,
    isInQueue: isInQueue,
    queuePosition: isInQueue ? queueIndex + 1 : 0,
    playersInQueue: matchmakingQueue.length,
    estimatedWaitTime: calculateEstimatedWaitTime(matchmakingQueue.length),
  });
});

// ✅ Match Players Function
function tryMatchPlayers() {
  if (matchmakingQueue.length >= 2) {
    const player1 = matchmakingQueue.shift();
    const player2 = matchmakingQueue.shift();

    const gameId = `game_${Date.now()}_${Math.random()
      .toString(36)
      .substr(2, 9)}`;

    // Create new game
    activeGames.set(gameId, {
      gameId: gameId,
      players: [
        { username: player1.username, playerId: 1 },
        { username: player2.username, playerId: 2 },
      ],
      status: "active",
      createdAt: Date.now(),
    });

    console.log(
      `Match created: ${gameId} - ${player1.username} vs ${player2.username}`
    );

    return {
      matched: true,
      gameId: gameId,
      player1: player1.username,
      player2: player2.username,
    };
  }

  return { matched: false };
}

// ✅ Helper Function
function calculateEstimatedWaitTime(queueLength) {
  if (queueLength <= 1) return "Finding opponent...";

  // Rough estimate: assume 1 match every 30 seconds
  const estimatedSeconds = Math.ceil(queueLength / 2) * 30;

  if (estimatedSeconds < 60) return `~${estimatedSeconds}s`;
  return `~${Math.ceil(estimatedSeconds / 60)}m`;
}

export { router as authRoutes };
