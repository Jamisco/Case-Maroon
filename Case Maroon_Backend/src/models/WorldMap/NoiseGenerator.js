import FastNoiseLite from "fastnoise-lite";
import { Vector2 } from "js-vectors";

export class NoiseGenerator {
  constructor(gridSize, noiseConfig) {
    this.gridSize =
      gridSize instanceof Vector2
        ? gridSize
        : new Vector2(gridSize.x, gridSize.y);

    this.landNoiseSettings =
      noiseConfig.landNoiseSettings instanceof NoiseSettings
        ? noiseConfig.landNoiseSettings
        : new NoiseSettings(noiseConfig.landNoiseSettings);

    // fractype typ and opther enums still have to be converted to the correct type

    this.rainNoiseSettings =
      noiseConfig.rainNoiseSettings instanceof NoiseSettings
        ? noiseConfig.rainNoiseSettings
        : new NoiseSettings(noiseConfig.rainNoiseSettings);

    this.tempNoiseSettings =
      noiseConfig.tempNoiseSettings instanceof NoiseSettings
        ? noiseConfig.tempNoiseSettings
        : new NoiseSettings(noiseConfig.tempNoiseSettings);

    // console.log("Land Noise Settings:", this.landNoiseSettings);
    // console.log("Rain Noise Settings:", this.rainNoiseSettings);
    // console.log("Temp Noise Settings:", this.tempNoiseSettings);

    this.noiseHash = 0;
  }

  ComputeNoise() {
    const width = this.gridSize.x;
    const height = this.gridSize.y;

    this.landNoiseSettings.init(this.gridSize);
    this.rainNoiseSettings.init(this.gridSize);
    this.tempNoiseSettings.init(this.gridSize);

    this.landValues = new Array(width);
    this.rainValues = new Array(width);
    this.tempValues = new Array(width);

    for (let x = 0; x < width; x++) {
      this.landValues[x] = new Array(height);
      this.rainValues[x] = new Array(height);
      this.tempValues[x] = new Array(height);

      for (let y = 0; y < height; y++) {
        const landVal = this.landNoiseSettings.getNoise(x, y, this.gridSize);
        const rainVal = this.rainNoiseSettings.getNoise(x, y, this.gridSize);
        const tempVal = this.tempNoiseSettings.getNoise(x, y, this.gridSize);

        this.landValues[x][y] = landVal;
        this.rainValues[x][y] = rainVal;
        this.tempValues[x][y] = tempVal;

        // Simple sum hash, could be improved
        this.noiseHash += landVal + rainVal + tempVal;
      }
    }
  }

  getNoiseHash() {
    return this.noiseHash;
  }

  getLandNoise(x, y) {
    return this.landValues[x][y];
  }

  getRainNoise(x, y) {
    return this.rainValues[x][y];
  }

  getTempNoise(x, y) {
    return this.tempValues[x][y];
  }
}

export class NoiseSettings {
  constructor({
    fractalType,
    noiseType,
    seed,
    frequency,
    multiplier,
    scale,
    fractal,
    minValue,
    maxValue,
    offset,
  }) {
    this.fractalType = fractalType;
    this.noiseType = noiseType;
    this.seed = seed;
    this.frequency = frequency;
    this.multiplier = multiplier;
    this.multiplier = Math.round(multiplier * 100) / 100;
    this.scale = scale;
    this.fractal = fractal;
    this.minValue = minValue;
    this.maxValue = maxValue;
    this.offset = offset; // assuming simple {x,y} object

    this.gridSize = null;
    this.noiseGenerator = null;
  }

  init(gridSize) {
    this.gridSize = gridSize;

    this.noiseGenerator = new FastNoiseLite();

    this.noiseGenerator.SetSeed(this.seed);

    const fractals = Object.values(FastNoiseLite.FractalType)[this.fractalType];

    const noiseTypes = Object.values(FastNoiseLite.NoiseType)[this.noiseType];

    this.noiseGenerator.SetFractalType(fractals);
    this.noiseGenerator.SetNoiseType(noiseTypes);
    this.noiseGenerator.SetFrequency(this.frequency);
  }

  getNoise(x, y, planetSize) {
    const nx = x / (planetSize.x + this.offset.x);
    const ny = y / (planetSize.y + this.offset.y);

    // Step 2: Get raw noise
    let tempNoise = this.noiseGenerator.GetNoise(nx, ny);

    // Step 4: Multiply
    tempNoise *= this.multiplier;

    // Step 5: Clamp
    tempNoise = Math.max(this.minValue, Math.min(this.maxValue, tempNoise));

    // Step 6: Final round
    tempNoise = Math.round(tempNoise * 1e2) / 1e2;

    return tempNoise;
  }

  toJson() {
    return {
      fractalType: this.fractalType,
      noiseType: this.noiseType,
      seed: this.seed,
      frequency: this.frequency,
      multiplier: this.multiplier,
      scale: this.scale,
      fractal: this.fractal,
      minValue: this.minValue,
      maxValue: this.maxValue,
      offset: this.offset,
    };
  }
}

export class NoiseConfig {
  constructor(landNoiseSettings, rainNoiseSettings, tempNoiseSettings) {
    this.landNoiseSettings = landNoiseSettings;
    this.rainNoiseSettings = rainNoiseSettings;
    this.tempNoiseSettings = tempNoiseSettings;
  }
}
