using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace Fab.Dialog.Editor.Elements
{
    public class MultiChoiceNode : DialogChoiceNode
    {

        public override DialogNodeData Serialize()
        {
            DialogNodeData data = base.Serialize();
            data.Outputs = choicePorts.Select(p => new PortData() { Id = p.viewDataKey, Name = p.Q<TextField>().text }).ToList();
            return data;
        }

        protected override void Deserialize(DialogNodeData nodeData)
        {
            base.Deserialize(nodeData);

            NodeType = DialogNodeType.MultiChoice;

            if(nodeData.Outputs == null)
            {
                Port choicePort = CreateChoicePort("New Choice");
                choicePorts.Add(choicePort);
                outputContainer.Add(choicePort);
            }
            else
            {
                foreach (PortData port in nodeData.Outputs)
                {
                    Port choicePort = CreateChoicePort(port.Name);
                    choicePort.viewDataKey = port.Id;
                    choicePorts.Add(choicePort);
                    outputContainer.Add(choicePort);
                }
            }

            Button addChoiceButton = DialogElementUtility.CreateButton("Add Choice", () =>
            {
                Port choicePort = CreateChoicePort("New Choice");

                choicePorts.Add(choicePort);
                outputContainer.Add(choicePort);
            });

            mainContainer.Insert(1, addChoiceButton);
            RefreshExpandedState();
        }

        public Port CreateChoicePort(string name)
        {
            Port choicePort = Port.Create<WeightedEdge>(
                Orientation.Horizontal, 
                Direction.Output,
                Port.Capacity.Multi, 
                typeof(bool));

            choicePort.portName = "";

            Button deleteChoiceButton = DialogElementUtility.CreateButton("x", () =>
            {
                if (choicePorts.Count < 2)
                    return;

                if (choicePort.connected)
                {
                    graphView.DeleteElements(choicePort.connections);
                }

                graphView.RemoveElement(choicePort);
                choicePorts.Remove(choicePort);
            });


            TextField choiceTextField = DialogElementUtility.CreateTextField(name);
            choiceTextField.AddToClassList(DialogElementUtility.choiceTextFieldClassname);

            choicePort.Add(choiceTextField);
            choicePort.Add(deleteChoiceButton);

            return choicePort;
        }
    }
}
