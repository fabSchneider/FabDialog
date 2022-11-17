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
        }

        [System.Serializable]
        public class GraphCopyData
        {
            public List<DialogNodeData> nodes;
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

            this.AddManipulator(CreateNodeContextMenu("Add Single Choice Node", DialogType.SingleChoice));
            this.AddManipulator(CreateNodeContextMenu("Add Multi Choice Node", DialogType.MultiChoice));

            this.AddManipulator(CreateGroupContextMenu());

            this.AddManipulator(new ContentDragger());
            this.AddManipulator(new SelectionDragger());
            this.AddManipulator(new RectangleSelector());
        }

        private IManipulator CreateNodeContextMenu(string actionTitle, DialogType type)
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

        public DialogChoiceNode CreateNode(DialogType type, Vector2 localPosition)
        {
            Type nodeType = Type.GetType($"Fab.Dialog.Editor.Elements.{type}Node");
            DialogChoiceNode node = (DialogChoiceNode)Activator.CreateInstance(nodeType);
            node.Initialize(this, localPosition);

            return node;
        }

        public DialogChoiceNode CreateNode(DialogNodeData nodeData)
        {
            Type nodeType = Type.GetType($"Fab.Dialog.Editor.Elements.{nodeData.DialogType}Node");
            DialogChoiceNode node = (DialogChoiceNode)Activator.CreateInstance(nodeType);
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
                    // update choice data for each edge

                    DialogChoiceNode nextNode = (DialogChoiceNode)edge.input.node;
                    DialogChoiceData choiceData = (DialogChoiceData)edge.output.userData;

                    WeightedPath transition = new WeightedPath()
                    {
                        TargetNodeID = nextNode.ID,
                        Weight = 1f
                    };

                    choiceData.Paths.Add(transition);

                    if (edge is WeightedEdge weightedEdge)
                    {
                        weightedEdge.WeightedTransition = transition;
                    }                   
                    
                    //// update the weights on each weighted transition
                    //// TODO: This should be done once after all transitions have been updated / removed
                    //for (int i = 0; i < choiceData.Transitions.Count; i++)
                    //{
                    //    choiceData.Transitions[i].Weight = 1f / choiceData.Transitions.Count;
                    //}

                    // create/update weight label on the edge

                    //Label weightLabel = edge.Q<Label>(name: "weight-label");
                    //if(weightLabel == null)
                    //{
                    //    weightLabel = new Label();
                    //    weightLabel.name = "weight-label";
                    //    weightLabel.style.marginBottom = StyleKeyword.Auto;
                    //    weightLabel.style.marginTop = StyleKeyword.Auto;
                    //    weightLabel.style.marginLeft = StyleKeyword.Auto;
                    //    weightLabel.style.marginRight = StyleKeyword.Auto;
                    //    edge.edgeControl.Add(weightLabel);
                    //}

                    //weightLabel.text = 0.42f.ToString();
                }
            }

            if (changes.elementsToRemove != null)
            {
                Type edgeType = typeof(Edge);
                foreach (GraphElement element in changes.elementsToRemove)
                {
                    if (element is Edge edge)
                    {
                        DialogChoiceData choiceData = (DialogChoiceData)edge.output.userData;

                        if(edge is WeightedEdge weightedEdge && weightedEdge.WeightedTransition != null)
                        {
                            choiceData.Paths.Remove(weightedEdge.WeightedTransition);
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

            Dictionary<string, string> copyIDByOriginal = new Dictionary<string, string>();
            foreach (GraphElement element in elements)
            {
                if (element is DialogChoiceNode node)
                {
                    DialogNodeData data = node.ToNodeData();
                    // create a new Guid for this node
                    string newID = DialogChoiceNode.CreateGuid();
                    // keep track of old and new guid
                    copyIDByOriginal.Add(data.ID, newID);
                    data.ID = newID;
                    nodeData.Add(data);
                }
            }

            // replace all ids in choice data with new id's or remove if they
            // are referencing a node that was not copied
            foreach (DialogNodeData data in nodeData)
            {
                foreach (DialogChoiceData choice in data.Choices)
                {
                    for (int i = 0; i < choice.Paths.Count; i++)
                    {
                        WeightedPath transition = choice.Paths[i];

                        if (!string.IsNullOrEmpty(transition.TargetNodeID))
                        {
                            if (copyIDByOriginal.TryGetValue(transition.TargetNodeID, out string copyID))
                                transition.TargetNodeID = copyID;
                            else
                                transition = new WeightedPath();
                        }

                        choice.Paths[i] = transition;
                    }
                }
            }

            GraphCopyData copyData = new GraphCopyData()
            {
                nodes = nodeData
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

                Dictionary<string, DialogChoiceNode> nodesById = new Dictionary<string, DialogChoiceNode>();

                foreach (DialogNodeData nodeData in copyData.nodes)
                {
                    // add an offset to the node position so that copy 
                    //is not at the exact same position as the original

                    nodeData.Position += new Vector2(50, 50);

                    DialogChoiceNode node = CreateNode(nodeData);
                    nodesById[node.ID] = node;
                    AddToSelection(node);

                    AddElement(node);
                }

                // connect nodes
                DialogGraphUtility.ConnectNodes(this, nodesById);
            }
        }
    }
}

