
using UnityEngine;
using System.Collections.Generic;
using static CaseMaroon.Miscellaneous.GlobalData;
using static CaseMaroon.Backend.BackendResponses;
using System.Linq;
using Unity.VisualScripting;

namespace CaseMaroon.GameSystem
{
    public class Player
    {
        public int Id { get; set; }

        public HashSet<ReconPosition> ReconPositions { get; private set; }

        public HashSet<Vector2Int> OwnedPositions { get; private set; }

        public Player(PlayerResponse pr)
        {
            Id = pr.id;

            ReconPositions = new HashSet<ReconPosition>();
            OwnedPositions = new HashSet<Vector2Int>();

            if(pr.reconPositions.Count > 0)
            {
                ReconPositions.AddRange(pr.reconPositions);
            }

            if (pr.ownedPositions.Count > 0)
            {
                OwnedPositions.AddRange(pr.ownedPositions);
            }

        }

        public void UpdateReconPosition(List<ReconPosition> add, List<ReconPosition> remove)
        {
            AddReconPositions(add);
            RemoveReconPositions(remove);
        }

        private void AddReconPosition(ReconPosition reconPos)
        {
            if (ReconPositions.TryGetValue(reconPos, out var existing))
            {
                ReconPositions.Remove(existing);
                existing.AddRecon(reconPos);
                ReconPositions.Add(existing);
            }
            else
            {
                ReconPositions.Add(reconPos);
            }
        }

        private void RemoveReconPosition(ReconPosition reconPos)
        {
            if (ReconPositions.TryGetValue(reconPos, out var existing))
            {
                ReconPositions.Remove(existing);
                existing.RemoveRecon(reconPos);

                // Optionally remove if recon drops to 0
                if (existing.ReconLevel > 0)
                    ReconPositions.Add(existing);
            }
        }
        private void AddReconPositions(List<ReconPosition> reconPositions)
        {
            foreach (var reconPos in reconPositions)
            {
                AddReconPosition(reconPos);
            }
        }

        private void RemoveReconPositions(List<ReconPosition> reconPositions)
        {
            foreach (var reconPos in reconPositions)
            {
                RemoveReconPosition(reconPos);
            }
        }


        private void RequestDataFromSever()
        {
            // when the game starts, request data such as positions, units, building...
            // to be implemented
        }


    }
}
