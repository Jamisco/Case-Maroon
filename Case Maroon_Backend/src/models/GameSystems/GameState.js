// models/GameState.js

import { Player } from "./Player.js";
import { HexFunctions, ReconPosition } from "../Miscellaneous/index.js";
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
    this.initState = false;
  }

  initGame(worldMap) {
    
    this.players = [];
    this.buildings = [];
    this.units = [];
    this.initState = true;
    
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
      let curPlayer = this.players[0];

      // in the initial game state, we will allow the building to be placed, in a neutral position on player's side of the map.
      // if not, buildings can only be placed in owned positions
      if (!this.initState) {
        // if we are not in the initial state, the hex must be owned

        let oh = curPlayer.ownsHex(building.gridPosition);

        // if the player does not own the hex, the player cannot place a building there
        if (!oh) {
          return false;
        }
      }

      this.buildings.push(building);

      const gp = building.gridPosition;
      const distance = Building.reconScope;
      const ur = 0;
      const br = Building.reconLevel;

      this.updateVisionAround(gp, distance, ur, br);

      if (this.initState) {
        // in the initial state of the game, capture all hexes in the reconscope of the building

        let positions = HexFunctions.getSurroundingTiles(gp, distance);
        curPlayer.capturePositions(positions);

        this.initState = false;
      }

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
      // make sure user owns hex
      let curPlayer = this.players[0];

      if (!curPlayer.ownsHex(gridPos)) {
        return false;
      }

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

  moveUnit(unit, path) {
    
    if (!Array.isArray(path) || path.length === 0) {
      console.error("Path is invalid or empty.");
      return false;
    }

    // Find the unit by unique identifier
    const existingUnit = this.units.find((u) => u.id === unit.id);
    
    if (!existingUnit)
    {
      console.error("No matching unit found on server.");
      return false;
    }

    const toPos = path[path.length - 1];
    const player = this.players[0];

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
    
    // Move unit
    existingUnit.gridPosition = toPos;

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
