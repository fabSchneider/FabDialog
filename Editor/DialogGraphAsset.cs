using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Fab.Dialog.Editor
{
    [CreateAssetMenu(fileName = "NewDialogGraph.asset", menuName = "Fab/Dialog Graph")]
    public class DialogGraphAsset : DialogAsset
    {
        [field: SerializeField]
        [field: HideInInspector]
        public Vector3 ViewPosition { get; set; }
        
        [field: SerializeField]
        [field: HideInInspector]
        public Vector3 ViewScale { get; set; }

        [field: SerializeField]
        [field: NonReorderable]
        public List<DialogGroupData> Groups { get; set; } = new List<DialogGroupData>();
    }
}
