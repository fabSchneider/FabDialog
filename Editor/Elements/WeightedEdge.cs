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

        private WeightedPath weightedTransition;

        public WeightedPath WeightedTransition
        {
            get { return weightedTransition; }
            set
            {
                if (weightedTransition == value)
                    return;

                if(weightedTransition != null)
                {
                    weightField.UnregisterValueChangedCallback(OnWeightValueChange);
                    weightField.value = 1f;
                }

                if(value != null)
                {
                    weightedTransition = value;
                    weightField.value = weightedTransition.Weight;
                    weightField.RegisterValueChangedCallback(OnWeightValueChange);
                }
            }
        }

        public float Weigth
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

        private void OnWeightValueChange(ChangeEvent<float> change)
        {
            weightedTransition.Weight = change.newValue;
        }
    }
}
