using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEditor;
using UnityEngine.UIElements;
using Fab.Dialog.Editor;
using UnityEngine;
using Fab.Dialog.Editor.Elements;
using System;

namespace Fab.Dialog.Editor
{
    public class DialogGraphView : GraphView
    {
        private DialogEditorWindow editorWindow;
        private DialogSearchWindow searchWindow;

        private VisualElement updateDebugger;

        public DialogGraphView(DialogEditorWindow editorWindow) : base()
        {
            this.editorWindow = editorWindow;
            StyleSheet graphViewStylesheet = (StyleSheet)EditorGUIUtility.Load("Packages/com.fab.dialog/EditorResources/Styles/DialogEditorStyles.uss");
            StyleSheet nodeStyleSheet = (StyleSheet)EditorGUIUtility.Load("Packages/com.fab.dialog/EditorResources/Styles/DialogNodeStyles.uss");

            styleSheets.Add(graphViewStylesheet);
            styleSheets.Add(nodeStyleSheet);

            GridBackground background = new GridBackground();
            Insert(0, background);

            searchWindow = ScriptableObject.CreateInstance<DialogSearchWindow>();
            searchWindow.Initialize(this);

            AddManipulators();

            nodeCreationRequest = context => SearchWindow.Open(
                new SearchWindowContext(
                    viewport.WorldToLocal(context.screenMousePosition)), searchWindow);

            serializeGraphElements += OnCopy;
            unserializeAndPaste += OnPaste;


            graphViewChanged += OnGraphViewChanged;

            schedule.Execute(RefreshData).Every(100);

            SetupUpdateDebugger();
        }

        private void SetupUpdateDebugger()
        {
            updateDebugger = new VisualElement();
            updateDebugger.style.backgroundColor = Color.gray;
            updateDebugger.style.width = 200;
            updateDebugger.style.position = Position.Absolute;
            updateDebugger.style.right = 20;
            updateDebugger.style.bottom = 20;

            Label debuggerText = new Label();

            debuggerText.schedule.Execute(() =>
            {
                string updateInfo = "Update Debugger \n\n";

                foreach (Edge edge in refreshEdges)
                {
                    updateInfo += "Edge \n";
                }

                foreach (DialogNode node in refreshNodes)
                {
                    updateInfo += $"Node ({node.NodeName})\n";
                }

                debuggerText.text = updateInfo;
            }).Every(50);
            updateDebugger.Add(debuggerText);

            Add(updateDebugger);
        }

        private HashSet<Edge> refreshEdges = new HashSet<Edge>();
        private HashSet<DialogNode> refreshNodes = new HashSet<DialogNode>();

        public void FlagRefresh(Edge edge)
        {
            refreshEdges.Add(edge);
        }

        public void FlagRefresh(DialogNode node)
        {
            refreshNodes.Add(node);
        }

        public void RefreshData()
        {
            foreach (Edge e in refreshEdges)
            {
                if (e.parent != null)
                {
                    if (e.input.node is DialogNode node)
                    {
                        e.input.userData = e.output.userData;
                        refreshNodes.Add(node);
                    }
                }
            }

            refreshEdges.Clear();

            foreach (DialogNode node in refreshNodes)
            {
                node.UpdateInputs();
            }

            refreshNodes.Clear();
        }

        [System.Serializable]
        public class GraphCopyData
        {
            public List<DialogNodeData> nodes;
            public List<DialogEdgeData> edges;
        }

        public override List<Port> GetCompatiblePorts(Port startPort, NodeAdapter nodeAdapter)
        {
            List<Port> compatiblePorts = new List<Port>();

            ports.ForEach(port =>
            {
                if (startPort == port)
                    return;

                // NOTE: Allow connecting to node's own input

                //if (startPort.node == port.node)
                //    return;

                if (startPort.portType != port.portType)
                    return;

                if (startPort.direction == port.direction)
                    return;

                compatiblePorts.Add(port);
            });
            return compatiblePorts;
        }

        private void AddManipulators()
        {
            SetupZoom(
                ContentZoomer.DefaultMinScale,
                ContentZoomer.DefaultMaxScale,
                ContentZoomer.DefaultScaleStep,
                ContentZoomer.DefaultReferenceScale
                );

            this.AddManipulator(CreateNodeContextMenu("Add Single Choice Node", DialogNodeType.SingleChoice));
            this.AddManipulator(CreateNodeContextMenu("Add Multi Choice Node", DialogNodeType.MultiChoice));

            this.AddManipulator(CreateGroupContextMenu());

            this.AddManipulator(new ContentDragger());
            this.AddManipulator(new SelectionDragger());
            this.AddManipulator(new RectangleSelector());
        }

        private IManipulator CreateNodeContextMenu(string actionTitle, DialogNodeType type)
        {
            ContextualMenuManipulator ctxMenuManipulator = new ContextualMenuManipulator(
                menuEvent => menuEvent.menu.AppendAction(actionTitle, actionEvent =>
                    AddElement(CreateNode(type, GetLocalMousePosition(actionEvent.eventInfo.localMousePosition)))
                    )
                );

            return ctxMenuManipulator;
        }

        private IManipulator CreateGroupContextMenu()
        {
            ContextualMenuManipulator ctxMenuManipulator = new ContextualMenuManipulator(
                menuEvent => menuEvent.menu.AppendAction("Add Group", actionEvent =>
                    AddElement(CreateGroup("Dialog Group", GetLocalMousePosition(actionEvent.eventInfo.localMousePosition)))
                    )
                );

            return ctxMenuManipulator;
        }

