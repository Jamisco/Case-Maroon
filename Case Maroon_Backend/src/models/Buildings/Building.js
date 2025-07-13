

export class Building {

    static reconScope = 3;
    static reconLevel = 5;
    
    constructor(buildingType, gridPosition) {
        
        this.buildingType = buildingType; // building type
        this.gridPosition = gridPosition; // Vector2Int
    }

    toJSON() {
        return {
            id: this.id,
            type: this.type,
            position: this.position,
            ownerId: this.ownerId,
        };
    } 
}


export const BuildingType = Object.freeze({
    Headquarters: "Headquarters",
    SupplyDepot: "SupplyDepot",
});