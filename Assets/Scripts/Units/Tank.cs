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
        public Tank(Sprite image)
        {
            Tank def = DefaultUnitData.DefaultTank;
            CopyFields(def);
            Image = image;
        }
    }
}
