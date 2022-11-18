using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace Fab.Dialog.Editor.Elements
{


    public class DialogChoiceNode : DialogNode
    {
        protected string ResolveText(string text)
        {
            foreach (Port textInput in textInputPorts)
            {
                text = text.Replace($"{{{textInput.portName}}}", textInput.userData as string);
            }
            return text;
        }


        public static readonly string textFoldoutName = "text-foldout";

        protected string resolvedText;
        public string Text
        {
            get => resolvedText;
            set
            {
                resolvedText = value;
                if (resolvedTextLabel != null)
                    resolvedTextLabel.text = value;
            }
        }

        public string RawText => textEditor.Q<TextField>().text;

        public DialogChoiceNode() : base() { }

        protected VisualElement textEditor;
        protected Label resolvedTextLabel;

        protected List<Port> choicePorts = new List<Port>();
        public IEnumerable<Port> ChoicePorts => choicePorts;


        protected Port inPort;
        protected List<Port> textInputPorts = new List<Port>();

        public override void UpdateInputs()
        {
            Text = ResolveText(RawText);
        }

        protected override void Deserialize(DialogNodeData nodeData)
        {
            base.Deserialize(nodeData);

            //Choices = new List<DialogChoiceData>(nodeData.Choices.Count);

            //foreach (DialogChoiceData choice in nodeData.Choices)
             //   Choices.Add(new DialogChoiceData(choice));

            Text = ResolveText(nodeData.Text);

            inPort = Port.Create<WeightedEdge>(
               Orientation.Horizontal,
               Direction.Input,
               Port.Capacity.Multi,
               typeof(bool));

            if (nodeData.Inputs != null && nodeData.Inputs.Count > 0)
            {
                if (!string.IsNullOrEmpty(nodeData.Inputs[0].Id))
                    inPort.viewDataKey = nodeData.Inputs[0].Id;

                inPort.portName = nodeData.Inputs[0].Name;
            }
            else
            {
                inPort.portName = "In";
            }
            inputContainer.Add(inPort);

            // add text input ports
            if (nodeData.Inputs != null)
            {
                for (int i = 1; i < nodeData.Inputs.Count; i++)
                {
                    AddTextInputPort(nodeData.Inputs[i]);
                }               
            }

            VisualElement customDataContainer = DialogElementUtility.CreateCustomDataContainer();

            resolvedTextLabel = new Label(Text);
            resolvedTextLabel.AddToClassList(DialogElementUtility.textFieldClassname);

            textEditor = DialogElementUtility.CreateQuoteTextEditor(
                "Dialog Text",
                nodeData.Text,
                nodeData.TextCollapsed,
                change =>
                {
                    Text = ResolveText(change.newValue);
                });


            TextField editorTextField = textEditor.Q<TextField>();

            // TODO: Create manipulator for this

            editorTextField.RegisterCallback<ContextualMenuPopulateEvent>(evt =>
            {
                evt.menu.AppendAction("Create variable", action =>
                {
                    string selection = editorTextField.text;
                    int startIndex = Mathf.Min(editorTextField.cursorIndex, editorTextField.selectIndex);
                    int endIndex = Mathf.Max(editorTextField.cursorIndex, editorTextField.selectIndex);

                    selection = selection.Substring(startIndex, endIndex - startIndex);

                    selection = selection.Trim();
                    if (!string.IsNullOrEmpty(selection))
                    {
                        AddTextInputPort(new PortData() { Name = selection });

                        string newText = editorTextField.text;
                        newText = newText.Insert(startIndex, "{");
                        newText = newText.Insert(endIndex + 1, "}");

                        editorTextField.value = newText;

                        editorTextField.SelectRange(startIndex, startIndex);
                    }
                });
                evt.StopPropagation();
            });

            textEditor.Add(resolvedTextLabel);

            customDataContainer.Add(textEditor);
            extensionContainer.Add(customDataContainer);
        }

        private void AddTextInputPort(PortData portData)
        {
            Port port = InstantiatePort(Orientation.Horizontal, Direction.Input, Port.Capacity.Single, typeof(string));
            port.portName = portData.Name;
            if (!string.IsNullOrEmpty(portData.Id))
                port.viewDataKey = portData.Id;

            inputContainer.Add(port);
            textInputPorts.Add(port);
        }

        public override DialogNodeData Serialize()
        {
            DialogNodeData data = base.Serialize();

            List<PortData> inputs = new List<PortData>(textInputPorts.Count + 1);
            inputs.Add(new PortData() { Id = inPort.viewDataKey, Name = inPort.portName });
            foreach (Port port in textInputPorts)
            {
                inputs.Add(new PortData() { Id = port.viewDataKey, Name = port.portName });
            }

            data.Text = RawText;
            data.TextCollapsed = !textEditor.Q<Foldout>().value;
            data.Inputs = inputs;
            data.Outputs = choicePorts.Select(p => new PortData() { Id = p.viewDataKey, Name = p.portName }).ToList();
            return data;
        }
    }
}
