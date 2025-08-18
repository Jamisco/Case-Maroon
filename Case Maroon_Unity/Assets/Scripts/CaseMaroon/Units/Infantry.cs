using CaseMaroon.WorldMap;
using UnityEngine;

namespace CaseMaroon.Units
{
    public class Infantry : Unit
    {
        public override UnitType UnitType => UnitType.Infantry;

        public Infantry()
        {
            UnitName = UnityEngine.Random.Range(10, 99) + " Infantry Battalion";
            UnitId = UnityEngine.Random.Range(10000, 99999);
            Image = GameAssets.Instance.GetUnitImage(UnitType.Infantry); // Default image can be set later
            AtkPoints = new CombatPoints()
            {
                AgainstInfantry = 10,
                AgainstArmored = 5,
                AgainstArtillery = 8,
                AgainstAircraft = 0,
                AgainstNaval = 3,
                AgainstStructure = 15
            };
            DefPoints = new CombatPoints()
            {
                AgainstInfantry = 5,
                AgainstArmored = 3,
                AgainstArtillery = 4,
                AgainstAircraft = 2,
                AgainstNaval = 2,
                AgainstStructure = 5
            };
            HealthPoints = 50;
            EnergyPoints = 30;
            MovementType = MovementType.Feet;
            MovementPoints = 100;
        }
    }
}
