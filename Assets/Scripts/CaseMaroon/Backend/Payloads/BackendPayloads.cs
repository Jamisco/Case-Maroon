using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static CaseMaroon.Backend.GenericWrappers;
using UnityEngine;

namespace CaseMaroon.Backend
{
    public static class BackendPayloads
    {
        [Serializable]
        public struct SpawnUnitPayload
        {
            public Vector2Int gridPosition;
            public UnitDataWrap unit;

            public SpawnUnitPayload(Vector2Int pos, Unit data)
            {
                gridPosition = pos;
                unit = new UnitDataWrap(data);
            }
        }

    }
}
