import jwt from "jsonwebtoken";


export const activeGames = new Map();       // gameId → gameData
export const activeUsers = new Set();       // userId → userData
export const serverStats = { startTime: Date.now() }; // misc server stats

export const JWT_SECRET = "your-secret-key";

export function authenticateToken(req, res, next) {
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