        public void ValidateViewTransform()
        {
            if (contentViewContainer != null)
            {
                Vector3 vector = viewTransform.scale;
                vector.x = Mathf.Clamp(vector.x, minScale, maxScale);
                vector.y = Mathf.Clamp(vector.y, minScale, maxScale);
                viewTransform.scale = vector;
            }
        }

        public DialogNode CreateNode(DialogNodeType type, Vector2 localPosition)
        {
            Type nodeType = Type.GetType($"Fab.Dialog.Editor.Elements.{type}Node");
            DialogNode node = (DialogNode)Activator.CreateInstance(nodeType);
            node.Initialize(this, localPosition);

            return node;
        }

        public DialogNode CreateNode(DialogNodeData nodeData)
        {
            Type nodeType = Type.GetType($"Fab.Dialog.Editor.Elements.{nodeData.DialogType}Node");
            DialogNode node = (DialogNode)Activator.CreateInstance(nodeType);

            node.Initialize(this, nodeData);

            return node;
        }

        public Group CreateGroup(string title, Vector2 localPosition)
        {
            Group group = new Group()
            {
                title = title
            };

            group.SetPosition(new Rect(localPosition, Vector2.zero));

            return group;
        }

        public Group CreateGroup(DialogGroupData groupData)
        {
            Group group = new Group()
            {
                title = groupData.Name
            };

            group.SetPosition(new Rect(groupData.Position, Vector2.zero));

            return group;
        }

        private GraphViewChange OnGraphViewChanged(GraphViewChange changes)
        {
            // process all changes made to the graph view

            // edges
            if (changes.edgesToCreate != null)
            {
                foreach (Edge edge in changes.edgesToCreate)
                {
                    FlagRefresh(edge);
                }
            }

            if (changes.elementsToRemove != null)
            {
                Type edgeType = typeof(Edge);
                foreach (GraphElement element in changes.elementsToRemove)
                {
                    if (element is Edge edge)
                    {
                        if (edge.input.portType == typeof(string))
                        {
                            edge.input.userData = null;
                            if (edge.input.node is DialogNode node)
                                FlagRefresh(node);
                        }
                    }
                }
            }

            return changes;
        }

        public Vector2 GetLocalMousePosition(Vector2 mousePosition, bool isSearchWindow = false)
        {
            if (isSearchWindow)
            {
                mousePosition -= editorWindow.position.position;
            }

            Vector2 localMousePosition = contentViewContainer.WorldToLocal(mousePosition);
            return localMousePosition;
        }

        private string OnCopy(IEnumerable<GraphElement> elements)
        {
            List<DialogNodeData> nodeData = new List<DialogNodeData>();
            List<DialogEdgeData> edgeData = new List<DialogEdgeData>();

            foreach (GraphElement element in elements)
            {
                if (element is DialogNode node)
                {
                    DialogNodeData data = node.Serialize();
                    nodeData.Add(data);
                }
                else if (element is Edge e)
                {
                    edgeData.Add(new DialogEdgeData()
                    {
                        Input = e.input.viewDataKey,
                        Output = e.output.viewDataKey,
                        Weight = (e is WeightedEdge we) ? we.Weight : 1f
                    });
                }
            }

            GraphCopyData copyData = new GraphCopyData()
            {
                nodes = nodeData,
                edges = edgeData
            };

            return JsonUtility.ToJson(copyData);
        }

        private void OnPaste(string operationName, string data)
        {
            if (operationName == "Paste")
            {
                GraphCopyData copyData = JsonUtility.FromJson<GraphCopyData>(data);

                if (copyData.nodes.Count == 0)
                    return;

                // deselect all currently selected elements
                ClearSelection();

                Dictionary<string, string> copyByOriginalPorts = new Dictionary<string, string>();

                foreach (DialogNodeData nodeData in copyData.nodes)
                {
                    // add an offset to the node position so that copy 
                    // is not at the exact same position as the original
                    nodeData.Position += new Vector2(50, 50);

                    nodeData.Id = Guid.NewGuid().ToString();

                    for (int i = 0; i < nodeData.Inputs.Count; i++)
                    {
                        PortData inPort = nodeData.Inputs[i];
                        string newId = Guid.NewGuid().ToString();

                        copyByOriginalPorts.Add(inPort.Id, newId);
                        inPort.Id = newId;
                        nodeData.Inputs[i] = inPort;
                    }

                    for (int i = 0; i < nodeData.Outputs.Count; i++)
                    {
                        PortData outPort = nodeData.Outputs[i];
                        string newId = Guid.NewGuid().ToString();

                        copyByOriginalPorts.Add(outPort.Id, newId);
                        outPort.Id = newId;
                        nodeData.Outputs[i] = outPort;
                    }

                    DialogNode node = CreateNode(nodeData);
                    // update the id of the node data with the id
                    // of the newly created node so that we can
                    // use it when connecting the nodes;
                    nodeData.Id = node.ID;
                    AddToSelection(node);

                    AddElement(node);
                }

                // remap edges
                for (int i = copyData.edges.Count - 1 ; i >= 0; i--)
                {
                    DialogEdgeData edge = copyData.edges[i];

                    // remove edges that connect with nodes outside of the copy selection
                    if(!copyByOriginalPorts.TryGetValue(edge.Input, out string input) ||
                       !copyByOriginalPorts.TryGetValue(edge.Output, out string output))
                    {
                        copyData.edges.RemoveAt(i);
                        continue; 
                    }

                    edge.Input = input;
                    edge.Output = output;
                    copyData.edges[i] = edge;
                }

                // connect nodes
                DialogGraphUtility.ConnectNodes(this, copyData.edges);
            }
        }
    }
}

