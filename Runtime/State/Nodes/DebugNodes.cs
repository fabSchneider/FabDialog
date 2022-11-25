using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace Fab.Dialog
{
    [StateNode(
        name: "Debug/Output",
        identifier: "0F0B5DC1-4214-4581-B876-3F9412FAC8FB")]
    public class OutputNode : StateNode
    {
        Label label;

        public override void RegisterParameters()
        {
            AddGenericInput("In");
            AddGenericOutput("Out");
        }

        public override void OnCreateGUI(VisualElement root)
        {
            base.OnCreateGUI(root);
            VisualElement contentsContainer = root.Q("contents");

            label = new Label();
            label.text = "Hello World";
            contentsContainer.Add(label);
        }

        public override void Resolve()
        {
            label.text = Inputs[0].GetOrDefault<string>();

            Outputs[0].Value = Inputs[0].Value;
        }
    }

    [StateNode(
        name: "Debug/Log",
        identifier: "73E8DB5E-4C5D-4681-A18D-94BE81382E8C")]
    public class LogNode : StateNode
    {

        public override void RegisterParameters()
        {
            AddGenericInput("In");
        }

        public override void Resolve()
        {
            Debug.Log(Inputs[0].Value);
        }
    }
}
