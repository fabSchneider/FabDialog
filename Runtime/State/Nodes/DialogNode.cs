using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace Fab.Dialog
{
    [StateNode(
        name: "Dialog/Dialog",
        identifier: "0F0E8A56-E391-49F3-9FDB-8F8959FD524D")]
    public class DialogNode : ExecuteNode
    {
        public string Text => Inputs[1].GetOrDefault<string>();

        public override void RegisterParameters()
        {
            AddExecuteInput("In");
            AddGenericInput("Text");
            AddExecuteOutput("Next", true);
        }

        public override void OnCreateGUI(VisualElement root)
        {
            base.OnCreateGUI(root);
            VisualElement contentsContainer = root.Q("contents");

            Button addButton = new Button(() =>
            {
                AddExecuteOutput("New Choice", true);
            });
            addButton.text = "Add Choice";
            contentsContainer.Add(addButton);

            Button removeButton = new Button(() =>
            {
                if(Outputs.Count > 1)
                    RemoveOutput(Outputs.Count - 1);
            });
            removeButton.text = "Remove Choice";
            contentsContainer.Add(removeButton);
        }

        public override NodeParameter Execute()
        {
            return Outputs[0];
        }

        public override void Resolve() { }

        protected override void Serialize(NodeState nodeState)
        {
            string[] choiceNames = new string[Outputs.Count];

            for (int i = 0; i < Outputs.Count; i++)
            {
                choiceNames[i] = Outputs[i].Name;
            }
            nodeState.Add("choices", choiceNames);
        }

        protected override void Deserialize(NodeState state)
        {
            if (state.Contains("choices"))
            {
                string[] choiceNames = state.GetStringArray("choices");
                Outputs[0].Name = choiceNames[0];

                for (int i = 1; i < choiceNames.Length; i++)
                {
                    if (i < Outputs.Count)
                        Outputs[i].Name = choiceNames[i];
                    else
                        AddExecuteOutput(choiceNames[i], true);
                }
            }
        }
    }
}
