using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Fab.Dialog
{
    [StateNode(
    name: "Flow/Probability",
    identifier: "93145428-F2DD-4306-AB45-168A589A7069")]
    public class ProbabilityNode : ExecuteNode
    {
        private static readonly string probabilityFormat = "{0:0.0}";

        private List<float> probabilities = new List<float> { 1f };

        public override void RegisterParameters()
        {
            AddExecuteInput("In");
            AddExecuteOutput(string.Format(probabilityFormat, probabilities[0]));
        }

        public override void OnCreateParameterGUI(NodeParameter parameter, VisualElement parameterElement, bool isInput)
        {
            if (!isInput)
            {
                Label paramLabel = parameterElement.Q<Label>();
                paramLabel.style.display = DisplayStyle.None;

                FloatField floatField = new FloatField();
                int paramIdx = outputs.IndexOf(parameter);
                floatField.value = probabilities[paramIdx];
                parameter.Name = string.Format(probabilityFormat, floatField.value);
                paramLabel.text = parameter.Name;
                floatField.RegisterValueChangedCallback(change =>
                {
                    parameter.Name = string.Format(probabilityFormat, change.newValue);
                    paramLabel.text = parameter.Name;
                    probabilities[paramIdx] = change.newValue;
                });

                floatField.AddToClassList("dialog-node__text-field");
                floatField.AddToClassList("dialog-node__text-field--hidden");
                floatField.AddToClassList("dialog-node__choice-text-field");
                parameterElement.Add(floatField);

                parameterElement.Add(StateGraphGUI.CreateHelpButton(() => Debug.Log(parameter.Name)));
            }
        }

        public override void OnCreateGUI(VisualElement root)
        {
            base.OnCreateGUI(root);
            VisualElement contentsContainer = root.Q("contents");

            Button addButton = new Button(() =>
            {
                probabilities.Add(0f);
                AddExecuteOutput(string.Format(probabilityFormat, probabilities[probabilities.Count - 1]));
            });
            addButton.text = "Add Path";
            contentsContainer.Add(addButton);

            Button removeButton = new Button(() =>
            {
                if (Outputs.Count > 1)
                {
                    probabilities.RemoveAt(Outputs.Count - 1);
                    RemoveOutput(Outputs.Count - 1);
                }
            });
            removeButton.text = "Remove Path";
            contentsContainer.Add(removeButton);
        }

        public override NodeParameter Execute()
        {
            float weightSum = 0f;
            foreach (float weight in probabilities)
                weightSum += weight;

            // pick next
            float pick = UnityEngine.Random.Range(0f, weightSum);

            // TODO: make this more robust
            float currentWeight = 0f;
            for (int i = 0; i < probabilities.Count; i++)
            {
                currentWeight += probabilities[i];
                if (pick <= currentWeight)
                {
                    return Outputs[i];
                }
            }

            throw new InvalidOperationException("This should not happen");
        }

        public override void Resolve() { }

        protected override void Serialize(NodeState nodeState)
        {
            nodeState.Add("probabilities", probabilities);
        }

        protected override void Deserialize(NodeState state)
        {
            if (state.Contains("probabilities"))
            {
                probabilities = new List<float>(state.GetFloatArray("probabilities"));

                for (int i = 0; i < probabilities.Count; i++)
                {
                    if (i < Outputs.Count)
                        Outputs[i].Name = string.Format(probabilityFormat, probabilities[i]);
                    else
                        AddExecuteOutput(string.Format(probabilityFormat, probabilities[i]));
                }
            }
        }
    }
}
