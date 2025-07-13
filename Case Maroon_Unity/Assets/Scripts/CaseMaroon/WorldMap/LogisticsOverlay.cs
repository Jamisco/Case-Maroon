using CaseMaroon.Miscellaneous;
using CaseMaroon.Units;
using CaseMaroon.WorldMapUI;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace CaseMaroon.WorldMap
{
    public class LogisticsOverlay : MonoBehaviour
    {
        public MasterRoadHex masterRoadHex;
        public GameObject overlayObj;

        public void SupplyMapUnits(Vector2Int start)
        {
            Dictionary<Vector2Int, List<UnitInfoUI_1>> units = WorldUI.Instance.GetAllUnits();

            List<Mesh> supplyLinks = new List<Mesh>();

            foreach (var kvp in units)
            {
                Vector2Int unitPos = kvp.Key;
                List<UnitInfoUI_1> unitList = kvp.Value;

                if (unitList.Count > 0 && unitPos != start)
                {
                    Mesh supplyLinkMesh = CreateSupplyLink(start, unitPos);
                    if (supplyLinkMesh != null)
                    {
                        supplyLinks.Add(supplyLinkMesh);
                    }

                    foreach (UnitInfoUI_1 unit in unitList)
                    {
                        unit.SupplyUnit(50);
                    }
                }
            }

            Mesh superMesh = GlobalData.CombineMeshes(supplyLinks);
            overlayObj.GetComponent<MeshFilter>().mesh = superMesh;
        }

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

            List<CombineInstance> combinedMeshes 
                = new List<CombineInstance>();

            for (int i = 0; i < path.Count; i++)
            {
                Vector2Int current = path[i];
                char[] roadMask = "000000".ToCharArray();

                // Add connection to previous tile
                if (i > 0)
                {
                    int dirFromPrev 
                        = GetHexDirectionIndex(current, path[i - 1]);

                    if (dirFromPrev != -1)
                    {
                        roadMask[dirFromPrev] = '1';
                    }
                }

                // Add connection to next tile
                if (i < path.Count - 1)
                {
                    int dirToNext = GetHexDirectionIndex(current, path[i + 1]);

                    if (dirToNext != -1)
                    {
                        roadMask[dirToNext] = '1';
                    }
                }

                Color ranCol = new Color(Random.value, Random.value, Random.value, 1f);

                List<Color> colors = Enumerable
                    .Repeat(ranCol, 6)
                    .ToList();

                // Generate mesh for current hex with the calculated road mask

                Mesh hexMesh = masterRoadHex.GenerateHexWithRoad(new string(roadMask), colors);

                combinedMeshes.Add(new CombineInstance
                {
                    mesh = hexMesh,
                    transform = Matrix4x4.TRS(GridToWorld(current), Quaternion.identity, Vector3.one)
                });
            }

            // Combine all hex meshes into one
            Mesh finalMesh = new Mesh { name = "SupplyLinkCombined" };
            finalMesh.CombineMeshes(combinedMeshes.ToArray(), true, true);

            return finalMesh;
        }
        private Vector3 GridToWorld(Vector2Int gridPos)
        {
            return Worldmap.Instance.gridManager.GridToWorldPostion(gridPos) + new Vector3(0, 0, 0.01f);
        }
        private int GetHexDirectionIndex(Vector2Int from, Vector2Int to)
        {
            return HexFunctions.GetConnectingSide(from, to);
        }


        public void Test_Logi()
        {
            Vector2Int start = BuildingOverlay.Instance.buildings.First().Key;

            SupplyMapUnits(start);
        }
    }
}
