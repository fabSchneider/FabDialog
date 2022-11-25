using System.Collections;
using UnityEngine;

namespace Fab.Dialog
{
    public abstract class ExecuteNode : StateNode
    {
        public abstract NodeParameter Execute();

        public void AddExecuteInput(string name, bool canRename = false)
        {
            AddInput(null, name, canRename);
        }

        public void AddExecuteOutput(string name, bool canRename = false)
        {
            AddOutput(null, name, canRename);
        }
    }
}