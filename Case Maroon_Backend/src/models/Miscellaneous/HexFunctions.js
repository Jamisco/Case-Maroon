// HexFunctions.js

// Assuming Vector2 is a simple {x, y} object or class with x,y numeric properties
// You can replace/add vector utility functions as needed.

const EvenRowOffsets = [
  { x: 0, y: 1 },    // 0
  { x: 1, y: 0 },    // 1
  { x: 0, y: -1 },   // 2
  { x: -1, y: -1 },  // 3
  { x: -1, y: 0 },   // 4
  { x: -1, y: 1 },   // 5
];

const OddRowOffsets = [
  { x: 1, y: 1 },    // 0
  { x: 1, y: 0 },    // 1
  { x: 1, y: -1 },   // 2
  { x: 0, y: -1 },   // 3
  { x: -1, y: 0 },   // 4
  { x: 0, y: 1 },    // 5
];

// Helper to add two Vector2 objects
function addVec2(a, b) {
  return { x: a.x + b.x, y: a.y + b.y };
}

export const HexFunctions = {
  getNeighbor(pos, side) {
    side = side % 6;
    const offsets = (pos.y % 2 === 0) ? EvenRowOffsets : OddRowOffsets;
    return addVec2(pos, offsets[side]);
  },

  getAllNeighbors(pos) {
    const neighbors = [];
    for (let i = 0; i < 6; i++) {
      neighbors.push(this.getNeighbor(pos, i));
    }
    return neighbors;
  },

  getOppositeSide(side) {
    return (side + 3) % 6;
  },

  getConnectingSide(from, to) {
    const neighbors = this.getAllNeighbors(from);
    for (let i = 0; i < neighbors.length; i++) {
      if (neighbors[i].x === to.x && neighbors[i].y === to.y) {
        return i;
      }
    }
    return -1; // Not directly connected
  },

  getSurroundingTiles(initialPosition, distance = 1) {
  const loopOrder = [1, 3, 4, 5, 6, 1, 2];
  const surroundingTiles = [];

  if (distance < 1) {
    distance = 1;
  }

  let currentPos = { ...initialPosition };
  let startPos = { ...initialPosition };

  let counter = 1;

  while (counter <= distance) {
    for (let s = 0; s < loopOrder.length; s++) {
      for (let i = 1; i <= counter; i++) {
        currentPos = HexFunctions.getNeighbor(currentPos, loopOrder[s]);
        surroundingTiles.push({ ...currentPos });

        if (s === 0) {
          startPos = { ...currentPos };
          break;
        }
      }
    }
    currentPos = { ...startPos };
    counter++;
  }

  return surroundingTiles;
}
};
