import Ajv from "ajv";

const ajv = new Ajv();

// ─── MODELS ──────────────────────────────────────────────


export const GameFoundResponse = {
  gameFound: false,
  message: "",
  gameId: null,
  opponent: null,
  playerId: null
};

export const QueueStatusResponse = {
  success: false,
  isInQueue: false,
  queuePosition: 0,
  playersInQueue: 0,
  estimatedWaitTime: null
};

// ─── SCHEMAS ─────────────────────────────────────────────

const QueueJoinSchema = {
  type: "object",
  properties: {
    success: { type: "boolean" },
    message: { type: "string" },
    gameId: { type: ["string", "null"] },
    opponent: { type: ["string", "null"] },
    queuePosition: { type: "integer", minimum: 0 },
    playersInQueue: { type: "integer", minimum: 0 }
  },
  required: ["success", "message", "queuePosition", "playersInQueue"],
  additionalProperties: false,
};

const GameFoundSchema = {
  type: "object",
  properties: {
    gameFound: { type: "boolean" },
    message: { type: "string" },
    gameId: { type: ["string", "null"] },
    opponent: { type: ["string", "null"] },
    playerId: { type: ["integer", "null"], minimum: 1, maximum: 2 }
  },
  required: ["gameFound", "message"],
  additionalProperties: false,
};

const QueueStatusSchema = {
  type: "object",
  properties: {
    success: { type: "boolean" },
    message: { type: "string" },
    
    queuePosition: { type: "integer", minimum: 0 },
    playersInQueue: { type: "integer", minimum: 0 },
    
    gameFound: { type: "boolean" },
    gameId: { type: ["string", "null"] },
    opponent: { type: ["string", "null"] },
  },
  
  required: ["success", "message", "queuePosition", "playersInQueue", "gameFound"],
  additionalProperties: false,
};

// ─── VALIDATORS ──────────────────────────────────────────

export const validateQueueJoin = ajv.compile(QueueJoinSchema);
export const validateGameFound = ajv.compile(GameFoundSchema);
export const validateQueueStatus = ajv.compile(QueueStatusSchema);

export {
  QueueJoinSchema,
  GameFoundSchema,
  QueueStatusSchema
};