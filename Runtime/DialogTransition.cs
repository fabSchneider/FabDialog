using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Fab.Dialog
{
    public abstract class DialogTransition 
    {
        public abstract Dialog GetNext();
    }

    public class SimpleDialogTransition : DialogTransition
    {
        private Dialog next;

        public SimpleDialogTransition(Dialog next)
        {
            this.next = next;
        }

        public override Dialog GetNext()
        {
            return next;
        }
    }

    public class WeightedDialogTransition : DialogTransition
    {
        private Dialog[] choices;
        private float[] weights;
        private float weightSum;

        public WeightedDialogTransition(Dialog[] choices, float[] weights)
        {
            if (choices.Length != weights.Length)
                throw new System.Exception("Choices and weights need to be the same length.");

            this.choices = choices;
            this.weights = weights;

            weightSum = 0f;
            foreach (float weight in weights)
                weightSum += weight;
        }

        public override Dialog GetNext()
        {
            // make a weighted choice for the next dialog

            // pick next
            float pick = Random.Range(0f, weightSum);

            // TODO: make this more robust
            float currentWeight = 0f;
            for (int i = 0; i < choices.Length; i++)
            {
                currentWeight += weights[i];
                if (pick <= currentWeight)
                {
                    return choices[i];
                }
            }

            return null;
        }

    }
}
