using UnityEditor;
using UnityEngine;
using System;
using System.Reflection;
using System.Linq;
using CaseMaroon.Miscellaneous;
using System.Data.Common;

namespace CaseMaroon
{
    [CustomEditor(typeof(TestInvoker))]
    public class TestInvokerEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            TestInvoker invoker = (TestInvoker)target;

            if (invoker.targetScript != null)
            {
                MonoBehaviour target = invoker.targetScript;
                Type targetType = target.GetType();

                // Fetch public parameterless methods that start with "Test_" (case-insensitive)
                MethodInfo[] methods = targetType
                    .GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly)
                    .Where(m =>
                        m.GetParameters().Length == 0 &&
                        m.Name.StartsWith("Test_", StringComparison.OrdinalIgnoreCase)
                    )
                    .ToArray();

                if(methods.Count() == 0)
                {
                    Debug.LogWarning("No Methods Found");
                    return;
                }

                string[] methodNames = methods.Select(m => m.Name).ToArray();
                int selectedIndex = Mathf.Max(0, Array.IndexOf(methodNames, invoker.methodName));

                int newIndex = EditorGUILayout.Popup("Select Method", selectedIndex, methodNames);
                invoker.methodName = methodNames[newIndex];

                if (GUILayout.Button("Invoke Choosen Method"))
                {
                    MethodInfo method = methods[newIndex];
                    method.Invoke(target, null);
                }
            }
            else
            {
                EditorGUILayout.HelpBox("Assign a target MonoBehaviour to begin.", MessageType.Info);
            }
        }
    }
}