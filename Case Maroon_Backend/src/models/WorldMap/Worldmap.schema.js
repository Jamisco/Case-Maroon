import { Vector2IntSchema } from "../Miscellaneous/Vectors.schema.js"
import { Vector2Schema } from "../Miscellaneous/Vectors.schema.js"

export const WorldMapConfigSchema = {

  type: "object",
  properties: {
    shapeScale: Vector2Schema,
    gridSize: Vector2IntSchema,
    chunkSize:Vector2IntSchema,
  },
  
  required: ["shapeScale", "gridSize", "chunkSize"],
  additionalProperties: false,
}