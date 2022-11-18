using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Fab.Dialog
{
    public class DialogAsset : ScriptableObject
    {
        [field: SerializeField]
        [field: NonReorderable]
        public List<DialogNodeData> Nodes { get; set; } = new List<DialogNodeData>();

        [field: SerializeField]
        [field: NonReorderable]
        public List<DialogEdgeData> Edges { get; set; } = new List<DialogEdgeData>();
    }
}
