import { Vector2Schema } from "../Miscellaneous/Vectors.schema.js";

export const BiomeTraversalCostSchema = {
  type: "object",
  properties: {
    InfantryCost: { type: "integer" },
    TrackedCost: { type: "integer" },
  },
  required: ["InfantryCost", "TrackedCost"],
  additionalProperties: false,
};

export const BiomeRulesSchema = {
  type: "object",
  properties: {
    biomeType: { type: "integer" }, // enum as int
    tempRange: Vector2Schema,
    rainRange: Vector2Schema,
    traversalCost: BiomeTraversalCostSchema,
  },
  required: ["biomeType", "tempRange", "rainRange", "traversalCost"],
  additionalProperties: false,
};

export const BiomeDataSchema = {
  type: "object",
  properties: {
    biomeType: { type: "integer" },
    temperature: { type: "number" },
    rain: { type: "number" },
    moveCost: BiomeTraversalCostSchema,
  },
  required: ["biomeType", "temperature", "rain", "moveCost"],
  additionalProperties: false,
};

export const BiomeConfigSchema = {
  type: "object",
  properties: {
    waterThreshold: { type: "number" },
    snowThreshold: { type: "number" },
    biomeRules: {
      type: "array",
      items: BiomeRulesSchema,
    },
  },
  required: ["waterThreshold", "snowThreshold", "biomeRules"],
  additionalProperties: false,
};
