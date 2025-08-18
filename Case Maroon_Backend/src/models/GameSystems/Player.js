import {
  ReconPosition,
  ReconPositionSchema,
  Vector2IntSchema,
} from "../Miscellaneous/index.js";

export class Player {
  static nextId = 1;
  static reconScope = 3;
  static reconLevel = 2;

  constructor(pid, username) {
    this.id = pid;
    this.username = username;
    
    this.reconPositions = []; // Array of ReconPosition

    this.initState = true;
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

export const PlayerState = {
    0: "Loading",
    1: "Idle",
    2: "WaitingForHQPlacement",
    3: "WaitingForUnitPlacement",
    4: "WaitingForNext",
    
    Loading: 0,
    Idle: 1,
    WaitingForHQPlacement: 2,
    WaitingForUnitPlacement: 3,
    WaitingForNext: 4
};