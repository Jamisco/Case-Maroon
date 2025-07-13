export const Vector2Schema = {
  type: "object",
  properties: {
    x: { type: "number" },
    y: { type: "number" },
  },
  required: ["x", "y"],
  additionalProperties: false,
};

export const Vector2IntSchema = {
  type: "object",
  properties: {
    x: { type: "integer" },
    y: { type: "integer" },
  },
  required: ["x", "y"],
  additionalProperties: false,
};

export const Vector3Schema = {
  type: "object",
  properties: {
    x: { type: "number" },
    y: { type: "number" },
    z: { type: "number" },
  },
  required: ["x", "y", "z"],
  additionalProperties: false,
};
