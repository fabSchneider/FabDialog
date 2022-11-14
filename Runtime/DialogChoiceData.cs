using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Fab.Dialog
{
    [Serializable]
    public class DialogChoiceData
    {
        public DialogChoiceData(string text)
        {
            ID = Guid.NewGuid().ToString();
            Text = text;
        }

        public DialogChoiceData(DialogChoiceData other)
        {
            ID = other.ID;
            Text = other.Text;
            NodeID = other.NodeID;
        }

        [field: SerializeField]
        public string ID { get; set; }

        [field: SerializeField]
        public string Text { get; set; }

        [field: SerializeField]
        public string NodeID { get; set; }
    }
}
