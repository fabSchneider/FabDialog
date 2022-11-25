using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;


namespace Fab.Dialog.Editor
{
    [CustomEditor(typeof(StateGraphAsset))]
    public class StateGraphAssetEditor : UnityEditor.Editor
    {
        public static readonly string StateGraphAssetIconPath = "Packages/com.fab.dialog/EditorResources/Icons/Dialog.png";

        [InitializeOnLoadMethod]
        public static void SetCustomIcon()
        {
            MonoImporter monoImporter = AssetImporter.GetAtPath("Packages/com.fab.dialog/Runtime/State/StateGraphAsset.cs") as MonoImporter;
            MonoScript monoScript = monoImporter.GetScript();
            Texture2D icon = AssetDatabase.LoadAssetAtPath<Texture2D>(StateGraphAssetIconPath);
            EditorGUIUtility.SetIconForObject(monoScript, icon);
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            if (GUILayout.Button("Open Graph Editor"))
            {
                StateGraphEditorWindow.Open((StateGraphAsset)target);
            }

            using (new EditorGUI.DisabledScope(true))
            {
                SerializedProperty prop = serializedObject.FindProperty("graphData");
                EditorGUILayout.TextArea(prop.stringValue, GUILayout.ExpandWidth(true));
            }
            serializedObject.ApplyModifiedProperties();
        }
    }
}
