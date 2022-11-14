using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Fab.Dialog.Editor
{
    [Serializable]
    public class DialogGroupData
    {
        [field: SerializeField]
        public string Name { get; set; }

        [field: SerializeField]
        public Vector2 Position { get; set; }

        [field: SerializeField]
        public List<string> NodeIDs { get; set; }
    }
}
