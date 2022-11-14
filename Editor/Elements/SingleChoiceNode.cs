using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

namespace Fab.Dialog.Editor.Elements
{
    public class SingleChoiceNode : DialogNode
    {
        protected override void InitializeInternal(GraphView graphView, DialogNodeData nodeData)
        {
            base.InitializeInternal(graphView, nodeData);

            if (Choices.Count == 0)
                Choices.Add(new DialogChoiceData("Next"));

            DialogType = DialogType.SingleChoice;
        }

        protected override void Draw()
        {
            base.Draw();

            foreach (DialogChoiceData choice in Choices)
            {
                Port choicePort = this.CreatePort(choice.Text, direction: Direction.Output, capacity: Port.Capacity.Single);
                choicePort.userData = choice;
                outputContainer.Add(choicePort);
            }

            RefreshExpandedState();
        }
    }
}
