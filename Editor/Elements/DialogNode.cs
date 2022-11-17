using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace Fab.Dialog.Editor.Elements
{
    public class DialogChoiceNode : Node
    {
        protected static readonly string ussClassname = "dialog-node";
        protected static readonly string mainClassname = ussClassname + "__main-container";
        protected static readonly string titleClassname = ussClassname + "__title-container";
        protected static readonly string extensionClassname = ussClassname + "__extension-container";
        protected static readonly string textFieldClassname = ussClassname + "__text-field";
        protected static readonly string textFieldHiddenClassname = textFieldClassname + "--hidden";
        protected static readonly string nameTextFieldClassname = ussClassname + "__name-text-field";
        protected static readonly string quoteTextFieldClassname = ussClassname + "__quote-text-field";
        protected static readonly string choiceTextFieldClassname = ussClassname + "__choice-text-field";
        protected static readonly string buttonClassname = ussClassname + "__button";
        protected static readonly string customDataClassname = ussClassname + "__custom-data-container";
        protected static readonly string helpButtonClassName = ussClassname + "__text-help-button";

        protected static readonly string textFoldoutName = "text-foldout";
        public string ID { get; set; }

        public string DialogName { get; set; }

        protected GraphView graphView;
        public List<DialogChoiceData> Choices { get; set; }
        public string Text { get; set; }
        public DialogChoiceNode() : base() { }

        public DialogType DialogType { get; set; }

        public static string CreateGuid()
        {
            return Guid.NewGuid().ToString();
        }

        public void Initialize(GraphView graphView, Vector2 position)
        {
            // Create default
            DialogNodeData nodeData = new DialogNodeData()
            {
                ID = CreateGuid(),
                Name = "DialogName",
                Choices = new List<DialogChoiceData>(),
                Text = "Add some dialog here.",
                TextCollapsed = false,
                Position = position
            };

            InitializeInternal(graphView, nodeData);
            Draw();
            this.Q<Foldout>(name: textFoldoutName).value = !nodeData.TextCollapsed;
        }

        public void Initialize(GraphView graphView, DialogNodeData nodeData)
        {
            InitializeInternal(graphView, nodeData);
            Draw();
            this.Q<Foldout>(name: textFoldoutName).value = !nodeData.TextCollapsed;
        }

        protected virtual void InitializeInternal(GraphView graphView, DialogNodeData nodeData)
        {
            this.graphView = graphView;

            ID = nodeData.ID;

            DialogName = nodeData.Name;

            List<DialogChoiceData> choices = new List<DialogChoiceData>(nodeData.Choices.Count);
            foreach (DialogChoiceData choice in nodeData.Choices)
                choices.Add(new DialogChoiceData(choice));

            Choices = choices;
            Text = nodeData.Text;

            SetPosition(new Rect(nodeData.Position, Vector2.zero));

            mainContainer.AddToClassList(mainClassname);
            extensionContainer.AddToClassList(extensionClassname);
        }

        protected virtual void Draw()
        {
            TextField nameTextField = DialogElementUtility.CreateTextField(DialogName, change =>
            {
                DialogName = change.newValue;
            });

            nameTextField.AddToClassList(textFieldClassname);
            nameTextField.AddToClassList(nameTextFieldClassname);
            nameTextField.AddToClassList(textFieldHiddenClassname);

            titleContainer.Insert(0, nameTextField);

            Port inputPort = Port.Create<WeightedEdge>(
                Orientation.Horizontal,
                Direction.Input,
                Port.Capacity.Multi,
                typeof(bool));

            inputPort.portName = "In";
            inputContainer.Add(inputPort);

            VisualElement customDataContainer = new VisualElement();
            customDataContainer.AddToClassList(customDataClassname);

            Foldout textFoldout = DialogElementUtility.CreateFoldout("Dialog Text");
            textFoldout.name = textFoldoutName;

            Button textHelpButton = new Button(() => Debug.Log("Help!!"));
            textHelpButton.text = "i";
            textHelpButton.AddToClassList(helpButtonClassName);

            textFoldout.Q(className: "unity-foldout__input").Add(textHelpButton);


            TextField textTextField = DialogElementUtility.CreateTextArea(Text, change =>
            {
                Text = change.newValue;
            });

            textTextField.RegisterCallback<MouseUpEvent>(evt =>
            {
                textTextField.panel.contextualMenuManager.DisplayMenu(evt, evt.target);

                evt.StopPropagation();
                evt.PreventDefault();
            });

            textTextField.RegisterCallback<ContextualMenuPopulateEvent>(evt =>
            {
                evt.menu.AppendAction("Create variable", action => 
                {
                    string selection = textTextField.text;
                    int startIndex = Mathf.Min(textTextField.cursorIndex, textTextField.selectIndex);
                    int endIndex = Mathf.Max(textTextField.cursorIndex, textTextField.selectIndex);

                    selection = selection.Substring(startIndex, endIndex - startIndex);

                    selection = selection.Trim();
                    if (!string.IsNullOrEmpty(selection))
                    {
                        Port port = InstantiatePort(Orientation.Horizontal, Direction.Input, Port.Capacity.Single, typeof(string));
                        port.portName = selection;
                        inputContainer.Add(port);
                    }

                    Debug.Log("\""+selection+"\"");
                });
                evt.StopPropagation();
                //evt.PreventDefault();
            });

            textTextField.AddToClassList(textFieldClassname);
            textTextField.AddToClassList(quoteTextFieldClassname);

            textFoldout.Add(textTextField);
            customDataContainer.Add(textFoldout);

            extensionContainer.Add(customDataContainer);
        }

        public DialogNodeData ToNodeData()
        {
            List<DialogChoiceData> choices = new List<DialogChoiceData>(Choices.Count);
            // create a copy of the choice data
            foreach (DialogChoiceData choice in Choices)
                choices.Add(new DialogChoiceData(choice));

            DialogNodeData data = new DialogNodeData()
            {
                ID = ID,
                Name = DialogName,
                Text = Text,
                TextCollapsed = !this.Q<Foldout>(name: textFoldoutName).value,
                Choices = choices,
                DialogType = DialogType,
                Position = GetPosition().position
            };

            return data;
        }
    }
}
