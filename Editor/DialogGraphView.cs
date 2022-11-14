using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEditor;
using UnityEngine.UIElements;
using Fab.Dialog.Editor;
using UnityEngine;
using Fab.Dialog.Editor.Elements;
using System;

namespace Fab.Dialog
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

            OnGraphViewChanged();
        }

        [System.Serializable]
        public class GraphCopyData
        {
            public List<DialogNodeData> nodes;
        }

        private string OnCopy(IEnumerable<GraphElement> elements)
        {
            List<DialogNodeData> nodeData = new List<DialogNodeData>();

            Dictionary<string, string> copyIDByOriginal = new Dictionary<string, string>();
            foreach (GraphElement element in elements)
            {
                if (element is DialogNode node)
                {
                    DialogNodeData data = node.ToNodeData();
                    // create a new Guid for this node
                    string newID = DialogNode.CreateGuid();
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
                    if (!string.IsNullOrEmpty(choice.NodeID))
                    {
                        if (copyIDByOriginal.TryGetValue(choice.NodeID, out string copyID))
                            choice.NodeID = copyID;
                        else
                            choice.NodeID = string.Empty;
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

                Dictionary<string, DialogNode> nodesById = new Dictionary<string, DialogNode>();

                foreach (DialogNodeData nodeData in copyData.nodes)
                {
                    // add an offset to the node position so that copy 
                    //is not at the exact same position as the original

                    nodeData.Position += new Vector2(50, 50);

                    DialogNode node = CreateNode(nodeData);
                    nodesById[node.ID] = node;
                    AddToSelection(node);

                    AddElement(node);
                }

                // connect nodes
                DialogGraphUtility.ConnectNodes(this, nodesById);
            }
        }

        public override List<Port> GetCompatiblePorts(Port startPort, NodeAdapter nodeAdapter)
        {
            List<Port> compatiblePorts = new List<Port>();

            ports.ForEach(port =>
            {
                if (startPort == port)
                    return;

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

        public DialogNode CreateNode(DialogType type, Vector2 localPosition)
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

        private void OnGraphViewChanged()
        {
            graphViewChanged = (changes) =>
            {
                if (changes.edgesToCreate != null)
                {
                    foreach (Edge edge in changes.edgesToCreate)
                    {
                        DialogNode nextNode = (DialogNode)edge.input.node;
                        DialogChoiceData choiceData = (DialogChoiceData)edge.output.userData;

                        choiceData.NodeID = nextNode.ID;
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
                            choiceData.NodeID = string.Empty;
                        }
                    }
                }

                return changes;
            };
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
    }
}

