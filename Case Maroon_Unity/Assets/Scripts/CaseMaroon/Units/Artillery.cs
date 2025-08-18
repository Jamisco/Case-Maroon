using CaseMaroon.WorldMap;
using UnityEngine;

namespace CaseMaroon.Units
{
    public class Artillery : Unit
    {
        public override UnitType UnitType => UnitType.Artillery;
        public Artillery()
        {
            UnitName = UnityEngine.Random.Range(10, 99) + " Artillery Battery";
            UnitId = UnityEngine.Random.Range(10000, 99999);
            Image = GameAssets.Instance.GetUnitImage(UnitType.Infantry);
            AtkPoints = new CombatPoints()
            {
                AgainstInfantry = 30,
                AgainstArmored = 20,
                AgainstArtillery = 25,
                AgainstAircraft = 0,
                AgainstNaval = 15,
                AgainstStructure = 50
            };
            DefPoints = new CombatPoints()
            {
                AgainstInfantry = 5,
                AgainstArmored = 3,
                AgainstArtillery = 5,
                AgainstAircraft = 2,
                AgainstNaval = 3,
                AgainstStructure = 5
            };
            HealthPoints = 80;
            EnergyPoints = 60;
            MovementType = MovementType.Feet;
            MovementPoints = 60;
        }

    }
}
