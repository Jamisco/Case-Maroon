import { Vector2Schema } from '../Miscellaneous/Vectors.schema.js';

export const UnitSchema = {
  type: "object",
  properties: {
    unitId: { type: "integer" },
    playerId: { type: "integer", nullable: true },
    unitType: { type: "string" },
    gridPosition: Vector2Schema,
    movePoints: { type: "integer" }
  },
  required: ["unitId", "unitType", "gridPosition", "movePoints"],
  additionalProperties: false
};

