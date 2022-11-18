using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace Fab.Dialog.Editor.Elements
{
    public class TextNode : DialogNode
    {
        protected VisualElement textEditor;

        public string Text { get; set; }

        protected Port outPort;

        public override DialogNodeData Serialize()
        {
            List<string> textOutputs = new List<string>();

            foreach (Edge e in outPort.connections)
            {
                textOutputs.Add(e.input.viewDataKey);
            }

            DialogNodeData data = base.Serialize();

            data.Text = Text;
            data.TextCollapsed = !textEditor.Q<Foldout>().value;
            data.Outputs = new List<PortData>() { new PortData() { Id = outPort.viewDataKey, Name = outPort.portName } };
            return data;
        }

        protected override void Deserialize(DialogNodeData nodeData)
        {
            base.Deserialize(nodeData);

            Text = nodeData.Text;
            NodeType = DialogNodeType.Text;

            outPort = Port.Create<Edge>(
                Orientation.Horizontal,
                Direction.Output,
                Port.Capacity.Multi,
                typeof(string));

            if (nodeData.Outputs != null && nodeData.Outputs.Count > 0)
            {
                outPort.viewDataKey = nodeData.Outputs[0].Id;
                outPort.portName = nodeData.Outputs[0].Name;
            }
            else
            {
                outPort.portName = "Out";
            }

            outputContainer.Add(outPort);

            textEditor = DialogElementUtility.CreateQuoteTextEditor(
                "Text",
                Text,
                nodeData.TextCollapsed,
                change => RefreshOutput());

            VisualElement customDataContainer = DialogElementUtility.CreateCustomDataContainer();
            TextField editorTextField = textEditor.Q<TextField>();

            customDataContainer.Add(textEditor);
            extensionContainer.Add(customDataContainer);

            RefreshOutput();

            RefreshExpandedState();
        }

        private void RefreshOutput()
        {
            Text = textEditor.Q<TextField>().text;
            outPort.userData = Text;
            foreach (Edge e in outPort.connections)
            {
                graphView.FlagRefresh(e);
            }
        }

    }
}