import { Vector2IntSchema } from "../Miscellaneous/Vectors.schema.js";

export const NoiseSettingsSchema = {
  type: "object",
  properties: {
    fractalType: {
      type: "integer",
      enum: [0, 1, 2, 3, 4, 5] // None, FBm, Ridged, PingPong, DomainWarpProgressive, DomainWarpIndependent
    },
    noiseType: {
      type: "integer",
      enum: [0, 1, 2, 3, 4, 5] // OpenSimplex2, OpenSimplex2S, Cellular, Perlin, ValueCubic, Value
    },
    seed: { type: "integer" },
    frequency: { type: "number" },
    multiplier: { type: "number" },
    scale: { type: "number" },
    fractal: {
      type: "integer",
      minimum: 0,
      maximum: 20
    },
    minValue: {
      type: "number",
      minimum: 0,
      maximum: 1
    },
    maxValue: {
      type: "number",
      minimum: 0,
      maximum: 1
    },
    offset: Vector2IntSchema,
  },
  required: [
    "fractalType", "noiseType", "seed", "frequency",
    "multiplier", "scale", "fractal", "minValue", "maxValue", "offset"
  ]
};


export const NoiseConfigSchema = {
 
  type: "object",
  properties: {
    landNoiseSettings: NoiseSettingsSchema,
    rainNoiseSettings: NoiseSettingsSchema,
    tempNoiseSettings: NoiseSettingsSchema
  },
  required: [
    "landNoiseSettings", "rainNoiseSettings", "tempNoiseSettings"
  ],
  additionalProperties: false
}