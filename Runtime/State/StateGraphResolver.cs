using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Fab.Dialog
{
    public class StateGraphResolver
    {
        public Action<StateNode> onNodeResolved;

        public void ResolveGraph(StateGraph stateGraph, IEnumerable<StateNode> dirtyNodes)
        {
            if (dirtyNodes.Count() == 0)
                return;

            // do topological sort of the graph nodes to find
            // the order of which nodes to update first
            List<StateNode> forwardSorted = TopologicalSort(stateGraph, dirtyNodes, true);

            // pull data from input nodes and resolve one after the other
            foreach (StateNode node in forwardSorted)
            {
                foreach (NodeParameter input in node.Inputs)
                {
                    foreach (NodeParameter output in stateGraph.GetConnected(input))
                    {
                        input.Value = output.Value;
                    }
                }

                node.Resolve();
                onNodeResolved?.Invoke(node);
            }
        }

        private List<StateNode> TopologicalSort(StateGraph graph, IEnumerable<StateNode> dirtyNodes, bool forward)
        {
            List<StateNode> sorted = new List<StateNode>();
            // keep track of temporary visited nodes for cycle detection
            List<StateNode> tempVisited = new List<StateNode>();
            List<StateNode> visited = new List<StateNode>();
            List<StateNode> unvisited = new List<StateNode>(dirtyNodes);

            while (unvisited.Count > 0)
            {
                Visit(unvisited[0], graph, unvisited, visited, tempVisited, sorted, forward);
            }

            return sorted;
        }

        private void Visit(
            StateNode node,
            StateGraph graph,
            List<StateNode> unvisited,
            List<StateNode> visited,
            List<StateNode> tempVisited,
            List<StateNode> sorted,
            bool forward)
        {
            // node has already been visited
            if (visited.Contains(node))
                return;
            // node has already been visited in this branch,
            // we have detected cycle and throw an exception
            if (tempVisited.Contains(node))
                throw new Exception("Cycle detected!");


            unvisited.Remove(node);
            tempVisited.Add(node);

            if (forward)
                foreach (StateNode child in GetOutputNodes(node, graph))
                    Visit(child, graph, unvisited, visited, tempVisited, sorted, forward);
            else
                foreach (StateNode child in GetInputNodes(node, graph))
                    Visit(child, graph, unvisited, visited, tempVisited, sorted, forward);

            tempVisited.Remove(node);
            visited.Add(node);
            if (forward)
                sorted.Insert(0, node);
            else
                sorted.Add(node);
        }

        private IEnumerable GetOutputNodes(StateNode node, StateGraph graph)
        {
            // ignore execute parameters
            return node.Outputs
                .Where(p => p.Type != null && graph.IsConnected(p))
                .SelectMany(p => graph.GetConnected(p))
                .Select(p => p.Owner);
        }

        private IEnumerable GetInputNodes(StateNode node, StateGraph graph)
        {
            // ignore execute parameters
            return node.Inputs
                .Where(p => p.Type != null && graph.IsConnected(p))
                .SelectMany(p => graph.GetConnected(p))
                .Select(p => p.Owner);
        }
    }
}
