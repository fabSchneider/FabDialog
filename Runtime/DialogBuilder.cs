using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Fab.Dialog
{
    public class DialogBuilder
    {
        public DialogAsset asset;

        Dictionary<string, DialogNodeData> nodesByGuid;
        Dictionary<string, Dialog> dialogsByGuid;

        public DialogBuilder(DialogAsset asset)
        {
            this.asset = asset;
            nodesByGuid = new Dictionary<string, DialogNodeData>();
            dialogsByGuid = new Dictionary<string, Dialog>();
        }

        public Dialog BuildDialog(string start)
        {
            DialogNodeData startNode = null;
            Dialog dialog = null;

            foreach (DialogNodeData node in asset.Nodes)
            {
                nodesByGuid.Add(node.ID, node);
                if (node.Name == start)
                {
                    startNode = node;
                }
            }

            if (startNode != null)
            {
                dialog = CreateDialog(startNode);
            }

            return dialog;
        }

        private Dialog CreateDialog(DialogNodeData nodeData)
        {
            // create the dialog object
            Dialog dialog = new Dialog()
            {
                ID = nodeData.ID,
                text = nodeData.Text
            };

            // add it to the created dialog dictionary 
            // to make sure we don't create it again later on
            dialogsByGuid.Add(dialog.ID, dialog);

            // create all available choices for the dialog
            DialogChoice[] choices = new DialogChoice[nodeData.Choices.Count];

            for (int i = 0; i < choices.Length; i++)
            {
                DialogChoiceData choiceData = nodeData.Choices[i];
                choices[i] = CreateChoice(choiceData);
            }

            dialog.choices = choices;

            return dialog;
        }

        private Dialog GetOrCreateDialog(string nodeID)
        {
            // get the node data from the nodeID
            if(!nodesByGuid.TryGetValue(nodeID, out DialogNodeData next))
            {
                // if no node exists for the node id exists, return null
                return null;
            }

            // recursively creates new dialogs unless the dialog has already been created
            if (!dialogsByGuid.TryGetValue(next.ID, out Dialog nextDialog))
                nextDialog = CreateDialog(next);

            return nextDialog;
        }

        private DialogChoice CreateChoice(DialogChoiceData choiceData)
        {
            // creates the dialog choice with the correct transitions
            // from a choiceData object


            // create transition
            DialogTransition transition = null;
            if(choiceData.Paths.Count == 0)
            {
                transition = new SimpleDialogTransition(null);
            }
            else if (choiceData.Paths.Count == 1)
            {
                // if only one path exists for the given choice
                // create a simple transition
                string nextNodeId = choiceData.Paths[0].TargetNodeID;

                Dialog next = GetOrCreateDialog(nextNodeId);
                transition = new SimpleDialogTransition(next);
            }
            else if (choiceData.Paths.Count > 1)
            {
                // if multiple paths exists for the given choice 
                // create a weighted transition

                Dialog[] choices = new Dialog[choiceData.Paths.Count];
                float[] weights = new float[choices.Length];

                for (int j = 0; j < choices.Length; j++)
                {
                    WeightedPath weightedPath = choiceData.Paths[j];
                    choices[j] = GetOrCreateDialog(weightedPath.TargetNodeID);
                    weights[j] = weightedPath.Weight;
                }

                transition = new WeightedDialogTransition(choices, weights);
            }

            DialogChoice choice = new DialogChoice()
            {
                text = choiceData.Text,
                transition = transition
            };

            return choice;
        }
    }
}
