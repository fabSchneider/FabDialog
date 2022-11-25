using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Fab.Dialog
{
    [StateNode(
    name: "Logic/Select",
    identifier: "FA7C386F-2C4F-4109-827F-DB7DD0E6B185")]
    public class SelectNode : StateNode
    {
        public override void RegisterParameters()
        {
            AddGenericInput("A");
            AddGenericInput("B");
            AddInput(typeof(bool), "Select A");

            AddGenericOutput("Out");
        }

        public override void Resolve()
        {
            bool selectA = Inputs[2].GetOrDefault<bool>();
            Outputs[0].Value = selectA ? Inputs[0].Value : Inputs[1].Value; 
        }
    }
}
