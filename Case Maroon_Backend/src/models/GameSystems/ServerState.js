

export const activeGames = new Map();       // gameId → gameData
export const activeUsers = new Set();       // userId → userData
export const serverStats = { startTime: Date.now() }; // misc server stats