using CaseMaroon.WorldMapUI;
using System.Collections.Generic;
using UnityEngine;

namespace CaseMaroon.WorldMap
{
    public class LogisticsOverlay : MonoBehaviour
    {
        public MasterRoadHex masterRoadHex;
        public GameObject overlayObj;

        // run this, the problem is the roads arent connecting properly
        public void RunSupplyLink(Vector2Int start, Vector2Int stop)
        {
            Mesh supplyLinkMesh = CreateSupplyLink(start, stop);

            if (supplyLinkMesh != null)
            {
                overlayObj.GetComponent<MeshFilter>().mesh = supplyLinkMesh;
            }
        }
        private Mesh CreateSupplyLink(Vector2Int start, Vector2Int stop)
        {
            List<Vector2Int> path = WorldUI.Instance.GetLogisticsPath(start, stop);
            if (path == null || path.Count < 2)
                return null;

            List<CombineInstance> combine = new List<CombineInstance>();

            for (int i = 0; i < path.Count; i++)
            {
                Vector2Int current = path[i];
                string roadMask = "000000";

                // Check previous tile to add road side
                if (i > 0)
                {
                    int sideFromPrev = GetHexDirectionIndex(path[i - 1], current);
                    if (sideFromPrev != -1)
                        roadMask = new string(ReplaceCharAt(roadMask.ToCharArray(), sideFromPrev, '1'));
                }

                // Check next tile to add road side
                if (i < path.Count - 1)
                {
                    int sideToNext = GetHexDirectionIndex(current, path[i + 1]);
                    if (sideToNext != -1)
                        roadMask = new string(ReplaceCharAt(roadMask.ToCharArray(), sideToNext, '1'));
                }

                string maskString = new string(roadMask);
                Mesh hexMesh = masterRoadHex.GenerateHexWithRoad(maskString);

                CombineInstance ci = new CombineInstance
                {
                    mesh = hexMesh,
                    transform = Matrix4x4.TRS(GridToWorld(current), Quaternion.identity, Vector3.one)
                };

                combine.Add(ci);
            }

            Mesh finalMesh = new Mesh();
            finalMesh.name = "SupplyLinkCombined";
            finalMesh.CombineMeshes(combine.ToArray(), true, true);
            return finalMesh;
        }

        private Vector3 GridToWorld(Vector2Int gridPos)
        {
            return Worldmap.Instance.gridManager.GridToWorldPostion(gridPos) + new Vector3(0, 0, 0.01f);
        }

        private char[] ReplaceCharAt(char[] input, int index, char newChar)
        {
            input[index] = newChar;
            return input;
        }

        // Returns direction index [0–5] from a to b on hex grid
        private int GetHexDirectionIndex(Vector2Int from, Vector2Int to)
        {
            Vector2Int diff = to - from;

            // Even-q layout, counter-clockwise: top-right, right, bottom-right, bottom-left, left, top-left
            Vector2Int[] directionsEven = new Vector2Int[]
            {
                new Vector2Int(1, 0),  // 0
                new Vector2Int(1, -1), // 1
                new Vector2Int(0, -1), // 2
                new Vector2Int(-1, -1),// 3
                new Vector2Int(-1, 0), // 4
                new Vector2Int(0, 1)   // 5
            };

            Vector2Int[] directionsOdd = new Vector2Int[]
            {
                new Vector2Int(1, 1),  // 0
                new Vector2Int(1, 0),  // 1
                new Vector2Int(0, -1), // 2
                new Vector2Int(-1, 0), // 3
                new Vector2Int(-1, 1), // 4
                new Vector2Int(0, 1)   // 5
            };

            Vector2Int[] dirs = (from.y % 2 == 0) ? directionsEven : directionsOdd;

            for (int i = 0; i < dirs.Length; i++)
            {
                if (dirs[i] == diff)
                    return i;
            }

            return -1; // not adjacent
        }


    }
}
