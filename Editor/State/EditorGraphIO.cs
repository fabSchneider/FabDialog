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
    public static class EditorGraphIO
    {
        public static string CreateGuid()
        {
            return Guid.NewGuid().ToString();
        }

        public static List<NodeState> GetNodeState(GraphView graphView)
        {
            List<NodeState> nodes = new List<NodeState>();
            foreach (Node node in graphView.nodes)
                nodes.Add(CreateNodeState(node));

            return nodes;
        }

        public static NodeState CreateNodeState(Node nodeView)
        {
            Rect position = nodeView.GetPosition();

            NodeState state = new NodeState()
            {
                guid = nodeView.viewDataKey,
                title = nodeView.title,
                xPos = position.x,
                yPos = position.y,
                inputPorts = GetPortData(nodeView.inputContainer),
                outputPorts = GetPortData(nodeView.outputContainer)
            };

            if (nodeView is StateNodeView stateNodeView)
            {
                state.type = StateNodeLibrary.GetDescriptor(stateNodeView.StateNode.GetType()).identifier.ToString();
                stateNodeView.StateNode.Serialize(state);
            }
            return state;
        }
        private static string[] GetPortData(VisualElement container)
        {
            return container.Query<Port>()
                .ForEach(p => p.viewDataKey)
                .ToArray();
        }

        public static List<Node> ApplyNodeStateCopy(StateGraphView graphView, List<NodeState> nodeStates)
        {
            List<Node> createdNodes = new List<Node>();
            foreach (NodeState state in nodeStates)
            {
                Node node = graphView.CreateAndAddNode(state);
                createdNodes.Add(node);
            }
            return createdNodes;
        }

        public static void ApplyNodeStates(StateGraphView graphView, List<NodeState> nodeStates)
        {
            List<Node> matchedNodes = new List<Node>();
            foreach (NodeState state in nodeStates)
            {
                // check if node already exists
                StateNodeView node = (StateNodeView)graphView.GetNodeByGuid(state.guid);
                if (node == null)
                {
                    node = graphView.CreateAndAddNode(state);
                }

                matchedNodes.Add(node);
                node.ApplyNodeState(state);
            }

            // remove all unmatched nodes
            List<Node> removeNodes = graphView.nodes.Where(n => !matchedNodes.Contains(n)).ToList();
            foreach (Node node in removeNodes)
                graphView.RemoveElement(node);
        }

        public static void ApplyEdgeStates(GraphView graphView, List<EdgeState> edgeStates)
        {
            List<Edge> matchedEdges = new List<Edge>();
            foreach (EdgeState state in edgeStates)
            {

                // check if edge already exists
                Edge edge = graphView.GetEdgeByGuid(state.guid);
                if (edge == null)
                {
                    edge = new Edge();
                    graphView.AddElement(edge);
                }

                matchedEdges.Add(edge);

                // this will (re)connect the edge
                ApplyEdgeState(state, edge, graphView);
            }

            // remove all unmatched edges
            List<Edge> removeEdges = graphView.edges.Where(e => !matchedEdges.Contains(e)).ToList();
            foreach (Edge edge in removeEdges)
            {
                edge.input.Disconnect(edge);
                edge.output.Disconnect(edge);
                graphView.RemoveElement(edge);
            }
        }

        public static List<Edge> ApplyEdgeStatesCopy(GraphView graphView, List<EdgeState> edgeStates)
        {
            List<Edge> createdEdges = new List<Edge>();

            foreach (EdgeState state in edgeStates)
            {
                Edge edge = new Edge();
                graphView.AddElement(edge);

                ApplyEdgeState(state, edge, graphView);

                createdEdges.Add(edge);
            }

            return createdEdges;
        }

        public static EdgeState CreateEdgeState(Edge edge)
        {
            StateNodeDescriptor descriptor = StateNodeLibrary.GetDescriptor(edge.GetType());

            EdgeState state = new EdgeState()
            {
                type = descriptor == null ? null : descriptor.identifier.ToString(),
                guid = edge.viewDataKey,
                output = edge.output.viewDataKey,
                input = edge.input.viewDataKey,
            };

            if (edge is ISerializable serializable)
                serializable.Serialize(state);

            return state;
        }

        public static void ApplyEdgeState(EdgeState state, Edge edge, GraphView graphView)
        {
            edge.viewDataKey = state.guid;

            if (edge.input != null && edge.output != null)
            {
                // disconnect existing connected ports
                edge.input.Disconnect(edge);
            }

            edge.input = graphView.GetPortByGuid(state.input);
            edge.output = graphView.GetPortByGuid(state.output);

            // connect edge with the new input and output
            edge.input.Connect(edge);
            edge.output.Connect(edge);

            if (edge is ISerializable serializable)
                serializable.Deserialize(state);
        }

        public static List<EdgeState> GetEdgeState(GraphView graphView)
        {
            List<EdgeState> edges = new List<EdgeState>();
            foreach (Edge edge in graphView.edges)
            {
                // make sure the edge has an input and an output
                if (edge.input != null && edge.output != null)
                    edges.Add(CreateEdgeState(edge));
            }
            return edges;
        }

        public static GraphState CreateGraphState(GraphView graphView)
        {
            GraphState graphState = new GraphState()
            {
                nodes = GetNodeState(graphView),
                edges = GetEdgeState(graphView),
                data = new GraphStateData()
            };

            graphState.data.Add("viewPosition", graphView.viewTransform.position);
            graphState.data.Add("viewScale", graphView.viewTransform.scale);

            return graphState;
        }

        public static void ApplyGraphState(
            GraphState state,
            StateGraphView graphView,
            StateNodeViewFactory nodeFactory)
        {
            try
            {
                Vector3 viewPosition = state.GetVector3("viewPosition");
                graphView.viewTransform.position = viewPosition;
                Vector3 viewScale = state.GetVector3("viewScale");
                graphView.viewTransform.scale = viewScale;
            }
            catch (KeyNotFoundException) { }

            ApplyNodeStates(graphView, state.nodes);
            ApplyEdgeStates(graphView, state.edges);
        }

        public static List<GraphElement> ApplyGraphStateCopy(
            GraphState copyState,
            StateGraphView graphView,
            StateNodeViewFactory nodeFactory)
        {
            // assign new guids to all elements in the state
            Dictionary<string, string> copyByOriginalPorts = new Dictionary<string, string>();

            foreach (NodeState nodeState in copyState.nodes)
            {
                // add an offset to the node position so that copy 
                // is not at the exact same position as the original
                nodeState.xPos += 50;
                nodeState.yPos += 50;

                nodeState.guid = CreateGuid();

                for (int i = 0; i < nodeState.inputPorts.Length; i++)
                {
                    string inPort = nodeState.inputPorts[i];
                    string newId = CreateGuid();

                    copyByOriginalPorts.Add(inPort, newId);
                    nodeState.inputPorts[i] = newId;
                }

                for (int i = 0; i < nodeState.outputPorts.Length; i++)
                {
                    string outPort = nodeState.outputPorts[i];
                    string newId = CreateGuid();

                    copyByOriginalPorts.Add(outPort, newId);
                    nodeState.outputPorts[i] = newId;
                }
            }

            // remap edges
            for (int i = copyState.edges.Count - 1; i >= 0; i--)
            {
                EdgeState edgeState = copyState.edges[i];
                edgeState.guid = CreateGuid();
                // remove edges that connect with nodes outside of the copy selection
                if (!copyByOriginalPorts.TryGetValue(edgeState.input, out string input) ||
                   !copyByOriginalPorts.TryGetValue(edgeState.output, out string output))
                {
                    copyState.edges.RemoveAt(i);
                    continue;
                }

                edgeState.input = input;
                edgeState.output = output;
            }

            List<GraphElement> copiedElements = new List<GraphElement>();

            copiedElements.AddRange(ApplyNodeStateCopy(graphView, copyState.nodes));
            copiedElements.AddRange(ApplyEdgeStatesCopy(graphView, copyState.edges));

            return copiedElements;
        }

        public static void SaveGraph(StateGraphAsset asset, GraphView graphView)
        {
            asset.GraphState = CreateGraphState(graphView);

            EditorUtility.SetDirty(asset);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        public static void LoadGraph(
            StateGraphAsset asset,
            StateGraphView graphView,
            StateNodeViewFactory elementFactory)
        {
            ApplyGraphState(asset.GraphState, graphView, elementFactory);
        }
    }
}
