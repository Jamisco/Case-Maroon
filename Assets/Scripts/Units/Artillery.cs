using Assets.Scripts.Units;
using UnityEngine;

namespace CaseMaroon.Units
{
    public class Artillery : UnitData
    {
        public override UnitType UnitType => UnitType.Artillery;

        public Artillery(Sprite image)
        {
            Artillery def = DefaultUnitData.DefaultArtillery;
            CopyFields(def);
            Image = image;
        }

    }
}
