

export class Unit {
  
  static reconScope = 2;
  static reconLevel = 2;
  
  constructor(id, type, position, mp) {
    this.unitId = id; // unique ID
    this.unitType = type; // e.g., "infantry", "tank"
    this.gridPosition = position; // Vector2Int object
    this.movePoints = mp; // movement points
  }

  moveTo(pos) {
    this.gridPosition = pos;
  }

  toJSON() {
    return {
      id: this.unitId,
      type: this.unitType,
      position: this.gridPosition,
      mp: this.movePoints,
    };
  }
}
