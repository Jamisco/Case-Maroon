import express from "express";
import Ajv from "ajv";
import crypto from "crypto";
import jwt from "jsonwebtoken";

import { Vector2, Vector3 } from "js-vectors";
import { Vector2IntSchema } from "../models/Miscellaneous/Vectors.schema.js";

import { buildingSchema } from "../models/Buildings/Building.schema.js";
import { MapConfigSchema } from "../models/WorldMap/MapConfig.schema.js";

import { Worldmap } from "../models/WorldMap/Worldmap.js";
import { activeGames } from "../models/GameSystems/ServerState.js";

import {
  validateMoveUnit,
  validateSpawnUnit,
} from "../models/Miscellaneous/index.js";

import { GameManager } from "../models/GameSystems/GameManager.js";

import { authenticateToken } from "../models/GameSystems/ServerState.js";

let worldMap = null;

const ajv = new Ajv();
const mapValidator = ajv.compile(MapConfigSchema);
const validateBuilding = ajv.compile(buildingSchema);

export function gameRoutes(activeGames) {
  const router = express.Router();

  // Helper to get the game manager
  function getGame(req, res) {
    const { gameId } = req.params;
    const game = activeGames.get(parseInt(gameId));
    if (!game) {
      res.status(404).json({
        success: false,
        message: "Game not found"
      });
      return null;
    }
    return game;
  }

  // -------------------- GET WorldMap --------------------
  router.get("/:gameId/getMapConfig", (req, res) => {
    const gameManager = getGame(req, res);
    
    console.log("Received request for MapConfig for gameId:", req.params.gameId);
    
    if (!gameManager) return;

    let seed = gameManager.worldMap.MapConfig.noiseConfig.landNoiseSettings.seed;
    console.log("MapConfig seed:", seed);
    
    return res.status(200).json(
      gameManager.worldMap.getMapConfig(),
    );
  });

  // -------------------- Validate NoiseHash --------------------
  router.get("/:gameId/validateNoiseHash", (req, res) => {
    const gameManager = getGame(req, res);
    
    console.log("Received request to validate noise hash for gameId:", req.params.gameId);
    
    if (!gameManager) return;

    const clientHash = req.query.noiseHash;
    if (!clientHash) {
      return res.status(400).json({
        success: false,
        message: "Missing noise hash in query",
      });
    }

    const valid = gameManager.validateNoiseHash(clientHash);

    if (valid) {
      gameManager.gridGenerated = true;
      return res.status(200).json({
        success: true,
        message: "Noise hash is valid",
        serverHash: gameManager.noiseHash,
      });
    } else {
      return res.status(400).json({
        success: false,
        message: "Noise hash is invalid",
        serverHash: gameManager.noiseHash,
      });
    }
  });

  //-------------------- POST Spawn Unit --------------------
  router.post("/:gameId/spawnunit", authenticateToken, (req, res) => {
    const gameManager = getGame(req, res);
    if (!gameManager) return;

    const data = req.body;
    const playerId = gameManager.getPlayerId(req.user.username);

    if (!validateSpawnUnit(data)) {
      return res.status(400).json({
        success: false,
        message: "Invalid spawn unit payload",
        errors: validateSpawnUnit.errors,
      });
    }

    try {
      if (gameManager.spawnUnit(playerId, data.gridPosition, data.unit)) {
        return res.status(200).json({
          success: true,
          message: "Unit Spawned Successfully",
        });
      }
      return res.status(400).json({
        success: false,
        message: "Unit already exists at this position or hex not owned",
      });
    } catch (err) {
      console.error(err);
      return res
        .status(500)
        .json({ success: false, message: "Failed to spawn unit" });
    }
  });

  // -------------------- POST Move Unit --------------------
  router.post("/:gameId/moveunit", authenticateToken, (req, res) => {
    const gameManager = getGame(req, res);
    if (!gameManager) return;

    const data = req.body;
    const playerId = gameManager.getPlayerId(req.user.username);

    if (!validateMoveUnit(data)) {
      return res.status(400).json({
        success: false,
        message: "Invalid move unit payload",
        errors: validateMoveUnit.errors,
      });
    }

    try {
      if (gameManager.moveUnit(playerId, data.unit, data.path)) {
        return res.status(200).json({
          success: true,
          message: "Unit moved successfully",
        });
      }
      return res.status(400).json({
        success: false,
        message: "Failed to move unit",
      });
    } catch (err) {
      console.error(err);
      return res.status(500).json({
        success: false,
        message: "Server error while moving unit",
      });
    }
  });

  // -------------------- POST Place Building --------------------
  router.post("/:gameId/placebuilding", authenticateToken, (req, res) => {
    const gameManager = getGame(req, res);
    if (!gameManager) return;

    const building = req.body;
    const playerId = gameManager.getPlayerId(req.user.username);

    if (!validateBuilding(building)) {
      return res.status(400).json({
        success: false,
        message: "Invalid building schema",
        errors: validateBuilding.errors,
      });
    }

    try {
      if (!gameManager.addBuilding(playerId, building)) {
        return res.status(400).json({
          success: false,
          message: "Building already exists or hex not owned",
        });
      }
      return res.status(200).json({
        success: true,
        message: "Building spawned successfully",
      });
    } catch (err) {
      return res.status(500).json({
        success: false,
        message: "Failed to spawn building",
        error: err.message,
      });
    }
  });

  // -------------------- GET Game State --------------------
  router.get("/:gameId/GetGameState", authenticateToken, (req, res) => {
    const gameManager = getGame(req, res);
    if (!gameManager) return;

    try {
      res.status(200).json(
        gameManager.toJson(),
      );
    } catch (error) {
      res.status(500).json({
        success: false,
        message: error.message || "Internal server error",
      });
    }
  });

  // -------------------- GET Biome (public) --------------------
  router.get("/:gameId/GetBiome", (req, res) => {
    const gameManager = getGame(req, res);
    if (!gameManager) return;

    const x = parseInt(req.query.x, 10);
    const y = parseInt(req.query.y, 10);
    if (isNaN(x) || isNaN(y)) {
      return res.status(400).json({
        success: false,
        error: "Invalid or missing 'x' and 'y' coordinates in query string",
      });
    }

    try {
      const biome = gameManager.worldMap.getBiomeData(x, y);
      if (!biome) {
        return res.status(404).json({
          success: false,
          error: "No biome data found at specified coordinates.",
        });
      }
      res.json(biome.toJSON());
    } catch (error) {
      console.error("Error in /GetBiome:", error);
      res.status(500).json({
        success: false,
        error: "Internal server error while retrieving biome data.",
      });
    }
  });


  // -------------------- POST Player State --------------------
  router.post("/:gameId/playerState", authenticateToken, (req, res) => {
    const gameManager = getGame(req, res);
    if (!gameManager) return;

    const playerId = req.user.username;
    const { newState } = req.body;

    try {
      const success = gameManager.updatePlayerState(playerId, newState);
      if (!success) {
        return res.status(404).json({
          success: false,
          message: "Player not found",
        });
      }
      return res.status(200).json({
        success: true,
        message: "Player state updated",
        playerId,
        newState,
      });
    } catch (err) {
      return res.status(400).json({
        success: false,
        message: err.message,
      });
    }
  });

  // -------------------- GET All Players Status --------------------
  router.get("/:gameId/playersStatus", authenticateToken, (req, res) => {
    const gameManager = getGame(req, res);
    if (!gameManager) return;

    try {
      const playersStatus = gameManager.getPlayersStatus();
      return res.status(200).json({
        success: true,
        players: playersStatus,
      });
    } catch (err) {
      return res.status(500).json({
        success: false,
        message: "Failed to get players' status",
        error: err.message,
      });
    }
  });

  return router;
}
