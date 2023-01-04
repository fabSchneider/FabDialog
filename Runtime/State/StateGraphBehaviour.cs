using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace Fab.Dialog
{
    public class StateGraphBehaviour : MonoBehaviour
    {
        [SerializeField]
        protected StateGraphAsset asset;

        protected StateGraph graph;
        public StateGraph Graph => graph;

        protected StateGraphResolver resolver;

        protected void Awake()
        {
            graph = new StateGraph(asset.GraphState);

            if(graph.FindNode<MissingNode>() != null)
            {
                Debug.LogError("Graph contains missing nodes. Please resolve missing nodes before playing.");
                enabled = false;
            }

            resolver = new StateGraphResolver();
            resolver.ResolveGraph(graph, graph.Nodes);
        }

        public void Update()
        {
            Resolve();
        }

        #region Property Methods

        /// <summary>
        /// Triggers an immediate resolving of the graph.
        /// </summary>
        public void Resolve()
        {
            resolver.ResolveGraph(graph, graph.Nodes.Where(n => n.NeedsResolve));
        }

        /// <summary>
        /// Gets the first entry node found in the graph
        /// </summary>
        /// <returns></returns>
        public EntryNode GetEntry()
        {
            return graph.FindNode<EntryNode>();
        }

        /// <summary>
        /// Returns the value of a given bool property.
        /// </summary>
        public bool GetBool(string propertyName)
        {
            BoolNode propNode = graph.FindNode<BoolNode>(propertyName);
            return propNode.Value;
        }

        /// <summary>
        /// Returns the value of a given bool property.
        /// </summary>
        public void SetBool(string propertyName, bool value)
        {
            BoolNode propNode = graph.FindNode<BoolNode>(propertyName);
            propNode.Value = value;
        }

        /// <summary>
        /// Returns the value of a given int property.
        /// </summary>
        public int GetInt(string propertyName)
        {
            IntNode propNode = graph.FindNode<IntNode>(propertyName);
            return propNode.Value;
        }

        /// <summary>
        /// Returns the value of a given int property.
        /// </summary>
        public void SetInt(string propertyName, int value)
        {
            IntNode propNode = graph.FindNode<IntNode>(propertyName);
            propNode.Value = value;
        }

        /// <summary>
        /// Returns the value of a given float property.
        /// </summary>
        public float GetFloat(string propertyName)
        {
            FloatNode propNode = graph.FindNode<FloatNode>(propertyName);
            return propNode.Value;
        }

        /// <summary>
        /// Returns the value of a given float property.
        /// </summary>
        public void SetFloat(string propertyName, float value)
        {
            FloatNode propNode = graph.FindNode<FloatNode>(propertyName);
            propNode.Value = value;
        }

        /// <summary>
        /// Returns the value of a given string property.
        /// </summary>
        public string GetString(string propertyName)
        {
            StringNode propNode = graph.FindNode<StringNode>(propertyName);
            return propNode.Value;
        }

        /// <summary>
        /// Returns the value of a given string property.
        /// </summary>
        public void SetString(string propertyName, string value)
        {
            StringNode propNode = graph.FindNode<StringNode>(propertyName);
            propNode.Value = value;
        }

        #endregion Property Methods
    }
}
