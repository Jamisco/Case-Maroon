import { BiomeGenerator } from "./BiomeGenerator.js";
import { NoiseGenerator } from "./NoiseGenerator.js";


export class Worldmap {

    constructor(MapConfig) {
        
        this.gridSize = MapConfig.worldmapConfig.gridSize;
                
        this.noiseGenerator = new NoiseGenerator(this.gridSize, MapConfig.noiseConfig);
           
        this.biomeGenerator = new BiomeGenerator(MapConfig.biomeConfig);
    }
    
    ComputeNoise() {
        this.noiseGenerator.ComputeNoise();     
    }
    
    getBiomeData(x, y) {
        const landNoise = this.noiseGenerator.getLandNoise(x, y);
        const rainNoise = this.noiseGenerator.getRainNoise(x, y);
        const tempNoise = this.noiseGenerator.getTempNoise(x, y);
                
        const biome = this.biomeGenerator.getMatchingBiome(tempNoise, rainNoise, landNoise);
        
        if (!biome) {
            throw new Error("No matching biome found.");
        }

        return biome;
    }
    
}

export class WorldMapConfig {
  
    constructor(shapeScale, gridSize, chunkSize) {
        
        this.shapeScale = shapeScale; // Number
        this.gridSize = gridSize; // Vector2
        this.chunkSize = chunkSize; // Vector2        
    }
}
