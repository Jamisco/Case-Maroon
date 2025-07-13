
import { WorldMapConfigSchema } from "./WorldMap.schema.js";
import { NoiseConfigSchema } from "./NoiseGenerator.schema.js";
import { BiomeConfigSchema } from "./BiomeGenerator.schema.js";


export const MapConfigSchema = {
  type: "object",
  properties: {
    worldmapConfig: WorldMapConfigSchema,
    noiseConfig: NoiseConfigSchema,
    biomeConfig: BiomeConfigSchema,
  },
  required: ["worldmapConfig", "noiseConfig", "biomeConfig"],
  additionalProperties: false,
};
