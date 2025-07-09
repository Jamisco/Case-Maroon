using UnityEngine;
using System.Collections.Generic;
using CaseMaroon.WorldMapUI;
using System.Linq;

namespace CaseMaroon.Miscellaneous
{
    public static class HexFunctions
    {
        // Offsets for even and odd rows (pointy-topped, odd-r)
        // Direction 0 = Top-Right, clockwise around
        private static readonly Vector2Int[] EvenRowOffsets = new Vector2Int[]
        {
            new Vector2Int(0, 1),  // 0
            new Vector2Int(1, 0),  // 1
            new Vector2Int(0, -1),  // 2
            new Vector2Int(-1, -1),  // 3
            new Vector2Int(-1, 0),  // 4
            new Vector2Int(-1, 1),  // 5
        };

        private static readonly Vector2Int[] OddRowOffsets = new Vector2Int[]
        {
            new Vector2Int(1, 1),   // 1 step north
            new Vector2Int(1, 0),   // 1 step east
            new Vector2Int(1, -1),   // 1 step south
            new Vector2Int(0, -1),   // southwest
            new Vector2Int(-1, 0),   // 2 steps southwest
            new Vector2Int(0, 1),   // west
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
            {
                neighbors[i] = GetNeighbor(pos, i);
            }

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

        public static List<Vector2Int> GetSurroundingTiles(Vector2Int initialPosition, int distance = 1)
        {
            int[] loopOrder = new int[] { 1, 3, 4, 5, 6, 1, 2 };

            List<Vector2Int> surroundingTiles = new List<Vector2Int>();

            if (distance < 1)
            {
                distance = 1;
            }

            Vector2Int currentPos = initialPosition;
            Vector2Int startPos = initialPosition;

            int counter = 1;

            while (counter <= distance)
            {
                for (int s = 0; s < loopOrder.Length; s++)
                {
                    for (int i = 1; i <= counter; i++)
                    {
                        currentPos = GetNeighbor(currentPos, loopOrder[s]);

                        surroundingTiles.Add(currentPos);

                        if (s == 0)
                        {
                            startPos = currentPos;
                            break;
                        }
                    }
                }

                currentPos = startPos;
                counter++;
            }

            return surroundingTiles;
        }

    }
}
