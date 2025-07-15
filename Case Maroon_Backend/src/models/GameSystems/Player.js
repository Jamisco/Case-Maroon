import {
  ReconPosition,
  ReconPositionSchema,
  Vector2IntSchema,
} from "../Miscellaneous/index.js";

export class Player {
  static nextId = 1;
  static reconScope = 3;
  static reconLevel = 1;

  constructor() {
    this.id = Player.nextId++;
    this.reconPositions = []; // Array of ReconPosition
    this.ownedPositions = []; // Array of Vector2Int
  }

  capturePosition(gridPos) {
    const exists = this.ownedPositions.some(
      (pos) => pos.x === gridPos.x && pos.y === gridPos.y
    );

    if (!exists) {
      this.ownedPositions.push(gridPos);
      let rp = new ReconPosition(gridPos, this.reconLevel, 0);

      this.addReconPosition(rp);
    }
  }

  capturePositions(gridPositions) {
    for (const gridPos of gridPositions) {
      const exists = this.ownedPositions.some(
        (pos) => pos.x === gridPos.x && pos.y === gridPos.y
      );

      if (!exists) {
        this.ownedPositions.push(gridPos);
        let rp = new ReconPosition(gridPos, this.reconLevel, 0);
        this.addReconPosition(rp);
      }
    }
  }

  // Helper to check if a position exists in an array
  _findReconIndex(gridPosition) {
    return this.reconPositions.findIndex(
      (rp) =>
        rp.gridPosition.x === gridPosition.x &&
        rp.gridPosition.y === gridPosition.y
    );
  }

  _findOwnedIndex(gridPosition) {
    return this.ownedPositions.findIndex(
      (pos) => pos.x === gridPosition.x && pos.y === gridPosition.y
    );
  }

  ownsHex(gridPosition) {
    return this.ownedPositions.some(
      (pos) => pos.x === gridPosition.x && pos.y === gridPosition.y
    );
  }

  addReconPosition(reconPos) {
    const idx = this._findReconIndex(reconPos.gridPosition);

    if (idx !== -1) {
      this.reconPositions[idx].addRecon(reconPos);
    } else {
      this.reconPositions.push(reconPos);
    }
  }

  removeReconPosition(reconPos) {
    const idx = this._findReconIndex(reconPos.gridPosition);

    if (idx !== -1) {
      this.reconPositions[idx].removeRecon(reconPos);

      if (this.reconPositions[idx].reconLevel <= 0) {
        this.reconPositions.splice(idx, 1);
      }
    }
  }

  addReconPositions(reconPositions) {
    for (const rp of reconPositions) {
      this.addReconPosition(rp);
    }
  }

  removeReconPositions(reconPositions) {
    for (const rp of reconPositions) {
      this.removeReconPosition(rp);
    }
  }
}
