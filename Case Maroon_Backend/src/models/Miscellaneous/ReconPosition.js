// reconPosition.js

import { HexFunctions } from "./HexFunctions.js";

// --- Model Class ---
export class ReconPosition {
  /**
   * @param {{ gridPosition: {x: number, y: number}, reconLevel: number }} data
   */

  //Recon levels
    // 0 - Can see nothing - Fog
    // 1 - Can see Biome/Hex - Friend/Foe/Neutral
    // 2 - Can see Unit outline
    // 3 - Can see Unit Stats(with x accuracy)
    // 4 - Can see Unit Stats(with x++ accuracy)
    // 5 - Can see Unit Stats(with full accuracy)
  
  constructor(gridPosition, unitRecon = 0, buildingRecon = 0) {
    this.gridPosition = gridPosition;
    this.unitRecon = unitRecon;
    this.buildingRecon = buildingRecon;
  }

  addRecon(rp) {
    this.unitRecon += rp.unitRecon;
    this.buildingRecon += rp.buildingRecon;
  }

  removeRecon(rp) {
    this.unitRecon -= rp.unitRecon;
    this.buildingRecon -= rp.buildingRecon;
  }

  static createReconPosition(gridPos, distance, ur, br) {
    const surrPos = HexFunctions.getSurroundingTiles(gridPos, distance);

    // Also include the center tile itself (gridPos)
    surrPos.push(gridPos);

    const reconPositions = [];

    for (const pos of surrPos) {
      reconPositions.push(new ReconPosition(pos, ur, br));
    }

    return reconPositions;
  }
}

// --- JSON Schema ---
export const ReconPositionSchema = {
  type: "object",
  properties: {
    gridPosition: {
      type: "object",
      properties: {
        x: { type: "integer" },
        y: { type: "integer" },
      },
      required: ["x", "y"],
      additionalProperties: false,
    },
    reconLevel: {
      type: "integer",
      minimum: 0,
      maximum: 5,
    },
  },
  required: ["gridPosition", "reconLevel"],
  additionalProperties: false,
};
