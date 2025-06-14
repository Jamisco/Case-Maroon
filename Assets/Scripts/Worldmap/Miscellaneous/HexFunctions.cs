using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace CaseMaroon.WorldMap
{
    /// <summary>
    /// Hex Help Functions.
    /// It must be understood that unless specified the parameters for all functions will be expecting      NON AXIAL COORDINATES!!
    /// </summary>
    public static class HexFunctions
    {
        public struct HexTile : IEquatable<HexTile>
        {
            // Distance travelled is the distance from the start to the current Tile 
            // Distance left is the distance left from the current to the target tile 

            public int distanceTravelled;
            public int distanceLeft;
            public int totalDistance;

            public Vector3Int AxialPosition;

            /// <summary>
            /// Before calling this, make sure you have called the SetAdjacentTiles method
            /// </summary>
            public List<HexTile> adjacentTiles;

            public HexTile(Vector3Int axialPosition, bool setAdjacent = false)
            {
                this.AxialPosition = axialPosition;
                distanceLeft = 0;
                distanceTravelled = 0;
                totalDistance = 0;
                adjacentTiles = new List<HexTile>();

                if (setAdjacent)
                {
                    SetAdjacentTiles();
                }
            }

            // Calling this in the constructor will cause a stack overflow
            // since for each adjacent tile you will have to get its adjecent tiles
            // then get the adjacent tiles of the adjacent tiles... etc
            public void SetAdjacentTiles()
            {
                foreach (Vector3Int pos in GetNeighbors(
                                    Axial.NonAxialOffset2D(AxialPosition)))
                {
                    Vector3Int newPos = Axial.AxialOffsetPosition(pos.x, pos.y);

                    adjacentTiles.Add(new HexTile(newPos));
                }
            }

            // We need this because although 2 different types might have thesame position
            // other instance variables might be different, thus the types wont be equal
            bool IEquatable<HexTile>.Equals(HexTile other)
            {
                return AxialPosition.Equals(other.AxialPosition);
            }
        }
        public struct Axial
        {
            public int x;
            public int y;
            public int z;

            /// <summary>
            /// Converts a position (X, Y) to axial coordinates. Returns Axial Struct
            /// </summary>
            /// <param name="x"></param>
            /// <param name="y"></param>
            /// <returns>A new Axial Class</returns>
            public static Axial AxialFromOffset(int x, int y)
            {
                Axial a = new Axial();
                a.x = x - (y - (y & 1)) / 2;
                a.y = y;
                a.z = -a.x - a.y;
                return a;
            }
            // Replace all non-axial functions to use Vector2Int instead of Vector3Int for non-axial coordinates

            // 1. Update AxialFromOffset overloads and AxialOffsetPosition overloads to use Vector2Int for non-axial coordinates
            public static Axial AxialFromOffset(Vector2Int pos)
            {
                Axial a = new Axial();
                a.x = pos.x - (pos.y - (pos.y & 1)) / 2;
                a.y = pos.y;
                a.z = -a.x - a.y;
                return a;
            }
            /// <summary>
            /// Converts a position (X, Y) to axial coordinates
            /// </summary>
            /// <param name="x"></param>
            /// <param name="y"></param>
            /// <returns>The Vector3Int containing the new Axial position</returns>
            public static Vector3Int AxialOffsetPosition(int x, int y)
            {
                Axial a = new Axial();
                a.x = x - (y - (y & 1)) / 2;
                a.y = y;
                a.z = -a.x - a.y;

                return a.Coordinates;
            }
            /// <summary>
            /// Converts a position (X, Y) to axial coordinates
            /// </summary>
            /// <param name="x"></param>
            /// <param name="y"></param>
            /// <returns>The Vector3Int containing the new Axial position</returns>
            public static Vector3Int AxialOffsetPosition(Vector2Int pos)
            {
                return AxialOffsetPosition(pos.x, pos.y);
            }

            /// <summary>
            /// Converts an Axial position to a Non Axial position
            /// </summary>
            /// <param name="axialOffset"></param>
            /// <returns></returns>
            public static Vector3Int NonAxialOffset(Vector3Int axialOffset)
            {
                int x = axialOffset.x + ((axialOffset.y - (axialOffset.y & 1)) / 2);

                return new Vector3Int(x, axialOffset.y, 0);
            }

            /// <summary>
            /// Converts an Axial position to a 2D Non Axial position
            /// </summary>
            /// <param name="axialOffset"></param>
            /// <returns></returns>
            public static Vector2Int NonAxialOffset2D(Vector3Int axialOffset)
            {
                int x = axialOffset.x + ((axialOffset.y - (axialOffset.y & 1)) / 2);

                return new Vector2Int(x, axialOffset.y);
            }

            public Vector3Int Coordinates { get { return new Vector3Int(x, y, z); } }
        }

        public static Vector2Int MapHexSize { get; set; }

        // 2. Update FindPath to use Vector2Int for start and stop (non-axial)
        public static List<HexTile> FindPath(Vector2Int start, Vector2Int stop, Vector3Int maxSize)
        {
            HexTile startPoint = new HexTile(Axial.AxialOffsetPosition(start.x, start.y));
            HexTile stopPoint = new HexTile(Axial.AxialOffsetPosition(stop.x, stop.y));

            List<HexTile> openPathTiles = new List<HexTile>();
            List<HexTile> closedPathTiles = new List<HexTile>();
            List<HexTile> closestTiles = new List<HexTile>();

            HexTile currentTile = startPoint;
            currentTile.distanceTravelled = 0;
            currentTile.distanceLeft = GetEstimatedPathCost(startPoint.AxialPosition, stopPoint.AxialPosition, maxSize);

            openPathTiles.Add(currentTile);
            HexTile tempTile;

            while (openPathTiles.Count != 0)
            {
                openPathTiles = openPathTiles.OrderBy(x => x.totalDistance)
                    .ThenBy(x => x.distanceLeft).ToList();

                tempTile = openPathTiles[0];

                closestTiles = openPathTiles.Where(x => (x.distanceLeft == tempTile.distanceLeft)
                                            && (x.totalDistance == tempTile.totalDistance))
                                .OrderBy(x => XDistance(stopPoint.AxialPosition, x.AxialPosition)).ToList();

                currentTile = closestTiles[0];

                openPathTiles.Remove(currentTile);
                closedPathTiles.Add(currentTile);

                int distanceTravelled = currentTile.distanceTravelled + 1;

                if (closedPathTiles.Contains(stopPoint))
                {
                    break;
                }

                currentTile.SetAdjacentTiles();

                for (int i = 0; i < currentTile.adjacentTiles.Count; i++)
                {
                    HexTile adjacentTile = currentTile.adjacentTiles[i];

                    if (closedPathTiles.Contains(adjacentTile))
                    {
                        continue;
                    }

                    if (!(openPathTiles.Contains(adjacentTile)))
                    {
                        HexTile adj = adjacentTile;

                        adj.distanceTravelled = distanceTravelled;
                        adj.distanceLeft = GetEstimatedPathCost(adjacentTile.AxialPosition, stopPoint.AxialPosition, maxSize);
                        openPathTiles.Add(adj);
                    }
                    else if (adjacentTile.totalDistance > distanceTravelled + adjacentTile.distanceLeft)
                    {
                        adjacentTile.distanceTravelled = distanceTravelled;
                    }
                }
            }

            List<HexTile> finalPathTiles = new List<HexTile>();

            if (closedPathTiles.Contains(stopPoint))
            {
                currentTile = closedPathTiles.Last();
                currentTile.SetAdjacentTiles();

                finalPathTiles.Add(currentTile);

                for (int i = currentTile.distanceTravelled - 1; i >= 0; i--)
                {
                    currentTile = closedPathTiles.Find(x => x.distanceTravelled == i && currentTile.adjacentTiles.Contains(x));
                    finalPathTiles.Add(currentTile);
                }

                finalPathTiles.Reverse();
            }

            return finalPathTiles;

            int XDistance(Vector3Int target, Vector3Int currentPosition)
            {
                if (target.y == currentPosition.y)
                {
                    Debug.Log("Same Row");
                    Debug.Log(currentPosition.ToString());
                    return 0;
                }

                target = Axial.NonAxialOffset(target);
                currentPosition = Axial.NonAxialOffset(currentPosition);

                int temp1;

                temp1 = Mathf.Abs(target.x - currentPosition.x);

                return temp1;
            }
        }

        /// <summary>
        /// Returns estimated path cost from given start position to target position of hex tile using Manhattan distance.
        /// </summary>
        /// <param name="startPosition">Start position.</param>
        /// <param name="targetPosition">Destination position.</param>
        /// <param name="isAxial">Default is true, set to false if the parameters are non axial coordinates.</param>
        /// 
        private static int GetEstimatedPathCost(Vector3Int startPosition, Vector3Int targetPosition, Vector3Int maxSize, bool isAxial = true)
        {
            if (isAxial)
            {
                startPosition = Axial.NonAxialOffset(startPosition);
                targetPosition = Axial.NonAxialOffset(targetPosition);

                return CalculateDistance(startPosition, targetPosition, maxSize, Edges.Horizontal);
            }
            else
            {
                return CalculateDistance(startPosition, targetPosition, maxSize, Edges.Horizontal);
            }
            // this method can wrap and not wrap


            // the below code works if you are not wrapping...
            //return Mathf.Max(Mathf.Abs(startPosition.Z - targetPosition.Z), Mathf.Max(Mathf.Abs(startPosition.X - targetPosition.X), Mathf.Abs(startPosition.Y - targetPosition.Y)));
        }

        public enum Edges { None, Horizontal, Vertical, Both }

        public static int CalculateDistanceAsPercent(Vector3Int start, Vector3Int stop, Vector3Int maxSize, Edges edges = Edges.Horizontal, bool circle = true)
        {
            float distanceX = 0;
            float distanceY = 0;

            // When wrapping values across the map for any axis, do not account for the Z values
            // since wrapping only takes place across a 2d axis, we only need to account for X and Y axis
            switch (edges)
            {
                case Edges.None:
                    return (int)Vector3Int.Distance(start, stop);
                case Edges.Horizontal:

                    distanceX = GetWrappedShortestDistance(maxSize.x, start.x, stop.x);
                    distanceY = GetShortestDistance(maxSize.y, start.y, stop.y);

                    break;
                case Edges.Vertical:
                    distanceX = GetShortestDistance(maxSize.x, start.x, stop.x);
                    distanceY = GetWrappedShortestDistance(maxSize.y, start.y, stop.y);
                    break;
                case Edges.Both:
                    distanceX = GetWrappedShortestDistance(maxSize.x, start.x, stop.x);
                    distanceY = GetWrappedShortestDistance(maxSize.y, start.y, stop.y);
                    break;
                default:
                    break;
            }

            float wrapedDistance;


            if (circle)
            {
                // do pythagoreom
                wrapedDistance = Mathf.Sqrt(Mathf.Pow(distanceX, 2) + Mathf.Pow(distanceY, 2));
            }
            else
            {
                // just add both distances
                wrapedDistance = Mathf.Max(distanceX, distanceY);
            }

            return Mathf.RoundToInt(wrapedDistance * 100);

            #region
            // Credits
            // https://blog.demofox.org/2017/10/01/calculating-the-distance-between-points-in-wrap-around-toroidal-space/
            #endregion

            static float GetWrappedShortestDistance(int length, int start, int stop)
            {
                // get the new distance here
                float distance = Mathf.Abs(start - stop);

                float halfLength = Mathf.CeilToInt(length / 2);

                if (distance > halfLength)
                {
                    distance = length - distance;
                }

                return distance / length;
            }

            static float GetShortestDistance(int length, int start, int stop)
            {
                return Mathf.Abs(start - stop) / length;
            }

        }

        public static int CalculateDistance(Vector3Int start, Vector3Int stop, Vector3Int maxSize, Edges edges = Edges.Horizontal)
        {

            Vector3Int start1 = Axial.NonAxialOffset(start);
            Vector3Int stop1 = Axial.NonAxialOffset(stop);

            start = Axial.AxialOffsetPosition(start.x, start.y);
            stop = Axial.AxialOffsetPosition(stop.x, stop.y);

            int halfWidth = Mathf.CeilToInt(MapHexSize.y / 2);

            int distanceX = 0;
            int distanceY = 0;

            // When wrapping values across the map for any axis, do not account for the Z values
            // since wrapping only takes place across a 2d axis, we only need to account for X and Y axis
            switch (edges)
            {
                case Edges.None:
                    return (int)Vector3Int.Distance(start, stop);
                case Edges.Horizontal:

                    distanceX = GetWrappedShortestDistance(maxSize.x, start.x, stop.x);
                    distanceY = GetShortestDistance(start.y, stop.y);

                    break;
                case Edges.Vertical:
                    distanceX = GetShortestDistance(start.x, stop.x);
                    distanceY = GetWrappedShortestDistance(maxSize.y, start.y, stop.y);
                    break;
                case Edges.Both:
                    distanceX = GetWrappedShortestDistance(maxSize.x, start.x, stop.x);
                    distanceY = GetWrappedShortestDistance(maxSize.y, start.y, stop.y);
                    break;
                default:
                    break;
            }

            // do pythagoreom
            float wrapedDistance = Mathf.Sqrt(Mathf.Pow(distanceX, 2) + Mathf.Pow(distanceY, 2));

            return Mathf.RoundToInt(wrapedDistance);

            #region
            // Credits
            // https://blog.demofox.org/2017/10/01/calculating-the-distance-between-points-in-wrap-around-toroidal-space/
            #endregion
            static int GetWrappedShortestDistance(int length, int start, int stop)
            {
                // get the new distance here
                int distance = Mathf.Abs(start - stop);

                int halfLength = Mathf.CeilToInt(length / 2);

                if (distance > halfLength)
                {
                    distance = length - distance;
                }

                return distance;
            }

            static int GetShortestDistance(int start, int stop)
            {
                return Mathf.Abs(start - stop);
            }

        }
        // 3. Update GetSurroundingTiles to use Vector2Int for initialPosition and return List<Vector2Int>
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
                        currentPos = GetNeighborHex(currentPos, loopOrder[s]);

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

        public static int GetOppositeNeighbor(int neighbor)
        {
            // will return numbers from 1 - 6

            if (neighbor <= 3)
            {
                return neighbor + 3;
            }
            else
            {
                return neighbor - 3;
            }
        }

        // 4. Overload GetNeighborHex for Vector2Int
        private static Vector2Int GetNeighborHex(Vector2Int curPos, int neighborSide)
        {
            Vector2Int tempPos = curPos;

            switch (neighborSide)
            {
                case 1:
                    if (tempPos.y % 2 == 1)
                    {
                        tempPos.x += 1;
                    }
                    tempPos.y += 1;
                    break;
                case 2:
                    tempPos.x += 1;
                    break;
                case 3:
                    if (tempPos.y % 2 == 1)
                    {
                        tempPos.x += 1;
                    }
                    tempPos.y -= 1;
                    break;
                case 4:
                    if (tempPos.y % 2 == 0)
                    {
                        tempPos.x -= 1;
                    }
                    tempPos.y -= 1;
                    break;
                case 5:
                    tempPos.x -= 1;
                    break;
                default:
                    if (tempPos.y % 2 == 0)
                    {
                        tempPos.x -= 1;
                    }
                    tempPos.y += 1;
                    break;
            }

            return tempPos;
        }

        // 5. Overload GetNeighbors for Vector2Int
        public static Vector2Int[] GetNeighbors(Vector2Int curPos)
        {
            Vector2Int[] neighbors = new Vector2Int[6];
            for (int i = 1; i <= 6; i++)
            {
                neighbors[i - 1] = GetNeighborHex(curPos, i);
            }
            return neighbors;
        }

        // 6. Update DrawHexShape to use Vector2Int for startPos and return List<Vector2Int>
        public static List<Vector2Int> DrawHexShape(int maxWidth, int minWidth, Vector2Int startPos)
        {
            return DrawFlatHeadHex(maxWidth, minWidth, startPos);
        }

        private static List<Vector2Int> DrawFlatHeadHex(int maxWidth, int minWidth, Vector2Int startPos)
        {
            List<Vector2Int> axialCoor = new List<Vector2Int>();

            int height = maxWidth - minWidth;

            int endXPos = startPos.x + minWidth;
            int startX = startPos.x;
            int startY = startPos.y + height;

            for (int y = startY; y >= startPos.y; y--)
            {
                for (int x = startX; x < endXPos; x++)
                {
                    axialCoor.Add(new Vector2Int(x, y));
                }
                endXPos++;
            }

            startX = startPos.x + height;
            startY = startPos.y - height;
            endXPos = startX + minWidth;

            for (int y = startY; y < startPos.y; y++)
            {
                for (int x = startX; x < endXPos; x++)
                {
                    axialCoor.Add(new Vector2Int(x, y));
                }
                startX--;
            }

            List<Vector2Int> returnHexes = new();

            foreach (Vector2Int pos in axialCoor)
            {
                // If you need to convert to non-axial, you can add a conversion here if needed
                returnHexes.Add(pos);
            }

            return returnHexes;
        }
    }
}