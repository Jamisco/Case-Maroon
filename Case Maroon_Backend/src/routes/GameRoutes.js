import express from "express";
import Ajv from "ajv";

import { Vector2, Vector3 } from "js-vectors";
import { Vector2IntSchema } from "../models/Miscellaneous/Vectors.schema.js";

import { UnitSchema } from "../models/Units/Unit.schema.js";

import { buildingSchema } from "../models/Buildings/Building.schema.js";

import { MapConfigSchema } from "../models/WorldMap/MapConfig.schema.js";

import { Worldmap } from "../models//WorldMap/Worldmap.js";

import { HexFunctions, ReconPosition } from "../models/Miscellaneous/index.js";

import { Building } from "../models/Buildings/index.js";

let worldMap = null;

const ajv = new Ajv();

const unitValidator = ajv.compile(UnitSchema);
const validateBuilding = ajv.compile(buildingSchema);
const mapValidator = ajv.compile(MapConfigSchema);

const v2IntValidator = ajv.compile(Vector2IntSchema);

export function gameRoutes(gameState) {
  const router = express.Router();

  router.post("/GenerateGrid", (req, res) => {
    const mapConfig = req.body;

    console.log("Received map data:");

    if (!mapValidator(mapConfig)) {
      console.error(mapValidator.errors);
      return res
        .status(400)
        .json({ success: false, message: "Invalid map data schema" });
    }

    // console.log("Received Valid Map Data");

    worldMap = new Worldmap(mapConfig);
    worldMap.ComputeNoise();

    let noiseHash = worldMap.noiseGenerator.getNoiseHash();
    gameState.initGame(worldMap);

    // ✅ Send back a success response
    return res.status(200).json({
      success: true,
      message: "Map data received and validated successfully",
      noiseHash: noiseHash,
    });
  });

  router.post("/spawnunit", (req, res) => {
    const data = req.body;

    console.log("Received spawn unit request:", data.gridPosition);

    if (!unitValidator(data.unit)) {
      console.error(unitValidator.errors);
      return res
        .status(400)
        .json({ success: false, message: "Invalid unit schema" });
    }

    if (!v2IntValidator(data.gridPosition)) {
      console.error(unitValidator.errors);
      return res
        .status(400)
        .json({ success: false, message: "Invalid Vector schema" });
    }

    try {
      if (gameState.spawnUnit(data.gridPosition, data.unit)) {
        return res.status(200).json({
          success: true,
          message: "Unit Spawned Successfully",
        });
      }

      return res.status(400).json({
        success: false,
        message: "Unit already exists at this position",
      });
    } catch (err) {
      console.error(err);
      res.status(500).json({ success: false, message: "Failed to spawn unit" });
    }
  });

  router.post("/moveunit", (req, res) => {
    
    const { gridPosition, unit } = req.body;

    console.log("Received move unit request:", gridPosition);
    
    
    if (!v2IntValidator(gridPosition) || !unitValidator(unit)) {
      return res.status(400).json({
        success: false,
        message: "Invalid unit or vector format",
      });
    }

    try {
      if (gameState.moveUnit(unit, gridPosition)) {
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
      res.status(500).json({
        success: false,
        message: "Server error while moving unit",
      });
    }
  });

  router.post("/placebuilding", (req, res) => {
    const building = req.body;

    console.log("Received spawn building request:", building);

    if (!validateBuilding(building)) {
      console.error(validateBuilding.errors);
      return res.status(400).json({
        success: false,
        message: "Invalid building schema",
        errors: validateBuilding.errors,
      });
    }

    try {
      if (!gameState.addBuilding(building)) {
        return res.status(400).json({
          success: false,
          message: "Building already exists at this position",
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

    router.post("/GenerateGrid", (req, res) => {
      const mapConfig = req.body;

      // console.log("Received map data:", mapConfig);

      if (!mapValidator(mapConfig)) {
        console.error(mapValidator.errors);
        return res
          .status(400)
          .json({ success: false, message: "Invalid map data schema" });
      }

      console.log("Received Valid Map Data");

      const worldMap = new Worldmap(mapConfig);
      gameService.setWorldmap(worldMap);

      worldMap.ComputeNoise();

      let noiseHash = worldMap.noiseGenerator.getNoiseHash();

      // ✅ Send back a success response
      return res.status(200).json({
        success: true,
        message: "Map data received and validated successfully",
        noiseHash: noiseHash,
      });
    });

    return router;
  });

  router.get("/GetBiome", (req, res) => {
    console.log("Received GetBiome Request:", req.query);

    const x = parseInt(req.query.x, 10);
    const y = parseInt(req.query.y, 10);

    if (isNaN(x) || isNaN(y)) {
      return res.status(400).json({
        success: false,
        error: "Invalid or missing 'x' and 'y' coordinates in query string",
      });
    }

    try {
      const biome = worldMap.getBiomeData(x, y);

      console.log("Biome is:", JSON.stringify(biome.toJSON(), null, 2));

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

  router.get("/GetGameState", (req, res) => {
    console.log("Received GetGameState Request:", req.query);

    let gs = gameState.worldMap;

    try {
      res.json(gameState.toJSON());
    } catch (error) {
      res.json({
        success: false,
        GameStateResponse: gameState,
      });
    }
  });

  return router;
}
