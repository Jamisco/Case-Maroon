import Ajv from "ajv";

import { Vector2IntSchema } from "../Miscellaneous/Vectors.schema.js";
import { UnitSchema } from "../Units/Unit.schema.js";

const ajv = new Ajv();

// ─── SCHEMAS ─────────────────────────────────────────────

const SpawnUnitSchema = {
  type: "object",
  properties: {
    gridPosition: Vector2IntSchema,
    unit: UnitSchema,
  },
  required: ["gridPosition", "unit"],
  additionalProperties: false,
};

const MoveUnitSchema = {
  type: "object",
  properties: {
    unit: UnitSchema,
    path: {
      type: "array",
      items: Vector2IntSchema,
      minItems: 1,
    },
  },
  required: ["unit", "path"],
  additionalProperties: false,
};

// ─── VALIDATORS ──────────────────────────────────────────

export const validateSpawnUnit = ajv.compile(SpawnUnitSchema);
export const validateMoveUnit = ajv.compile(MoveUnitSchema);

// Optional: export schemas too if needed elsewhere
export {
  SpawnUnitSchema,
  MoveUnitSchema 
};
