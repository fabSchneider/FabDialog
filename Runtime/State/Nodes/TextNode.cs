using System.Collections;
using UnityEngine;
using UnityEngine.UIElements;

namespace Fab.Dialog
{
    [StateNode(
    name: "Text/Text",
    identifier: "663A2836-9EDE-45FE-A749-B60C564487FD")]
    public class TextNode : StateNode
    {
        private TextField textField;
        public override void RegisterParameters()
        {
            AddOutput(typeof(string), "Out");
        }

        public override void OnCreateGUI(VisualElement root)
        {
            base.OnCreateGUI(root);
            VisualElement contentsContainer = root.Q("contents");

            textField = new TextField();
            textField.RegisterValueChangedCallback(change =>
            {
                Outputs[0].Value = change.newValue;
                NeedsResolve = true;
            });

            textField.value = "Hello World";
            Outputs[0].Value = textField.value;
            contentsContainer.Add(textField);
        }

        public override void Resolve()
        {
        }

        protected override void Serialize(NodeState state)
        {
            state.Add("text", Outputs[0].GetOrDefault<string>());
        }

        protected override void Deserialize(NodeState state)
        {
            string text = state.GetString("text");
            Outputs[0].Value = text;
            textField?.SetValueWithoutNotify(text);
        }

    }
}