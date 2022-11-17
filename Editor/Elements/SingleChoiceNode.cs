using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

namespace Fab.Dialog.Editor.Elements
{
    public class SingleChoiceNode : DialogChoiceNode
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
                Port choicePort = Port.Create<WeightedEdge>(
                    Orientation.Horizontal,
                    Direction.Output,
                    Port.Capacity.Multi,
                    typeof(bool));

                choicePort.portName = choice.Text;
                choicePort.userData = choice;
                outputContainer.Add(choicePort);
            }

            RefreshExpandedState();
        }
    }
}
