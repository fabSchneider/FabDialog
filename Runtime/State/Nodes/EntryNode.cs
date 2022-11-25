using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Fab.Dialog
{
    [StateNode(
    name: "Dialog/Entry",
    identifier: "5473DFAA-8C2F-4293-A75A-1326E974AEFA")]
    public class EntryNode : ExecuteNode
    {
        public override void RegisterParameters()
        {
            AddExecuteOutput("Out");
        }

        public override NodeParameter Execute()
        {
            return Outputs[0];
        }

        public override void Resolve()
        {
            
        }
    }
}
