using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace Fab.Dialog.Editor.Elements
{
    public class DialogNode : Node
    {
        public string ID => viewDataKey;
        public string NodeName { get; set; }
        public DialogNodeType NodeType { get; protected set; }
        protected DialogGraphView graphView;
        

        public DialogNode() : base() { }

        public void Initialize(DialogGraphView graphView, Vector2 position)
        {
            // Create default
            DialogNodeData nodeData = new DialogNodeData()
            {
                Id = viewDataKey,
                Name = "Name",
                Text = "Add some dialog here.",
                TextCollapsed = false,
                Position = position
            };

            Initialize(graphView, nodeData);
        }

        public void Initialize(DialogGraphView graphView, DialogNodeData nodeData)
        {
            this.graphView = graphView;
            if(!string.IsNullOrEmpty(nodeData.Id))
                viewDataKey = nodeData.Id;
            NodeName = nodeData.Name;

            Deserialize(nodeData);
        }

        public virtual void UpdateInputs()
        {

        }

        public virtual DialogNodeData Serialize()
        {
            DialogNodeData data = new DialogNodeData()
            {
                Id = viewDataKey,
                Name = NodeName,
                DialogType = NodeType,
                Position = GetPosition().position,
                Outputs = new List<PortData>(),
                Inputs = new List<PortData>()
            };

            return data;
        }

        protected virtual void Deserialize(DialogNodeData nodeData) 
        {
            SetPosition(new Rect(nodeData.Position, Vector2.zero));
            mainContainer.AddToClassList(DialogElementUtility.mainClassname);
            extensionContainer.AddToClassList(DialogElementUtility.extensionClassname);

            TextField nameTextField = DialogElementUtility.CreateTextField(NodeName, change =>
            {
                NodeName = change.newValue;
            });

            nameTextField.AddToClassList(DialogElementUtility.textFieldClassname);
            nameTextField.AddToClassList(DialogElementUtility.nameTextFieldClassname);
            nameTextField.AddToClassList(DialogElementUtility.textFieldHiddenClassname);

            titleContainer.Insert(0, nameTextField);
        }
    }
}
