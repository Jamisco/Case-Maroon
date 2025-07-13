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

  addBuilding(building) {
    // Check if a building already exists at this position
    const exists = this.buildings.some(
      (b) =>
        b.gridPosition.x === building.gridPosition.x &&
        b.gridPosition.y === building.gridPosition.y
    );

    if (!exists) {
      this.buildings.push(building);

      let gp = building.gridPosition;
      let distance = Building.reconScope;
      let ur = 0;
      let br = Building.reconLevel;

      let reconPos = ReconPosition.createReconPosition(gp, distance, ur, br);

      let player = this.players[0];

      player.addReconPositions(reconPos);
      player.capturePosition(gp);

      return true;
    }

    return false;
  }

  spawnUnit(gridPos, unit) {
    // Check if a unit already exists at this position
    const exists = this.units.some(
      (u) =>
        u.gridPosition.x === gridPos.x &&
        u.gridPosition.y === gridPos.y
    );

    if (!exists) {
      
      this.units.push(unit);
      unit.gridPosition = gridPos;
      
      let gp = unit.gridPosition;
      let distance = Unit.reconScope;
      let ur = Unit.reconLevel;;
      let br = 0;

      let reconPos = ReconPosition.createReconPosition(gp, distance, ur, br);

      let player = this.players[0];
      
      player.addReconPositions(reconPos);

      return true;
      
    }

    return false;
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
