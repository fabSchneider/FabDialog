using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace Fab.Dialog
{
    public class MissingNode : StateNode
    {
        private NodeState missingState; 
        public override void RegisterParameters()
        {
            
        }

        public override void Resolve()
        {
        }

        public override void OnCreateGUI(VisualElement root)
        {
            root.AddToClassList(StateGraphGUI.nodeMissingClassname);
        }

        protected override void Serialize(NodeState nodeState)
        {
            nodeState.type = missingState.type;
            nodeState.title = missingState.title;
            nodeState.guid = missingState.guid;
            nodeState.data = (GraphStateData)missingState.data.Clone();
        }

        protected override void Deserialize(NodeState nodeState)
        {
            Name = "MISSING: " + Name;

            missingState = nodeState;

            foreach (string input in nodeState.inputPorts)
                AddInput(null, null);


            foreach (string output in nodeState.outputPorts)
                AddOutput(null, null);
        }
    }
}
