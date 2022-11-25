using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace Fab.Dialog
{
    [StateNode(
    name: "Properties/Set Bool",
    identifier: "F6269293-4431-4BEE-8C77-F4B20B3CD4DC")]
    public class SetBoolNode : ExecuteNode
    {
        private string propName;
        private bool newVal;

        public override void RegisterParameters()
        {
            AddExecuteInput("In");
            AddExecuteOutput("Out");
        }

        public override void OnCreateGUI(VisualElement root)
        {
            VisualElement contentsContainer = root.Q("contents");

            TextField propNameField = new TextField();
            propNameField.label = "Property Name";
            propNameField.value = propName;
            propNameField.RegisterValueChangedCallback(change => propName = change.newValue);
            Toggle boolField = new Toggle();
            boolField.label = "New Value";
            boolField.value = newVal;
            boolField.RegisterValueChangedCallback(change => newVal = change.newValue);
            contentsContainer.Add(propNameField);
            contentsContainer.Add(boolField);
        }

        public override NodeParameter Execute()
        {
            BoolNode node = Owner.FindNode<BoolNode>(name: propName);
            if (node != null)
            {
                node.Value = newVal;
            }
            return Outputs[0];
        }

        public override void Resolve() {}

        protected override void Serialize(NodeState nodeState)
        {
            nodeState.Add("propName", propName);
            nodeState.Add("newVal", newVal);
        }

        protected override void Deserialize(NodeState nodeState)
        {
            try
            {
                propName = nodeState.GetString("propName");
                newVal = nodeState.GetBool("newVal");
            }

            catch (KeyNotFoundException) { }

        }
    }
}
