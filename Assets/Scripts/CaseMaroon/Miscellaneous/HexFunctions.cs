using UnityEngine;
using System.Collections.Generic;

namespace CaseMaroon.Miscellaneous
{
    public static class HexFunctions
    {
        // Offsets for even and odd rows (pointy-topped, odd-r)
        // Direction 0 = Top-Right, clockwise around
        private static readonly Vector2Int[] EvenRowOffsets = new Vector2Int[]
        {
            new Vector2Int(1, 1),    // 0: Top-Right
            new Vector2Int(0, 1),    // 1: Top-Left
            new Vector2Int(1, 0),    // 2: Right
            new Vector2Int(-1, 0),   // 3: Left
            new Vector2Int(1, -1),   // 4: Bottom-Right
            new Vector2Int(0, -1),   // 5: Bottom-Left
        };

        private static readonly Vector2Int[] OddRowOffsets = new Vector2Int[]
        {
            new Vector2Int(1, 1),    // 0: Top-Right
            new Vector2Int(-1, 1),   // 1: Top-Left
            new Vector2Int(1, 0),    // 2: Right
            new Vector2Int(-1, 0),   // 3: Left
            new Vector2Int(1, -1),   // 4: Bottom-Right
            new Vector2Int(-1, -1),  // 5: Bottom-Left
        };

        public static Vector2Int GetNeighbor(Vector2Int pos, int side)
        {
            side %= 6;
            var offsets = (pos.y % 2 == 0) ? EvenRowOffsets : OddRowOffsets;
            return pos + offsets[side];
        }

        public static Vector2Int[] GetAllNeighbors(Vector2Int pos)
        {
            Vector2Int[] neighbors = new Vector2Int[6];
            for (int i = 0; i < 6; i++)
                neighbors[i] = GetNeighbor(pos, i);
            return neighbors;
        }

        public static int GetOppositeSide(int side) => (side + 3) % 6;

        public static int GetConnectingSide(Vector2Int from, Vector2Int to)
        {
            var neighbors = GetAllNeighbors(from);
            for (int i = 0; i < neighbors.Length; i++)
                if (neighbors[i] == to)
                    return i;
            return -1; // Not directly connected
        }

        public static List<Vector2Int> GetSurroundingTiles(Vector2Int center, int radius = 1)
        {
            List<Vector2Int> results = new List<Vector2Int>();

            if (radius < 1) return results;

            Vector2Int pos = center;

            // Move to start of ring (side 3: Left)
            for (int i = 0; i < radius; i++)
                pos = GetNeighbor(pos, 3);

            // Walk around the ring
            for (int side = 0; side < 6; side++)
            {
                for (int step = 0; step < radius; step++)
                {
                    pos = GetNeighbor(pos, side);
                    results.Add(pos);
                }
            }

            return results;
        }
    }
}
