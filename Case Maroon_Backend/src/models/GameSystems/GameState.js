// models/GameState.js

import { Player } from "./Player.js";
import { ReconPosition } from "../Miscellaneous/index.js";
import { MapConfig } from "../WorldMap/MapConfig.js";
import { Building } from "../Buildings/index.js";
import { Unit } from "../Units/index.js";

export class GameState {
  constructor() {
    // this.units = new Map(); // Array of UnitModel
    // this.buildings = new Map(); // Array of BuildingModel
    this.players = [];
    this.buildings = [];
    this.units = [];
    this.worldMap = null;
  }

  initGame(worldMap) {
    this.players = [];
    this.worldMap = worldMap;

    let np = new Player();
    let positions = [];
    console.log(worldMap.gridSize);

    let gridSize = worldMap.gridSize;

    for (let x = 0; x < gridSize.x / 2; x++) {
      for (let y = 0; y < gridSize.y; y++) {
        let pos = { x, y };
        positions.push(new ReconPosition(pos, 1, 0));
      }
    }

    np.reconPositions = positions;
    this.players.push(np);
  }

  updateVisionAround(gridPos, distance, unitRecon, buildingRecon) {
    const reconPos = ReconPosition.createReconPosition(
      gridPos,
      distance,
      unitRecon,
      buildingRecon
    );
    const player = this.players[0];

    player.addReconPositions(reconPos);
    player.capturePosition(gridPos);
  }

  addBuilding(building) {
    // Check if a building already exists at this position
    const exists = this.buildings.some(
      (b) =>
        b.gridPosition.x === building.gridPosition.x &&
        b.gridPosition.y === building.gridPosition.y
    );

    if (!exists) {
      this.buildings.push(building);

      const gp = building.gridPosition;
      const distance = Building.reconScope;
      const ur = 0;
      const br = Building.reconLevel;

      this.updateVisionAround(gp, distance, ur, br);

      return true;
    }

    return false;
  }

  spawnUnit(gridPos, unit) {
    // Check if a unit already exists at this position
    const exists = this.units.some(
      (u) => u.gridPosition.x === gridPos.x && u.gridPosition.y === gridPos.y
    );

    if (!exists) {
      this.units.push(unit);
      unit.gridPosition = gridPos;

      const gp = unit.gridPosition;
      const distance = Unit.reconScope;
      const ur = Unit.reconLevel;
      const br = 0;

      this.updateVisionAround(gp, distance, ur, br);

      return true;
    }

    return false;
  }

  moveUnit(unit, toPos) {
    
    // Find the unit by unique identifier or coordinates
    const existingUnit = this.units.find((u) => u.id === unit.id);

    if (!existingUnit) {
      console.error("No matching unit found on server.");
      return false;
    }

    // Optional: prevent stacking units at destination
    // const exists = this.units.some(
    //   (u) => u.gridPosition.x === toPos.x && u.gridPosition.y === toPos.y
    // );
    // if (exists) {
    //   console.error("A unit already exists at the target position.");
    //   return false;
    // }

    const fromPos = existingUnit.gridPosition;

    // Remove vision from old position
    const oldRecon = ReconPosition.createReconPosition(
      fromPos,
      Unit.reconScope,
      Unit.reconLevel,
      0
    );
    
    const player = this.players[0];
    player.removeReconPositions(oldRecon);

    // Move unit
    existingUnit.gridPosition = toPos;

    // Add vision at new position
    const newRecon = ReconPosition.createReconPosition(
      toPos,
      Unit.reconScope,
      Unit.reconLevel,
      0
    );
    
    player.addReconPositions(newRecon);
    player.capturePosition(toPos);

    return true;
  }

  getGameState() {
    return this;
  }

  getPlayers() {
    return this.players;
  }

  toJSON() {
    return {
      players: this.players,
      gridSize: this.worldMap.gridSize,
    };
  }
}
