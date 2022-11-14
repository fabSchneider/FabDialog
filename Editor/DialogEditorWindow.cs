using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;
using UnityEditor.UIElements;

namespace Fab.Dialog.Editor
{
    public class DialogEditorWindow : EditorWindow
    {
        private DialogGraphView graphView;
        private DialogGraphAsset graphAsset;
        public DialogGraphAsset GraphAsset => graphAsset;

        private bool graphLoaded = false;

        [UnityEditor.Callbacks.OnOpenAsset(1)]
        public static bool OnOpenAsset(int instanceID, int line)
        {
            if (Selection.activeObject is DialogGraphAsset asset)
            {
                Open(asset);
                return true;
            }
            return false;
        }

        public static void Open(DialogGraphAsset graphAsset)
        {
            // try to get an existing window for the graph asset
            foreach (DialogEditorWindow existing in Resources.FindObjectsOfTypeAll<DialogEditorWindow>())
            {
                if (existing.graphAsset == graphAsset)
                {
                    existing.Focus();
                    return;
                }
            }


            DialogEditorWindow window = CreateWindow<DialogEditorWindow>();
            string name = graphAsset.name + "Window";
            window.name = name;
            window.graphAsset = graphAsset;
            window.SaveWindowState(name);

            Texture2D icon = AssetDatabase.LoadAssetAtPath<Texture2D>(DialogGraphUtility.DialogIconPath);

            window.titleContent = new GUIContent(graphAsset.name, icon);

            if (window.graphLoaded == false)
            {
                DialogGraphUtility.Load(window.graphView, graphAsset);
                window.graphLoaded = true;
            }

            window.Show();
        }

        private void OnEnable()
        {
            graphLoaded = false;
        }
        private void OnDestroy()
        {
            ClearWindowState();
        }
        private void SaveWindowState(string key)
        {
            if (graphAsset == null)
                Debug.LogError("Graph asset is null.");

            string assetPath = AssetDatabase.GetAssetPath(graphAsset);
            if (!string.IsNullOrEmpty(assetPath))
            {
                EditorPrefs.SetString(key, AssetDatabase.AssetPathToGUID(assetPath));
            }
        }

        private void LoadFromSavedState()
        {
            Debug.Log("Loading Graph Window State");
            string guid = EditorPrefs.GetString(name);
            graphAsset = AssetDatabase.LoadAssetAtPath<DialogGraphAsset>(AssetDatabase.GUIDToAssetPath(guid));
        }

        private void ClearWindowState()
        {
            EditorPrefs.DeleteKey(name);
        }

        public void UpdateWindowName()
        {
            // clear the window state first
            // as it's key is this windows name
            ClearWindowState();
            name = graphAsset.name + "Window";
            titleContent = new GUIContent(graphAsset.name);
            EditorUtility.SetDirty(this);
            // re-save the state with the updated name
            SaveWindowState(name);
        }

        public void Update()
        {
            if (graphAsset)
            {
                if (graphAsset.name != titleContent.text)
                {
                    UpdateWindowName();
                }
                if (!graphLoaded)
                {
                    DialogGraphUtility.Load(graphView, graphAsset);
                    graphLoaded = true;
                }
            }
            else
            {
                LoadFromSavedState();
                if (graphAsset != null)
                {
                    DialogGraphUtility.Load(graphView, graphAsset);
                    graphLoaded = true;
                }
            }
        }


        public void CreateGUI()
        {
            Toolbar toolbar = CreateToolbar();
            rootVisualElement.Add(toolbar);

            graphView = new DialogGraphView(this);
            graphView.style.width = new Length(100f, LengthUnit.Percent);
            graphView.style.height = new Length(100f, LengthUnit.Percent);

            rootVisualElement.Add(graphView);
        }

        private Toolbar CreateToolbar()
        {
            Toolbar toolbar = new Toolbar();
            ToolbarButton saveButton = new ToolbarButton(() => DialogGraphUtility.Save(graphView, graphAsset))
            {
                text = "Save"
            };

            ToolbarButton saveAsButton = new ToolbarButton(() =>
                {
                    string path = EditorUtility.SaveFilePanel(
                        "Save Dialog Graph As...",
                        System.IO.Path.GetDirectoryName(AssetDatabase.GetAssetPath(graphAsset)),
                        "New" + graphAsset.name,
                        "asset");

                    if (!string.IsNullOrEmpty(path))
                    {
                        path = System.IO.Path.GetRelativePath(Application.dataPath, path);
                        path = System.IO.Path.Combine("Assets", path);
                        graphAsset = ScriptableObject.CreateInstance<DialogGraphAsset>();
                        AssetDatabase.CreateAsset(graphAsset, path);
                        DialogGraphUtility.Save(graphView, graphAsset);
                    }
                })
            {
                text = "Save As..."
            };

            toolbar.Add(saveButton);
            toolbar.Add(saveAsButton);
            return toolbar;
        }
    }
}
