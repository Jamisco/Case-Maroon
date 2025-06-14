using CaseMaroon.Units;
using System;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;


namespace CaseMaroon.WorldMap
{
    public class BiomeVisualizer : MonoBehaviour
    {
        private enum VisualType { MovementCost, Biome }

        [SerializeField]
        private VisualType visualType = VisualType.Biome;

        [SerializeField]
        private MovementType movementType = MovementType.Feet;

        public BiomeConfig biomeConfig;
        public GameObject cellPrefab; // assign an empty Plane prefab scaled to your desired size
        public Material LandMat;
        public Material defaultMat;

        public int gridSize = 10;

        [Range(.5f, 5)]
        public float cellSpacing = .5f;

        public BiomeVisualizer[,] cells;
        // biome visualizer
        public void GenerateGrid()
        {
            ClearGrid();
            cells = new BiomeVisualizer[gridSize, gridSize];

            for (int x = 0; x < gridSize; x++)
            {
                for (int y = 0; y < gridSize; y++)
                {
                    Vector3 pos = new Vector3(x * cellSpacing, y * cellSpacing, 0);
                    GameObject obj = Instantiate(cellPrefab, pos, Quaternion.identity, transform);
                    obj.name = $"Cell_{x}_{y}";
                    obj.transform.localPosition = pos;
                    cells[x, y] = obj.GetComponent<BiomeVisualizer>();

                    Renderer renderer;
                    SpriteRenderer spriteRen = obj.GetComponent<SpriteRenderer>();

                    if (visualType == VisualType.Biome)
                    {
                        spriteRen.material = LandMat;
                        MaterialPropertyBlock block = new MaterialPropertyBlock();

                        float temperature = (float)x / (gridSize - 1);
                        float rain = (float)y / (gridSize - 1);

                        block.SetFloat("_Temperature", temperature);
                        block.SetFloat("_Rain", rain);
                        renderer = obj.GetComponent<Renderer>();
                        renderer.SetPropertyBlock(block);
                    }
                    else
                    {
                        spriteRen.material = defaultMat;

                        float temp = (float)Math.Round((float)x / (gridSize - 1), 1);
                        float rain = (float)Math.Round((float)y / (gridSize - 1), 1);

                        BiomeData rule = biomeConfig.GetMatchingRule(temp, rain);

                        int movementCost;

                        if (rule != null)
                        {
                            movementCost = rule.GetMovementCost(movementType);
                        }
                        else
                        {
                            movementCost = 0; // Default value if no rule is found
                            Debug.LogWarning($"No matching biome rule found for temperature: {temp}, rain: {rain}");
                        }

                        // Green = easy movement (0), Yellow = mid (0.5), Red = hard (1)
                        Color lerpedColor;
                        if (movementCost <= 0.5f)
                        {
                            // Lerp from Green to Yellow
                            lerpedColor = Color.Lerp(Color.green, Color.yellow, movementCost / 0.5f);
                        }
                        else
                        {
                            // Lerp from Yellow to Red
                            lerpedColor = Color.Lerp(Color.yellow, Color.red, (movementCost - 0.5f) / 0.5f);
                        }

                        spriteRen.color = lerpedColor;

                    }

                    CreateTextLabel($"({x},{y})", obj.transform, Vector3.back, 32, 0.1f, Color.black);
                }
            }

            CreateGridLabel("Temperature", new Vector3((gridSize - 1) * cellSpacing / 2f, -cellSpacing * 1.5f, 0), 0, Color.red);
            CreateGridLabel("Rain", new Vector3(-cellSpacing * 1.5f, (gridSize - 1) * cellSpacing / 2f, 0), 90, Color.blue);
        }
        private void CreateTextLabel(string text, Transform parent, Vector3 localPos, int fontSize, float charSize, Color color)
        {
            GameObject textObj = new GameObject("Label");
            textObj.transform.SetParent(parent);
            textObj.transform.localPosition = localPos;
            textObj.transform.localRotation = Quaternion.identity;

            TextMesh textMesh = textObj.AddComponent<TextMesh>();
            textMesh.text = text;
            textMesh.fontSize = fontSize;
            textMesh.characterSize = charSize;
            textMesh.anchor = TextAnchor.MiddleCenter;
            textMesh.alignment = TextAlignment.Center;
            textMesh.color = color;
        }

        private void CreateGridLabel(string text, Vector3 localPos, float zRotation, Color color)
        {
            GameObject label = new GameObject($"{text}Label");
            label.transform.SetParent(transform);
            label.transform.localPosition = localPos;
            label.transform.localRotation = Quaternion.Euler(0, 0, zRotation);

            TextMesh textMesh = label.AddComponent<TextMesh>();
            textMesh.text = text;
            textMesh.fontSize = 48;
            textMesh.characterSize = 0.7f;
            textMesh.anchor = TextAnchor.MiddleCenter;
            textMesh.alignment = TextAlignment.Center;
            textMesh.color = color;
        }


        public void ClearGrid()
        {
            // loop through children in reverse

            for(int i = transform.childCount - 1; i >= 0; i--)
            {
                DestroyImmediate(transform.GetChild(i).gameObject);
            }
        }
    }


#if UNITY_EDITOR

    [CustomEditor(typeof(BiomeVisualizer))]
    public class MovementCostGridEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            BiomeVisualizer exampleScript = (BiomeVisualizer)target;

            if (GUILayout.Button("Generate Grid"))
            {
                exampleScript.GenerateGrid();
            }

            if (GUILayout.Button("Clear Grid"))
            {
                exampleScript.ClearGrid();
            }

        }
    }

#endif

}


