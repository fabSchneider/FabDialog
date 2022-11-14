using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Fab.Dialog.Editor
{
    [CustomEditor(typeof(DialogGraphAsset))]
    public class DialogGraphAssetEditor : UnityEditor.Editor
    {
        [InitializeOnLoadMethod]
        public static void SetCustomIcon()
        {
            MonoImporter monoImporter = AssetImporter.GetAtPath("Packages/com.fab.dialog/Editor/DialogGraphAsset.cs") as MonoImporter;
            MonoScript monoScript = monoImporter.GetScript();
            Texture2D icon = AssetDatabase.LoadAssetAtPath<Texture2D>(DialogGraphUtility.DialogIconPath);

            EditorGUIUtility.SetIconForObject(monoScript, icon);
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            if (GUILayout.Button("Open Graph Editor"))
            {
                DialogEditorWindow.Open((DialogGraphAsset)target);
            }

            using (new EditorGUI.DisabledScope(true))
            {
                DrawPropertiesExcluding(serializedObject, "m_Script");
            }
            serializedObject.ApplyModifiedProperties();
        }
    }
}
