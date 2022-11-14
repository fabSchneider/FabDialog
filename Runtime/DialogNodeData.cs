using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Fab.Dialog
{
    [Serializable]
    public class DialogNodeData 
    {
        [field: SerializeField]
        public string ID { get; set; }

        [field: SerializeField]
        public string Name { get; set; }
        
        [field: SerializeField]
        public string Text { get; set; }

        [field: SerializeField]
        public bool TextCollapsed { get; set; }

        [field: SerializeField]
        public List<DialogChoiceData> Choices { get; set; }
        
        [field: SerializeField]
        public DialogType DialogType { get; set; }
        
        [field: SerializeField]
        public Vector2 Position { get; set; }
    }
}
