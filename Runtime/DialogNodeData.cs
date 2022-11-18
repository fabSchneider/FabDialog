using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Fab.Dialog
{
    [Serializable]
    public class DialogNodeData 
    {
        [field: SerializeField] public string Id { get; set; }
        [field: SerializeField] public string Name { get; set; }
        [field: SerializeField] public string Text { get; set; }
        [field: SerializeField] public DialogNodeType DialogType { get; set; }
        [field: SerializeField] public Vector2 Position { get; set; }
        [field: SerializeField] public bool TextCollapsed { get; set; }
        [field: SerializeField] public List<PortData> Inputs { get; set; }
        [field: SerializeField] public List<PortData> Outputs { get; set; }
    }

    [Serializable]
    public struct DialogEdgeData
    {
        [field: SerializeField] public string Input { get; set; }
        [field: SerializeField] public string Output { get; set; }
        [field: SerializeField] public float Weight { get; set; }
    }

    [Serializable]
    public struct PortData
    {
        [field: SerializeField] public string Id { get; set; }
        [field: SerializeField] public string Name { get; set; }
    }
}
