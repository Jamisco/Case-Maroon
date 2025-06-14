using Assets.Scripts.Worldmap.Miscellaneous;
using UnityEditor;
using UnityEngine;

namespace Assets.Case_Maroon
{

    [CustomPropertyDrawer(typeof(Vector2Range))]
    public class Vector2RangeAttributeDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginChangeCheck();
            // Create a regular Vector2 field
            Vector2 val = EditorGUI.Vector2Field(position, label, property.vector2Value);
            // If the value changed
            if (EditorGUI.EndChangeCheck())
            {
                var rangeAttribute = (Vector2Range)attribute;
                // Clamp the X/Y values to be within the allowed range
                val.x = Mathf.Clamp(val.x, rangeAttribute.MinX, rangeAttribute.MaxX);
                val.y = Mathf.Clamp(val.y, rangeAttribute.MinY, rangeAttribute.MaxY);
                // Update the value of the property to the clamped value
                property.vector2Value = val;
            }
        }
    }
}
