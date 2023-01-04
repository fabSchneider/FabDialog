using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace Fab.Dialog.Editor
{
    public class StateGraphView : GraphView
    {
        private StateGraphEditorWindow editorWindow;
        private StateNodeViewFactory elementFactory;
        private StateGraphResolver resolver;

        public StateGraphView(
            StateGraphEditorWindow editorWindow,
            StateNodeViewFactory elementFactory,
            StateGraphResolver resolver) : base()
        {
            this.editorWindow = editorWindow;
            StyleSheet graphViewStylesheet = (StyleSheet)EditorGUIUtility.Load("Packages/com.fab.dialog/EditorResources/Styles/DialogEditorStyles.uss");
            StyleSheet nodeStyleSheet = (StyleSheet)EditorGUIUtility.Load("Packages/com.fab.dialog/EditorResources/Styles/DialogNodeStyles.uss");

            styleSheets.Add(graphViewStylesheet);
            styleSheets.Add(nodeStyleSheet);

            GridBackground background = new GridBackground();
            Insert(0, background);

            AddManipulators();

            this.elementFactory = elementFactory;
            this.resolver = resolver;

            resolver.onNodeResolved += OnNodeResolved;

            serializeGraphElements += OnCopy;
            unserializeAndPaste += OnPaste;

            graphViewChanged += OnGraphViewChanged;
        }

        public void OnNodeResolved(StateNode node)
        {
            StateNodeView view = nodes.OfType<StateNodeView>().First(n => n.StateNode == node);
            view.AddToClassList("node--resolved");
            view.schedule.Execute(() => view.RemoveFromClassList("node--resolved")).ExecuteLater(1000);
        }

        public override List<Port> GetCompatiblePorts(Port startPort, NodeAdapter nodeAdapter)
        {
            // allow loops for execute ports
            if (startPort.portType == null)
                return GetCompatiblePorts(startPort, true);
            else
                return GetCompatiblePorts(startPort, false);
        }

        protected List<Port> GetCompatiblePorts(Port startPort, bool allowLoop)
        {
            List<Port> compatiblePorts = new List<Port>();

            Node startNode = startPort.node;
            ports.ForEach(port =>
            {
                // don't allow connecting to itself
                if (startPort == port)
                    return;

                // don't allow other portType
                if (port.portType != typeof(object) && startPort.portType != port.portType)
                    return;

                // don't allow same direction
                if (startPort.direction == port.direction)
                    return;

                if (!allowLoop)
                {
                    // check if connecting start node with target node
                    // would create a loop

                    // simple case: port connecting to another port on the same node
                    if (startNode == port.node)
                        return;

                    if (DetectLoop(port, startNode))
                        return;
                }

                compatiblePorts.Add(port);
            });
            return compatiblePorts;
        }

        private bool DetectLoop(Port port, Node startNode)
        {
            VisualElement oppositeContainer = port.direction == Direction.Output ? port.node.inputContainer : port.node.outputContainer;
            // get all compatible ports on nodes opposite direction that are connected to other nodes
            foreach (Port opposite in oppositeContainer.Query<Port>().Build())
            {
                if (opposite.portType != port.portType)
                    continue;

                // trace the connection to the new node
                foreach (Edge e in opposite.connections)
                {
                    Port nextPort = opposite.direction == Direction.Input ? e.output : e.input;

                    if (nextPort.node == startNode)
                    {
                        // if the node of traced port is the start node,
                        // we have found a loop
                        return true;
                    }

                    // otherwise keep tracing until a loop was found
                    if (DetectLoop(nextPort, startNode))
                        return true;
                }
            }

            return false;
        }

        private void AddManipulators()
        {
            SetupZoom(
                ContentZoomer.DefaultMinScale,
                ContentZoomer.DefaultMaxScale,
                ContentZoomer.DefaultScaleStep,
                ContentZoomer.DefaultReferenceScale
                );
            
            CreateContextMenuItems();

            this.AddManipulator(new ContentDragger());
            this.AddManipulator(new SelectionDragger());
            this.AddManipulator(new RectangleSelector());
        }

        private void CreateContextMenuItems()
        {
            foreach (StateNodeDescriptor descriptor in StateNodeLibrary.Descriptors)
            {
                this.AddManipulator(CreateNodeContextMenu(descriptor));
            }
        }

        private IManipulator CreateNodeContextMenu(StateNodeDescriptor descriptor)
        {
            ContextualMenuManipulator ctxMenuManipulator = new ContextualMenuManipulator(
                menuEvent =>
                {
                    menuEvent.menu.AppendAction(descriptor.path,
                        actionEvent =>
                        {
                            Vector2 position = GetLocalMousePosition(actionEvent.eventInfo.localMousePosition);
                            CreateAndAddNode(descriptor, position);
                        });
                });

            return ctxMenuManipulator;
        }

        public StateNodeView CreateAndAddNode(StateNodeDescriptor descriptor, Vector2 position)
        {
            StateNode node = elementFactory.Create(descriptor);
            StateNodeView nodeView = new StateNodeView();
            nodeView.SetPosition(new Rect(position.x, position.y, 0f, 0f));
            AddElement(nodeView);
            nodeView.owner = this;
            nodeView.Initialize(node);
            return nodeView;
        }

        public StateNodeView CreateAndAddNode(NodeState state)
        {
            StateNodeDescriptor descriptor = StateNodeLibrary.GetDescriptor(new Guid(state.type));
            StateNode node = elementFactory.Create(descriptor);

            node.Deserialize(state);

            StateNodeView nodeView = new StateNodeView();
            nodeView.SetPosition(new Rect(state.xPos, state.yPos, 0f, 0f));
            AddElement(nodeView);
            nodeView.owner = this;
            nodeView.Initialize(node);
            nodeView.ApplyNodeState(state);
            return nodeView;
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

        private GraphViewChange OnGraphViewChanged(GraphViewChange changes)
        {
            // process all changes made to the graph view

            if (changes.edgesToCreate != null)
            {
                foreach (Edge edge in changes.edgesToCreate)
                {
                }
            }

            if (changes.elementsToRemove != null)
            {
                foreach (GraphElement element in changes.elementsToRemove)
                {
                    if (element is Edge edge)
                    {
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
            GraphState graphState = new GraphState();

            foreach (GraphElement element in elements)
            {
                if (element is Node node)
                {
                    NodeState state = EditorGraphIO.CreateNodeState(node);
                    graphState.nodes.Add(state);
                }
                else if (element is Edge edge)
                {
                    EdgeState state = EditorGraphIO.CreateEdgeState(edge);
                    graphState.edges.Add(state);
                }
            }

            return GraphIO.SerializeGraphState(graphState);
        }

        private void OnPaste(string operationName, string data)
        {
            if (operationName == "Paste")
            {
                GraphState copyData = GraphIO.DesrializeGraphState(data);

                // deselect all currently selected elements
                ClearSelection();

                List<GraphElement> copiedElements = EditorGraphIO.ApplyGraphStateCopy(copyData, this, elementFactory);

                foreach (GraphElement element in copiedElements)
                    AddToSelection(element);
            }
        }
    }
}
