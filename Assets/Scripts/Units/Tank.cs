using Assets.Scripts.Units;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace CaseMaroon.Units
{
    public class Tank : UnitData
    {
        public override UnitType UnitType => UnitType.Armored;

        public static Tank CreateDefaultUnit(Sprite image)
        {
            Tank inf = DefaultUnitData.DefaultTank;

            inf.Image = image;

            return inf;
        }
    }
}
