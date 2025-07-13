using UnityEngine;

namespace CaseMaroon.Units
{
    public class BiomeVisualizer : MonoBehaviour
    {
        public MovementType moveType;

        public int resolution = 20;
        public float cellSize = 0.5f; // spacing in world units

        private float[,] infantryCost;
        private float[,] trackedCost;

        void OnEnable()
        {
            // Initialize arrays if null
            infantryCost ??= new float[resolution, resolution];
            trackedCost ??= new float[resolution, resolution];
        }

        private float[,] GetCurrentArray()
        {
            return moveType switch
            {
                MovementType.Feet => infantryCost,
                MovementType.Tracked => trackedCost,

                _ => infantryCost
            };
        }

        void OnValidate()
        {
            var array = GetCurrentArray();
            Vector3 origin = transform.position;

            for (int x = 0; x < resolution; x++)    
            {
                for (int y = 0; y < resolution; y++)
                {
                    float val = array[x, y]; // 0-1 movement multiplier
                    Gizmos.color = Color.Lerp(Color.green, Color.red, val);
                    Vector3 pos = origin + new Vector3(x * cellSize, y * cellSize, 0);
                    Gizmos.DrawCube(pos, new Vector3(cellSize * 0.9f, cellSize * 0.9f, 0.01f)); // flat square
                }
            }
        }
    }
}