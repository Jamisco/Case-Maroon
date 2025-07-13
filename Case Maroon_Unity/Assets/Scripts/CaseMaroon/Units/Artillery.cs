using Assets.Scripts.Units;
using UnityEngine;

namespace CaseMaroon.Units
{
    public class Artillery : Unit
    {
        public override UnitType UnitType => UnitType.Artillery;

        public static Artillery CreateDefaultUnit(Sprite image)
        {
            Artillery art = DefaultUnitData.DefaultArtillery;

            art.Image = image;

            return art;
        }

    }
}
