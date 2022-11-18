using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace Fab.Dialog.Editor.Elements
{
    public class WeightedEdge : Edge
    {
        private FloatField weightField;

        public float Weight
        {
            get => weightField.value;
            set => weightField.value = value;
        }

        public WeightedEdge() : base()
        {
            weightField = new FloatField();
            weightField.style.marginBottom = StyleKeyword.Auto;
            weightField.style.marginTop = StyleKeyword.Auto;
            weightField.style.marginLeft = StyleKeyword.Auto;
            weightField.style.marginRight = StyleKeyword.Auto;


            edgeControl.Add(weightField);
        }
    }
}
