using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Fab.Dialog
{
    public class StateEdge 
    {
        public StateEdge(NodeParameter output, NodeParameter input)
        {
            Output = output;
            Input = input;
        }
        public NodeParameter Output { get; private set; }
        public NodeParameter Input { get; private set; }
    }
}
