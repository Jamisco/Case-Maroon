

export class Building {

    static reconScope = 3;
    static reconLevel = 5;
    
    constructor(buildingType, gridPosition) {
         
        this.buildingType = buildingType; // building type
        this.playerId = null; // Player ID who owns this building
        this.gridPosition = gridPosition; // Vector2Int
    }

    toJSON() {
        return {
            id: this.id,
            gridPost: this.position,
            ownerId: this.ownerId,
        };
    } 
}


export const BuildingType = Object.freeze({
    Headquarters: "Headquarters",
    SupplyDepot: "SupplyDepot",
});