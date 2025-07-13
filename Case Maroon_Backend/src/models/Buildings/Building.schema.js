import { Vector2IntSchema } from "../Miscellaneous/Vectors.schema.js";

// import { BuildingType } from "./Building.js";

// const allowedBuildingTypes = Object.values(BuildingType);

export const buildingSchema = {
  type: "object",
  properties: {
    buildingType: {
      type: "integer",
    },
    gridPosition: Vector2IntSchema,
  },

  required: ["buildingType", "gridPosition"],
  additionalProperties: false,
};
