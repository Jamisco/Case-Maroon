
using UnityEngine;
using System.Collections.Generic;
using static CaseMaroon.Miscellaneous.GlobalData;

namespace CaseMaroon.GameSystem
{
    public class Player : MonoBehaviour
    {
        public int Id { get; set; }
        public string Name { get; set; }

        private Dictionary<Vector2Int, ReconPosition> ControlledPositions = new();

        public bool UpdateReconPosition(Vector2Int gridPos, int recon)
        {
            return false;
        }

        private void RequestDataFromSever()
        {
            // when the game starts, request data such as positions, units, building...
            // to be implemented
        }


    }
}
