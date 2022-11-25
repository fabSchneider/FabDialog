using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace Fab.Dialog
{
    public abstract class PropertyNode<T> : StateNode
    {
        public T Value
        {
            get => (T)Outputs[0].Value;
            set
            {
                if (value.GetType() != typeof(T))
                    throw new InvalidCastException("Cannot set value of property from a " + value.GetType().Name);

                Outputs[0].Value = value;
                NeedsResolve = true;
            }
        
        }

        public override void RegisterParameters()
        {
            Type type = typeof(T);
            AddOutput(type, type.Name);
        }

        public override void Resolve() { }

        protected override void Serialize(NodeState state)
        {
            state.Add("value", Outputs[0].GetOrDefault<T>());
        }
    }


    [StateNode(
        name: "Properties/Integer",
        identifier: "DC03E351-8C36-4D84-84C5-3DD7873E3ADB")]
    public class IntNode : PropertyNode<int>
    {
        protected override void Deserialize(NodeState state)
        {
            base.Deserialize(state);
            Outputs[0].Value = state.GetInt("value");
        }
    }

    [StateNode(
        name: "Properties/Float",
        identifier: "4FB7B1AA-7B64-4CC4-81BB-D8E43864E8F5")]
    public class FloatNode : PropertyNode<float>
    {
        protected override void Deserialize(NodeState state)
        {
            Outputs[0].Value = state.GetFloat("value");
        }
    }

    [StateNode(
        name: "Properties/Boolean",
        identifier: "C93E6C8D-BA77-41F5-BA7F-A43AFA378D82")]
    public class BoolNode : PropertyNode<bool>
    {
        public override void OnCreateGUI(VisualElement root)
        {
            base.OnCreateGUI(root);
            VisualElement contentsContainer = root.Q("contents");

            Toggle toggleField = new Toggle();
            toggleField.RegisterValueChangedCallback(change =>
            {
                Outputs[0].Value = change.newValue;
                NeedsResolve = true;
            });
            contentsContainer.Add(toggleField);
        }

        protected override void Deserialize(NodeState state)
        {
            Outputs[0].Value = state.GetBool("value");
        }
    }

    [StateNode(
        name: "Properties/String",
        identifier: "1A18DA51-9E6B-46F9-9F15-3C2950127A8F")]
    public class StringNode : PropertyNode<string>
    {
        protected override void Deserialize(NodeState state)
        {
            Outputs[0].Value = state.GetString("value");
        }
    }
}
