using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Fab.Dialog
{
    public class DialogBuilder
    {
        public DialogAsset asset;

        Dictionary<string, DialogNodeData> nodesByGuid;
        Dictionary<string, DialogNodeData> nodesByPort;
        Dictionary<string, Dialog> dialogsByGuid;

        public DialogBuilder(DialogAsset asset)
        {
            this.asset = asset;
            nodesByGuid = new Dictionary<string, DialogNodeData>();
            nodesByPort = new Dictionary<string, DialogNodeData>();
            dialogsByGuid = new Dictionary<string, Dialog>();
        }

        public Dialog BuildDialog(string start)
        {
            DialogNodeData startNode = null;
            Dialog dialog = null;

            foreach (DialogNodeData node in asset.Nodes)
            {
                nodesByGuid.Add(node.Id, node);
                if (node.Name == start)
                {
                    startNode = node;
                }

                foreach (PortData port in node.Inputs)
                    nodesByPort.Add(port.Id, node);

                foreach (PortData port in node.Outputs)
                    nodesByPort.Add(port.Id, node);
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
                ID = nodeData.Id,
                textProvider = ResolveTextProvider.CreateProvider(nodeData, asset)
            };

            // add it to the created dialog dictionary 
            // to make sure we don't create it again later on
            dialogsByGuid.Add(dialog.ID, dialog);

            // create all available choices for the dialog
            DialogChoice[] choices = new DialogChoice[nodeData.Outputs.Count];

            for (int i = 0; i < choices.Length; i++)
            {
                PortData choicePort = nodeData.Outputs[i];
                choices[i] = CreateChoice(choicePort);
            }

            dialog.choices = choices;

            return dialog;
        }

        private Dialog GetOrCreateDialog(DialogNodeData node)
        {

            // recursively creates new dialogs unless the dialog has already been created
            if (!dialogsByGuid.TryGetValue(node.Id, out Dialog nextDialog))
                nextDialog = CreateDialog(node);

            return nextDialog;
        }

        private DialogChoice CreateChoice(PortData choicePort)
        {
            // creates the dialog choice with the correct transitions
            // from a choiceData object

            DialogEdgeData[] edges = asset.Edges.Where(e => e.Output == choicePort.Id).ToArray(); ;


            // create transition
            DialogTransition transition = null;
            if(edges.Length == 0)
            {
                transition = new SimpleDialogTransition(null);
            }
            else if (edges.Length == 1)
            {
                // if only one path exists for the given choice
                // create a simple transition
                string nextPortId = edges[0].Input;

                Dialog next = GetOrCreateDialog(nodesByPort[nextPortId]);
                transition = new SimpleDialogTransition(next);
            }
            else if (edges.Length > 1)
            {
                // if multiple paths exists for the given choice 
                // create a weighted transition

                Dialog[] choices = new Dialog[edges.Length];
                float[] weights = new float[choices.Length];

                for (int j = 0; j < choices.Length; j++)
                {
                    DialogEdgeData edge = edges[j];
                    string nextPortId = edge.Input;
                    choices[j] = GetOrCreateDialog(nodesByPort[nextPortId]);
                    weights[j] = edge.Weight;
                }

                transition = new WeightedDialogTransition(choices, weights);
            }

            DialogChoice choice = new DialogChoice()
            {
                textProvider = new PlainTextProvider() { text = choicePort.Name },
                transition = transition
            };

            return choice;
        }
    }
}
