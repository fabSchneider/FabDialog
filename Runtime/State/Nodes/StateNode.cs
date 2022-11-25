using System;
using System.Collections.Generic;
using UnityEngine.UIElements;

namespace Fab.Dialog
{
    public class NodeParameter
    {
        private readonly StateNode owner;

        public StateNode Owner => owner;

        private readonly Type type;
        public string Name { get; set; }
        public bool CanRename { get; protected set; }

        public Type Type => type;
        public object Value { get; set; }

        public NodeParameter(Type type, string name, StateNode owner)
        {
            this.type = type;
            Name = name;
            this.owner = owner;
        }
        public NodeParameter(Type type, string name, StateNode owner, bool canRename)
        {
            this.type = type;
            Name = name;
            this.owner = owner;
            CanRename = canRename;
        }

        public bool TryGet<T>(out T value)
        {
            if (Value == null)
            {
                value = default(T);
                return false;
            }

            try
            {
                value = (T)Value;
                return true;
            }
            catch (InvalidCastException)
            {
                value = default(T);
                return false;
            }
        }

        public T GetOrDefault<T>()
        {
            if (Value == null)
            { 
                return default(T);
            }

            try
            {
                return (T)Value;
            }
            catch (InvalidCastException)
            {
                return default(T);
            }
        }
    }

    public abstract class StateNode : ISerializable
    {
        public StateGraph Owner { get; set; }

        public string Name { get; set; }
        public bool NeedsResolve { get; set; }

        protected List<NodeParameter> inputs;
        protected List<NodeParameter> outputs;
        public IReadOnlyList<NodeParameter> Inputs => inputs;
        public IReadOnlyList<NodeParameter> Outputs => outputs;

        public Action<bool, int> onAddedParameter;
        public Action<bool, int> onRemovingParameter;

        public StateNode()
        {
            inputs = new List<NodeParameter>();
            outputs = new List<NodeParameter>();
            RegisterParameters();

            NeedsResolve = true;
        }

        public void AddInput(Type type, string name, bool canRename = false)
        {
            NodeParameter parameter = new NodeParameter(type, name, this, canRename);
            inputs.Add(parameter);
            onAddedParameter?.Invoke(true, inputs.Count - 1);
        }

        public void AddGenericInput(string name, bool canRename = false)
        {
            AddInput(typeof(object), name, canRename);
        }
        public void AddOutput(Type type, string name, bool canRename = false)
        {
            NodeParameter parameter = new NodeParameter(type, name, this, canRename);
            outputs.Add(parameter);
            onAddedParameter?.Invoke(false, outputs.Count - 1);
        }

        public void AddGenericOutput(string name, bool canRename = false)
        {
            AddOutput(typeof(object), name, canRename);
        }

        public void RemoveInput(int index)
        {
            onRemovingParameter?.Invoke(true, index);
            inputs.RemoveAt(index);           
        }

        public void RemoveOutput(int index)
        {
            onRemovingParameter?.Invoke(false, index);
            outputs.RemoveAt(index);
        }

        public virtual void OnCreateGUI(VisualElement root)
        {

        }

        public abstract void RegisterParameters();

        public abstract void Resolve();

        public void Serialize(GraphStateBase state)
        {
            NodeState nodeState = (NodeState)state;
            nodeState.title = Name;
            Serialize(nodeState);

        }

        protected virtual void Serialize(NodeState nodeState)
        {

        }

        public void Deserialize(GraphStateBase state)
        {
            NodeState nodeState = (NodeState)state;
            Name = nodeState.title;
            Deserialize(nodeState);
        }

        protected virtual void Deserialize(NodeState nodeState)
        {

        }
    }
}
