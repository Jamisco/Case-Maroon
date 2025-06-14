using Assets.Scripts.Units;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace CaseMaroon.Units
{
    /// <summary>
    /// Holds the data used to create units. Data such as sprites, unit type etc
    /// </summary>
    [Serializable]
    public class UnitCreator : MonoBehaviour
    {
        public UnitSettings[] unitSettings;

        [Serializable]
        public struct UnitSettings
        {
            public UnitType unitType;
            public Sprite[] Images;
        }

        public UnitData CreateUnit(UnitType ut)
        {
            UnitData unitData = null;
            Sprite sprite;

            switch (ut)
            {
                case UnitType.Infantry:
                    sprite = unitSettings.Where(x => x.unitType == UnitType.Infantry).FirstOrDefault().Images[0];

                    unitData = DefaultUnitData.CreateDefaultUnit<Infantry>(sprite);

                    int ran2DNumber = UnityEngine.Random.Range(10, 99);

                    unitData.UnitName = ran2DNumber.ToString() + " Infantry Division";

                    break;
                case UnitType.Armored:

                    sprite = unitSettings.Where(x => x.unitType == UnitType.Armored).FirstOrDefault().Images[0];


                    break;
                case UnitType.Artillery:

                    sprite = unitSettings.Where(x => x.unitType == UnitType.Artillery).FirstOrDefault().Images[0];

                    break;
            }

            return unitData;

        }
    }
}
