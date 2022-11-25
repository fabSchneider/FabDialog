using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Fab.Dialog.Editor
{
    public class StateGraphEditorWindow : EditorWindow
    {
        private StateGraphView graphView;
        private StateGraphAsset graphAsset;
        private StateGraphResolver graphResolver;
        private bool runResolver;

        private static StateNodeViewFactory elementFactory = new StateNodeViewFactory();
        public StateGraphAsset GraphAsset => graphAsset;

        private bool graphLoaded = false;

        [UnityEditor.Callbacks.OnOpenAsset(2)]
        public static bool OnOpenAsset(int instanceID, int line)
        {
            if (Selection.activeObject is StateGraphAsset asset)
            {
                Open(asset);
                return true;
            }
            return false;
        }

        public static void Open(StateGraphAsset graphAsset)
        {
            // try to get an existing window for the graph asset
            foreach (StateGraphEditorWindow existing in Resources.FindObjectsOfTypeAll<StateGraphEditorWindow>())
            {
                if (existing.graphAsset == graphAsset)
                {
                    existing.Focus();
                    return;
                }
            }


            StateGraphEditorWindow window = CreateWindow<StateGraphEditorWindow>();
            string name = graphAsset.name + "Window";
            window.name = name;
            window.graphAsset = graphAsset;
            window.SaveWindowState(name);

            Texture2D icon = AssetDatabase.LoadAssetAtPath<Texture2D>(StateGraphAssetEditor.StateGraphAssetIconPath);

            window.titleContent = new GUIContent(graphAsset.name, icon);

            if (window.graphLoaded == false)
            {
                EditorGraphIO.LoadGraph(graphAsset, window.graphView, elementFactory);
                window.graphLoaded = true;
            }

            window.Show();
        }

        private void OnEnable()
        {
            graphLoaded = false;
            graphResolver = new StateGraphResolver();
            runResolver = true;
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
            graphAsset = AssetDatabase.LoadAssetAtPath<StateGraphAsset>(AssetDatabase.GUIDToAssetPath(guid));
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
                    EditorGraphIO.LoadGraph(graphAsset, graphView, elementFactory);
                    graphLoaded = true;
                }
            }
            else
            {
                LoadFromSavedState();
                if (graphAsset != null)
                {
                    EditorGraphIO.LoadGraph(graphAsset, graphView, elementFactory);
                    graphLoaded = true;
                }
            }

            if (runResolver)
            {
                GraphState state = EditorGraphIO.CreateGraphState(graphView);

                List<StateNode> stateNodes = new List<StateNode>();
                List<StateNode> dirtyNodes = new List<StateNode>();
                graphView.Query<StateNodeView>().ForEach(nodeView =>
                {
                    stateNodes.Add(nodeView.StateNode);
                    if (nodeView.StateNode.NeedsResolve)
                        dirtyNodes.Add(nodeView.StateNode);

                });
                StateGraph stateGraph = new StateGraph(state, stateNodes);
                graphResolver.ResolveGraph(stateGraph, dirtyNodes);

                foreach (StateNode node in dirtyNodes)
                {
                    node.NeedsResolve = false;
                }
            }             
        }


        public void CreateGUI()
        {
            Toolbar toolbar = CreateToolbar();
            rootVisualElement.Add(toolbar);

            graphView = new StateGraphView(this, elementFactory, graphResolver);
            graphView.style.width = new Length(100f, LengthUnit.Percent);
            graphView.style.height = new Length(100f, LengthUnit.Percent);
            rootVisualElement.Add(graphView);
        }

        private Toolbar CreateToolbar()
        {
            Toolbar toolbar = new Toolbar();
            ToolbarButton saveButton = new ToolbarButton(() => EditorGraphIO.SaveGraph(graphAsset, graphView))
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
                    graphAsset = ScriptableObject.CreateInstance<StateGraphAsset>();
                    AssetDatabase.CreateAsset(graphAsset, path);
                    EditorGraphIO.SaveGraph(graphAsset, graphView);
                }
            })
            {
                text = "Save As..."
            };

            VisualElement spacer = new VisualElement();
            spacer.style.flexGrow = 1f;

            ToolbarButton resolveButton = new ToolbarButton(OnResolverButtonPressed);
            resolveButton.name = "resolve-button";
            resolveButton.text = runResolver ? "Pause Resolver" : "Resume Resolver";

            toolbar.Add(saveButton);
            toolbar.Add(saveAsButton);
            toolbar.Add(spacer);
            toolbar.Add(resolveButton);
            return toolbar;
        }

        private void OnResolverButtonPressed()
        {
            ToolbarButton resolveButton = rootVisualElement.Q<ToolbarButton>(name: "resolve-button");
            runResolver = !runResolver;
            resolveButton.text = runResolver ? "Pause Resolver" : "Resume Resolver";
        }
    }
}