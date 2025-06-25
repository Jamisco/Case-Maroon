using Assets.Scripts.Units;
 using UnityEngine;

namespace CaseMaroon.Units
{
    public class Infantry : UnitData
    {
        public override UnitType UnitType => UnitType.Infantry;

        public static Infantry CreateDefaultUnit(Sprite image)
        {
            Infantry inf = DefaultUnitData.DefaultInfantry;

            inf.Image = image;

            return inf;
        }
    }
}
