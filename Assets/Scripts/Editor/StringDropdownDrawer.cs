#if UNITY_EDITOR
using CaseMaroon.GameSystem;
using CaseMaroon.Miscellaneous;
using NUnit.Framework;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace CaseMaroon
{
    [CustomPropertyDrawer(typeof(StringDropdownAttribute))]
    public class StringDropdownDrawer : PropertyDrawer
    {
        public List<string> stringIds = new List<string>();

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            StringDropdownAttribute.UpdateIds();

            string[] ids = StringDropdownAttribute.stringIds.ToArray();

            int currentIndex = Mathf.Max(0, System.Array.IndexOf(ids, property.stringValue));

            int selectedIndex = EditorGUI.Popup(position, label.text, currentIndex, ids);

            if (selectedIndex >= 0 && selectedIndex < ids.Length)
            {
                property.stringValue = ids[selectedIndex];
            }
        }
    }
#endif
}