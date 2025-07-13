export class BiomeGenerator {
  constructor(biomeConfig) {

    this.waterThreshold = biomeConfig.waterThreshold;
    this.snowThreshold = biomeConfig.snowThreshold;
    
    this.biomeRules = biomeConfig.biomeRules.map(
      (rule) => new BiomeRules(rule)
    );
  }

  getMatchingBiome(temperature, rainfall, land = null) {
    let rule = {};

    if (land !== null && land < this.waterThreshold) {
      rule = this.biomeRules.find((r) => r.biomeType === "Ocean");
    }

    for (const r of this.biomeRules) {
      let ss = r;

      if (BiomeType[Number(r.biomeType)] == BiomeType[9]) {
        continue;
      }

      if (
        temperature >= r.tempRange.x &&
        temperature <= r.tempRange.y &&
        rainfall >= r.rainRange.x &&
        rainfall <= r.rainRange.y
      ) {
        rule = r;
      }
    }

    return new BiomeData({
      biomeType: rule.biomeType,
      temperature,
      rain: rainfall,
      moveCost: rule.traversalCost,
    });
  }
}

export class BiomeRules {
  constructor({ biomeType, tempRange, rainRange, traversalCost }) {
    this.biomeType = biomeType; // number denoting enum
    this.tempRange = tempRange; // vector2
    this.rainRange = rainRange; // vector2
    this.traversalCost =
      traversalCost instanceof BiomeTraversalCost
        ? traversalCost
        : new BiomeTraversalCost(traversalCost);
  }

  toJSON() {
    return {
      biomeType: BiomeType[this.biomeType] || "Unknown", // number → string
      tempRange: { x: this.tempRange.x, y: this.tempRange.y },
      rainRange: { x: this.rainRange.x, y: this.rainRange.y },
      traversalCost: this.traversalCost.toJSON?.() || this.traversalCost,
    };
  }
}

export class BiomeTraversalCost {
  constructor({ InfantryCost, TrackedCost }) {
    this.InfantryCost = InfantryCost; // number
    this.TrackedCost = TrackedCost; // number
  }
}

export class BiomeData {
  constructor({ biomeType, temperature, rain, moveCost }) {
    this.biomeType = biomeType; // number denoting enum
    this.temperature = temperature; // number
    this.rain = rain; // number
    this.moveCost = moveCost; // BiomeTraversalCost object
  }

  toJSON() {
    return {
      biomeType: BiomeType[this.biomeType] || "Unknown", // convert number → string
      temperature: this.temperature,
      rain: this.rain,
      moveCost: this.moveCost,
    };
  }
}

export class BiomeConfig {
  constructor({ waterThreshold, snowThreshold, biomeRules }) {
    this.waterThreshold = waterThreshold; // number
    this.snowThreshold = snowThreshold; // number
    this.biomeRules = biomeRules; // array of BiomeRules

  }
}

// Example enum mapping (should be imported or defined somewhere)
const BiomeType = {
  0: "Tundra",
  1: "Taiga",
  2: "SnowForest",
  3: "Grassland",
  4: "DeciduousForest",
  5: "Swamp",
  6: "Desert",
  7: "Savannah",
  8: "Rainforest",
  9: "Ocean",
};
