using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Fab.Dialog
{
    public class StateGraph
    {
        protected List<StateNode> nodes;
        public IEnumerable<StateNode> Nodes => nodes;
        protected Dictionary<NodeParameter, List<NodeParameter>> connectionMap;

        public StateGraph(GraphState state)
        {
            nodes = CreateNodes(state.nodes);
            connectionMap = CreateConnectionMap(state.nodes, state.edges, nodes);
        }

        public StateGraph(GraphState state, List<StateNode> stateNodes)
        {
            foreach(StateNode node in stateNodes)
            {
                node.Owner = this;
            }

            nodes = stateNodes;
            connectionMap = CreateConnectionMap(state.nodes, state.edges, stateNodes);
        }

        #region Static Methods

        private List<StateNode> CreateNodes(List<NodeState> nodeStates)
        {
            List<StateNode> nodes = new List<StateNode>();

            foreach (NodeState nodeState in nodeStates)
            {
                StateNodeDescriptor descriptor = StateNodeLibrary.GetDescriptor(new Guid(nodeState.type));
                StateNode stateNode = (StateNode)Activator.CreateInstance(descriptor.type);
                stateNode.Deserialize(nodeState);
                stateNode.Owner = this;
                nodes.Add(stateNode);
            }

            return nodes;
        }

        private static Dictionary<NodeParameter, List<NodeParameter>> CreateConnectionMap(List<NodeState> nodeStates, List<EdgeState> edgeStates, List<StateNode> nodes)
        {
            Dictionary<string, NodeParameter> parametersByGuid = CreateParameterMapping(nodeStates, nodes);

            Dictionary<NodeParameter, List<NodeParameter>> connections = new Dictionary<NodeParameter, List<NodeParameter>>();

            foreach (EdgeState edgeState in edgeStates)
            {
                // output
                NodeParameter output = parametersByGuid[edgeState.output];
                if (!connections.TryGetValue(output, out List<NodeParameter> outConnections))
                {
                    outConnections = new List<NodeParameter>();
                    connections.Add(output, outConnections);
                }
                outConnections.Add(parametersByGuid[edgeState.input]);

                // input

                NodeParameter input = parametersByGuid[edgeState.input];
                if (!connections.TryGetValue(input, out List<NodeParameter> inConnections))
                {
                    inConnections = new List<NodeParameter>();
                    connections.Add(input, inConnections);
                }
                inConnections.Add(parametersByGuid[edgeState.output]);
            }

            return connections;
        }

        private static Dictionary<string, NodeParameter> CreateParameterMapping(List<NodeState> nodeStates, List<StateNode> stateNodes)
        {
            Dictionary<string, NodeParameter> parametersByGuid = new Dictionary<string, NodeParameter>();

            for (int i = 0; i < nodeStates.Count; i++)
            {
                NodeState nodeState = nodeStates[i];
                StateNode stateNode = stateNodes[i];
                for (int j = 0; j < nodeState.inputPorts.Length; j++)
                    parametersByGuid.Add(nodeState.inputPorts[j], stateNode.Inputs[j]);

                for (int j = 0; j < nodeState.outputPorts.Length; j++)
                    parametersByGuid.Add(nodeState.outputPorts[j], stateNode.Outputs[j]);
            }

            return parametersByGuid;
        }

        #endregion Static Methods

        #region Public Methods

        public bool IsConnected(NodeParameter parameter)
        {
            return connectionMap.ContainsKey(parameter);
        }

        public IEnumerable<NodeParameter> GetConnected(NodeParameter parameter)
        {
            if (connectionMap.TryGetValue(parameter, out List<NodeParameter> connections))
                return connections;
            return Enumerable.Empty<NodeParameter>();
        }

        /// <summary>
        /// Finds a node of the given type.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="name"></param>
        /// <returns>Returns null if no node was found</returns>
        public T FindNode<T>() where T : StateNode
        {
            return Nodes.OfType<T>().FirstOrDefault();
        }

        /// <summary>
        /// Finds the node with the given name in the graph.
        /// </summary>
        /// <param name="name"></param>
        /// <returns>Returns null if no node was found</returns>
        public StateNode FindNode(string name)
        {
            return Nodes.FirstOrDefault(n => n.Name == name);
        }

        /// <summary>
        /// Finds the node of the given type and name in the graph.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="name"></param>
        /// <returns>Returns null if no node was found</returns>
        public T FindNode<T>(string name) where T : StateNode
        {
            return Nodes.OfType<T>().FirstOrDefault(n => n.Name == name);
        }

        /// <summary>
        /// Gets the next exec node from a given output. 
        /// </summary>
        /// <returns>Returns null if no next exec node exists.</returns>
        public StateNode GetNext(StateNode origin)
        {
            if (origin == null)
                return null;

            // TODO: Implement better way to determine a node parameter is an exec node

            // get all exec outputs on the origin
            IEnumerable<NodeParameter> execOuts = origin.Outputs.Where(p => p.Type == null);
            NodeParameter execOut = execOuts.FirstOrDefault();

            // get the active output -> this is a bit weird right now
            foreach (NodeParameter exec in execOuts)
            {
                if (exec.TryGet(out bool val) && val)
                {
                    execOut = exec;
                    break;
                }
            }

            // get the first connected node from the selected output
            // if we allow multiple connections from one out we need another mechanism here
            if (execOut != null)
            {
                if (connectionMap.TryGetValue(execOut, out List<NodeParameter> connections))
                    return connections[0].Owner;
            }
            return null;
        }

        #endregion Public Methods

    }
}
