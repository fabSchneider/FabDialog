using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

namespace Fab.Dialog.Editor
{
    public class PropertyNodeView<T> : StateNodeView
    {
        public PropertyNodeView() : base()
        {
            Port outPick = Port.Create<Edge>(Orientation.Horizontal, Direction.Output, Port.Capacity.Multi, typeof(T));
            outputContainer.Add(outPick);
        }
    }
}
