using Assets.Scripts.Units;
using UnityEngine;

namespace CaseMaroon.Units
{
    public class Artillery : UnitData
    {
        public Artillery(Sprite image)
        {
            Artillery def = DefaultUnitData.DefaultArtillery;
            CopyFields(def);
            Image = image;
        }
    }
}
