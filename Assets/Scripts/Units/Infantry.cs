using Assets.Scripts.Units;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.VisualScripting;
using UnityEngine;

namespace CaseMaroon.Units
{
    public class Infantry : UnitData
    {
        public static Infantry CreateDefaultUnit(Sprite image)
        {
            Infantry inf = DefaultUnitData.DefaultInfantry;

            inf.Image = image;

            return inf;
        }
    }
}
