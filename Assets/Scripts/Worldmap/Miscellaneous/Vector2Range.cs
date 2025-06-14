using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts.Worldmap.Miscellaneous
{
    public class Vector2Range : PropertyAttribute
    {
        // Min/Max values for the X axis
        public readonly float MinX;
        public readonly float MaxX;
        // Min/Max values for the Y axis
        public readonly float MinY;
        public readonly float MaxY;

        public Vector2Range(float fMinX, float fMaxX, float fMinY, float fMaxY)
        {
            MinX = fMinX;
            MaxX = fMaxX;
            MinY = fMinY;
            MaxY = fMaxY;
        }
    }
}
