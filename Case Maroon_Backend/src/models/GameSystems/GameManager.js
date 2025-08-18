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
    
    // Map of gridPosition to Building
    this.buildings = new Map();
    
    // Map of gridPosition to Unit
    this.units = new Map();
    
    // Map of Grid Position to PlayerId
    this.ownedPositions = new Map(); 
    
    let seed = Math.floor(Math.random() * 1001);
    
    console.log("Choosing seed for world map: " + seed);

    mapSettings.noiseConfig.landNoiseSettings.seed = seed;
    
    this.worldMap = new Worldmap(mapSettings);
    this.worldMap.ComputeNoise();
    this.noiseHash = this.worldMap.noiseGenerator.getNoiseHash();
    this.gridGenerated = false;
    
    this.setPlayerPositions();
    
  }



  setWorldMap(worldMap) {
    
    this.worldMap = worldMap;
    this.setPlayerPositions();
  }

  validateNoiseHash(clientHash) {
    const tolerancePercent = 0.1;
    const difference = Math.abs(clientHash - this.noiseHash);

    const percentDifference = (difference / Math.abs(this.noiseHash)) * 100;

    let goodMap = percentDifference <= tolerancePercent;
    
    if (goodMap) {
      this.gridGenerated = true;
    }
     
    return goodMap;
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
    this.capturePosition(playerId, gridPos);
  }

  capturePosition(playerId, gridPos) {
    
    const player = this.getPlayerById(playerId);
    if (!player) return false;

    // Check if the position is already owned by the player
    const owned = this.ownedPositions.get(this.pk(gridPos)) || false;
    
    if( owned && owned === playerId) {
      return true; // Position already owned by this player
    }
    
    // If not owned, capture the position
    this.ownedPositions.set(this.pk(gridPos), playerId);
    
    let rp = new ReconPosition(gridPos, Player.reconLevel, 0);  
    player.addReconPosition(rp);
    
  }
  
  capturePositions(playerId, gridPositions) {
    
    for (const gridPos of gridPositions) {      
      this.capturePosition(playerId, gridPos);
    }
    
  }
  
  addBuilding(playerId, building) {
    const exists = this.buildings[building.gridPosition];
       
    if (exists) return false;

    if (!playerId) return false;

    let player = this.getPlayerById(playerId);
    
    // If this is the first building for the player, capture the position
    // to ensure they own the hex
    if (player.initState) {
      player.initState = false;
      this.capturePosition(playerId, building.gridPosition);
    }
    
    if (!this.ownsHex(playerId, building.gridPosition)) {
      this.capturePosition(playerId, building.gridPosition);
      
      // console.error("Player does not own the hex for this building.");
      // return false;
    }

    this.buildings.set(this.pk(building.gridPosition), building);

    const gp = building.gridPosition;
    const distance = Building.reconScope;
    const ur = 0;
    const br = Building.reconLevel;

    this.updateVisionAround(playerId, gp, distance, ur, br);

    if (this.gridGenerated) {
      
      let positions = HexFunctions.getSurroundingTiles(gp, distance);
      this.capturePositions(playerId, positions);
      this.gridGenerated = false;
    }

    return true;
  }

  spawnUnit(playerId, gridPos, unit) {
    
    const exists = this.units.get(this.pk(gridPos)) || false;
    if (exists) return false;

    if (!playerId || !this.ownsHex(playerId, gridPos)) {
      return false;
    }

    unit.gridPosition = gridPos;
    this.units.set(this.pk(gridPos), unit);

    const gp = unit.gridPosition;
    const distance = Unit.reconScope;
    const ur = Unit.reconLevel;
    const br = 0;

    this.updateVisionAround(playerId, gp, distance, ur, br);

    return true;
  }

  ownsHex(playerId, gridPosition) {
    const player = this.getPlayerById(playerId);
    if (!player) return false;
    let id = this.ownedPositions.get(this.pk(gridPosition)) || -1;
    
    return id === playerId;
  }
  
  pk(gridPosition) {
    return HexFunctions.gridKey(gridPosition);
  }
  
  moveUnit(playerId, unit, path) {
    
    if (!Array.isArray(path) || path.length == 0) {
      console.error("Path is invalid or empty.");
      return false;
    }

    const existingUnit = this.units.get(this.pk(unit.gridPosition));

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
        this.capturePosition(playerId, reconPos.gridPosition);
      }
    }

    this.units.delete(this.pk(unit.gridPosition));
    this.units.set(this.pk(toPos), unit);
    unit.gridPosition = toPos;
    
    return true;
  }

  getUnits() {
    
    return Array.from(this.units.values()).map((unit) => unit);
  }
  
toJson() {
  return {
    gameId: this.gameId,
    players: this.players.map((p) => ({
      id: p.id,
      username: p.username,
      reconPositions: p.reconPositions,
    })),
    buildings: Array.from(this.buildings.values()),
    units: Array.from(this.units.values()),

    ownedPositions: Array.from(this.ownedPositions.entries()).map(([key, playerId]) => {
      const [x, y] = key.split(",").map(Number);
      return {
        gridPosition: { x, y },
        playerId
      };
    }),

    noiseHash: this.noiseHash,
    gridGenerated: this.gridGenerated,
  };
}

}
