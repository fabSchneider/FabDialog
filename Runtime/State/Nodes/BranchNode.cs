using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Fab.Dialog
{
    [StateNode(
        name: "Logic/Branch",
        identifier: "6138BE93-3C3B-4F51-93AF-ADBEEEA923F3")]
    public class BranchNode : ExecuteNode
    {
        public override void RegisterParameters()
        {
            AddExecuteInput("In");
            AddInput(typeof(bool), "Pick A");

            AddExecuteOutput("A");
            AddExecuteOutput("B");
        }

        public override void Resolve()
        {
           
        }

        public override NodeParameter Execute()
        {
            return Inputs[1].GetOrDefault<bool>() ? Outputs[0] : Outputs[1];
        }

    }
}
