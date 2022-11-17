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
            Paths = new List<WeightedPath>();
        }

        public DialogChoiceData(DialogChoiceData other)
        {
            ID = other.ID;
            Text = other.Text;
            Paths = new List<WeightedPath>(other.Paths.Count);
            for (int i = 0; i < other.Paths.Count; i++)
            {
                Paths.Add(new WeightedPath(other.Paths[i]));
            }     
        }

        [field: SerializeField]
        public string ID { get; set; }

        [field: SerializeField]
        public string Text { get; set; }

        [field: SerializeField]
        public List<WeightedPath> Paths { get; set; }
    }

    [Serializable]
    public class WeightedPath
    {
        [field: SerializeField]
        public string TargetNodeID { get; set; }

        [field: SerializeField]
        public float Weight { get; set; }

        public WeightedPath() { }

        public WeightedPath(WeightedPath other)
        {
            TargetNodeID = other.TargetNodeID;
            Weight = other.Weight;
        }
    }
}
