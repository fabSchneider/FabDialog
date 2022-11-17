using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace Fab.Dialog.Editor.Elements
{
    public class MultiChoiceNode : DialogChoiceNode
    {
        protected override void InitializeInternal(GraphView graphView, DialogNodeData nodeData)
        {
            base.InitializeInternal(graphView, nodeData);
            if(Choices.Count == 0)
                Choices.Add(new DialogChoiceData("New Choice"));

            DialogType = DialogType.MultiChoice;
        }

        protected override void Draw()
        {
            base.Draw();

            Button addChoiceButton = DialogElementUtility.CreateButton("Add Choice", () =>
            {
                DialogChoiceData choice = new DialogChoiceData("New Choice");
                Port choicePort = CreateChoicePort(choice);
                Choices.Add(choice);

                outputContainer.Add(choicePort);
            });

            addChoiceButton.AddToClassList(buttonClassname);

            mainContainer.Insert(1, addChoiceButton);

            foreach (DialogChoiceData choice in Choices)
            {
                Port choicePort = CreateChoicePort(choice);
                outputContainer.Add(choicePort);
            }
            RefreshExpandedState();
        }

        public Port CreateChoicePort(DialogChoiceData choice)
        {
            Port choicePort = Port.Create<WeightedEdge>(
                Orientation.Horizontal, 
                Direction.Output,
                Port.Capacity.Multi, 
                typeof(bool));

            choicePort.portName = choice.Text;
            choicePort.userData = choice;

            Button deleteChoiceButton = DialogElementUtility.CreateButton("x", () =>
            {
                if (Choices.Count < 2)
                    return;

                if (choicePort.connected)
                {
                    graphView.DeleteElements(choicePort.connections);
                }

                DialogChoiceData choiceData = (DialogChoiceData)choicePort.userData;

                Choices.Remove(choiceData);
                graphView.RemoveElement(choicePort);

            });

            deleteChoiceButton.AddToClassList(buttonClassname);


            TextField choiceTextField = DialogElementUtility.CreateTextField(choice.Text, change =>
            {
                choice.Text = change.newValue;
            });

            choiceTextField.AddToClassList(textFieldClassname);
            choiceTextField.AddToClassList(textFieldHiddenClassname);
            choiceTextField.AddToClassList(choiceTextFieldClassname);

            choicePort.Add(choiceTextField);
            choicePort.Add(deleteChoiceButton);

            return choicePort;
        }
    }
}
