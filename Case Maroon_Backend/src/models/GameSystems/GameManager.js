// models/GameState.js

import { Player } from "./Player.js";
import { HexFunctions, ReconPosition } from "../Miscellaneous/index.js";
import { MapConfig } from "../WorldMap/MapConfig.js";
import { Building } from "../Buildings/index.js";
import { Unit } from "../Units/index.js";
import { Worldmap } from "../WorldMap/Worldmap.js";

import mapSettings from "../WorldMap/MapSettings.json" with { type: "json" };


export class GameManager {
  constructor(player1, player2) {
    this.gameId = Math.floor(10000 + Math.random() * 90000);
    this.players = [player1, player2]; // of class Player
    this.buildings = [];
    this.units = [];

    let seed = Math.floor(Math.random() * 1001);

    console.log("Choosing seed for world map: " + seed);

    mapSettings.noiseConfig.landNoiseSettings.seed = seed;

    this.worldMap = new Worldmap(mapSettings);
    this.worldMap.ComputeNoise();

    this.noiseHash = this.worldMap.noiseGenerator.getNoiseHash();

    this.gridGenerated = false;
    this.setPlayerPositions();
  }

  setPlayerPositions() {
    const position1 = [];
    const position2 = [];

    let gridSize = this.worldMap.gridSize;
    const halfX = Math.floor(gridSize.x / 2);

    // clear existing recon positions
    this.players[0].reconPositions = [];
    this.players[1].reconPositions = [];

    // Left half → player1
    for (let x = 0; x < halfX; x++) {
      for (let y = 0; y < gridSize.y; y++) {
        const pos = { x, y };
        position1.push(new ReconPosition(pos, 1, 0));
      }
    }

    // Right half → player2
    for (let x = halfX; x < gridSize.x; x++) {
      for (let y = 0; y < gridSize.y; y++) {
        const pos = { x, y };
        position2.push(new ReconPosition(pos, 1, 0));
      }
    }

    this.players[0].reconPositions = position1;
    this.players[1].reconPositions = position2;
  }

  setWorldMap(worldMap) {
    this.worldMap = worldMap;

    this.setPlayerPositions();
  }

  setGridGenerated(value) {
    this.gridGenerated = value;
  }

  validateNoiseHash(clientHash) {
    const tolerancePercent = 0.1;
    const difference = Math.abs(clientHash - this.noiseHash);

    const percentDifference = (difference / Math.abs(this.noiseHash)) * 100;

    return percentDifference <= tolerancePercent;
  }

  updatePlayerState(playerId, newState) {
    const player = this.getPlayerById(playerId);
    if (!player) return false;

    if (!Object.values(PlayerState).includes(newState)) {
      throw new Error(`Invalid PlayerState value: ${newState}`);
    }

    player.playerState = newState;
    return true;
  }

  getPlayerStates() {
    return this.players.map((player) => ({
      id: player.id,
      state: player.playerState,
    }));
  }

  getPlayerById(playerId) {
    return this.players.find((p) => p.id === playerId);
  }
  
  getPlayerByUsername(username) {
    return this.players.find((p) => p.username === username);
  }
  
  getPlayerId(username) {
    const player = this.getPlayerByUsername(username);
    return player ? player.id : null;
  }

  updateVisionAround(playerId, gridPos, distance, unitRecon, buildingRecon) {
    const reconPos = ReconPosition.createReconPosition(
      gridPos,
      distance,
      unitRecon,
      buildingRecon
    );
    const player = this.getPlayerById(playerId);
    if (!player) return;

    player.addReconPositions(reconPos);
    player.capturePosition(gridPos);
  }

  addBuilding(playerId, building) {
    const exists = this.buildings.some(
      (b) =>
        b.gridPosition.x === building.gridPosition.x &&
        b.gridPosition.y === building.gridPosition.y
    );
    if (exists) return false;

    let curPlayer = this.getPlayerById(playerId);
    if (!curPlayer) return false;

    // If this is the first building for the player, capture the position
    // to ensure they own the hex
    if (curPlayer.initState) {
      curPlayer.initState = false;
      curPlayer.capturePosition(building.gridPosition);
    }
    
    if (!curPlayer.ownsHex(building.gridPosition)) {
      console.error("Player does not own the hex for this building.");
      return false;
    } 

    this.buildings.push(building);

    const gp = building.gridPosition;
    const distance = Building.reconScope;
    const ur = 0;
    const br = Building.reconLevel;

    this.updateVisionAround(playerId, gp, distance, ur, br);

    if (this.gridGenerated) {
      let positions = HexFunctions.getSurroundingTiles(gp, distance);
      curPlayer.capturePositions(positions);
      this.gridGenerated = false;
    }

    return true;
  }

  spawnUnit(playerId, gridPos, unit) {
    const exists = this.units.some(
      (u) => u.gridPosition.x === gridPos.x && u.gridPosition.y === gridPos.y
    );
    if (exists) return false;

    let curPlayer = this.getPlayerById(playerId);
    if (!curPlayer || !curPlayer.ownsHex(gridPos)) {
      return false;
    }

    this.units.push(unit);
    unit.gridPosition = gridPos;

    const gp = unit.gridPosition;
    const distance = Unit.reconScope;
    const ur = Unit.reconLevel;
    const br = 0;

    this.updateVisionAround(playerId, gp, distance, ur, br);

    return true;
  }

  moveUnit(playerId, unit, path) {
    if (!Array.isArray(path) || path.length === 0) {
      console.error("Path is invalid or empty.");
      return false;
    }

    const existingUnit = this.units.find((u) => u.id === unit.id);

    if (!existingUnit) {
      console.error("No matching unit found on server.");
      return false;
    }

    const player = this.getPlayerById(playerId);
    if (!player) return false;

    const toPos = path[path.length - 1];

    // Capture all tiles within recon range along path
    for (const pos of path) {
      const recon = ReconPosition.createReconPosition(
        pos,
        Unit.reconScope,
        Unit.reconLevel,
        0
      );
      for (const reconPos of recon) {
        player.capturePosition(reconPos.gridPosition);
      }
    }

    existingUnit.gridPosition = toPos;
    return true;
  }

  toJSON() {
    return {
      players: this.players.map((p) => (p.toJSON ? p.toJSON() : p)),
      gridSize: this.worldMap.gridSize,
      buildings: this.buildings,
      units: this.units,
    };
  }
}
