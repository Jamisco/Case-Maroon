
using UnityEngine;
using System.Collections.Generic;
using static CaseMaroon.Miscellaneous.GlobalData;
using static CaseMaroon.Backend.BackendResponses;
using System.Linq;
using Unity.VisualScripting;
using static CaseMaroon.Backend.BackendModels;

namespace CaseMaroon.GameSystem
{
    public class Player
    {
        public int Id { get; set; }

        public HashSet<ReconPosition> ReconPositions { get; private set; }

        public HashSet<Vector2Int> OwnedPositions { get; private set; }

        public Player(PlayerModel pr)
        {
            Id = pr.id;

            ReconPositions = new HashSet<ReconPosition>();
            OwnedPositions = new HashSet<Vector2Int>();

            if(pr.reconPositions.Count > 0)
            {
                ReconPositions.AddRange(pr.reconPositions);
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

        public void Update(PlayerModel pr)
        {
            // Update the player's data with the server data
            Id = pr.id;
            // Update recon positions
            ReconPositions.Clear();
            ReconPositions.UnionWith(pr.reconPositions);
        }

        public void SetOwnedPositions(List<OwnedPosition> positions)
        {
            List<Vector2Int> pos = positions.Where(pos => pos.playerId == Id).Select(pos => pos.gridPosition).ToList();

            OwnedPositions.Clear();
            OwnedPositions.UnionWith(pos);
        }

        public bool CanSeeUnit(Vector2Int gridPos)
        {
            // Check if the player can see a unit at the given grid position
            return ReconPositions.Any(rp => rp.gridPosition == gridPos && rp.ReconLevel > 2);
        }

    }
}
