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
        private string text;

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
                text = change.newValue;
                NeedsResolve = true;
            });

            textField.value = text;
            contentsContainer.Add(textField);
        }

        public override void Resolve()
        {
            Outputs[0].Value = text;
        }

        protected override void Serialize(NodeState state)
        {
            state.Add("text", text);
        }

        protected override void Deserialize(NodeState state)
        {
            text = state.GetString("text");
        }

    }
}