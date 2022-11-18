using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

namespace Fab.Dialog.Editor.Elements
{
    public class SingleChoiceNode : DialogChoiceNode
    {
        protected override void Deserialize(DialogNodeData nodeData)
        {
            base.Deserialize(nodeData);

            NodeType = DialogNodeType.SingleChoice;

            if (nodeData.Outputs == null)
            {
                Port choicePort = Port.Create<WeightedEdge>(
                Orientation.Horizontal,
                Direction.Output,
                Port.Capacity.Multi,
                typeof(bool));

                choicePort.portName = "Next";

                choicePorts.Add(choicePort);
                outputContainer.Add(choicePort);
            }
            else
            {
                foreach (PortData port in nodeData.Outputs)
                {
                    Port choicePort = Port.Create<WeightedEdge>(
                        Orientation.Horizontal,
                        Direction.Output,
                        Port.Capacity.Multi,
                        typeof(bool));

                    choicePort.portName = port.Name;
                    choicePort.viewDataKey = port.Id;
                    choicePorts.Add(choicePort);
                    outputContainer.Add(choicePort);
                }
            }

            RefreshExpandedState();
        }
    }
}
