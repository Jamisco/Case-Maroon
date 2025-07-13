using CaseMaroon.Units;
using System;
using UnityEngine;
using static CaseMaroon.Backend.GlobalModel;

namespace CaseMaroon.Backend
{
    public static class BackendHelper
    {
        public static string ToJson(this Vector2Int v)
        {
            return JsonUtility.ToJson(v);
        }
        public static string ToQuery(this Vector2Int v)
        {
            return $"x={v.x}&y={v.y}";
        }

        public static Vector2Int ToVector2Int(this string json)
        {
            Vector2Int data;

            try
            {
                Vector2IntWrap wrap = JsonUtility.FromJson<Vector2IntWrap>(json);
                data = new Vector2Int(wrap.x, wrap.y);
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to parse JSON to Vector2Int: {e.Message}");
                data = Vector2Int.zero; // Default value in case of error
            }

            return data;
        }

        public static string ToJson(this Unit unit)
        {
            return JsonUtility.ToJson(new UnitDataWrap(unit));
        }
    }
}
