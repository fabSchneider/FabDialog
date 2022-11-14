using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Fab.Dialog
{
    public class Dialog
    {
        public string ID;
        public string text;
        public DialogChoice[] choices;
    }

    public class DialogChoice
    {
        public string text;
        public Dialog next;
    }

    public class DialogBehaviour : MonoBehaviour
    {
        [field: SerializeField]
        public DialogAsset DialogAsset { get; set; }

        [field: SerializeField]
        public string StartDialog { get; set; }

        public Dialog Dialog { get; set; }

        private Dialog nextDialog;
        public Rect rect = new Rect(50, 50, 600, 250);
        public float scale = 1f;


        private TypewritterEffect typeWritter;

        private void Start()
        {
            LoadDialog(StartDialog);

            nextDialog = Dialog;

            typeWritter = new TypewritterEffect(this);
            typeWritter.TextToDisplay = nextDialog.text;
            typeWritter.Start();
        }

        private void OnGUI()
        {
            GUI.matrix = GUI.matrix * Matrix4x4.Scale(Vector3.one * scale);
            GUILayout.BeginArea(rect);

            Dialog current = nextDialog;

            if (current == null)
            {
                GUILayout.Label("The Dialog has finished");
            }
            else
            {
                GUILayout.Label(typeWritter.CurrentText);
                foreach (var choice in current.choices)
                {
                    if (GUILayout.Button(choice.text))
                    {
                        nextDialog = choice.next;
                        if (nextDialog != null)
                        {
                            typeWritter.TextToDisplay = nextDialog.text;
                            typeWritter.Start();
                        }
                    }
                }
            }
            GUILayout.EndArea();
        }

        private void LoadDialog(string startDialog)
        {
            Dictionary<string, DialogNodeData> nodesByGuid = new Dictionary<string, DialogNodeData>();
            DialogNodeData startNode = null;
            foreach (DialogNodeData node in DialogAsset.Nodes)
            {
                nodesByGuid.Add(node.ID, node);
                if (node.Name == StartDialog)
                {
                    startNode = node;
                }
            }

            if(startNode != null)
            { 
                Dictionary<string, Dialog> dialogsByGuid = new Dictionary<string, Dialog>();
                Dialog = CreateDialog(startNode, nodesByGuid, dialogsByGuid);
            }
        }

        private Dialog CreateDialog(DialogNodeData node, Dictionary<string, DialogNodeData> nodesByGuid, Dictionary<string, Dialog> dialogsByGuid)
        {
            Dialog dialog = new Dialog()
            {
                ID = node.ID,
                text = node.Text
            };

            dialogsByGuid.Add(dialog.ID, dialog);

            DialogChoice[] choices = new DialogChoice[node.Choices.Count];

            for (int i = 0; i < choices.Length; i++)
            {
                DialogChoice choice = new DialogChoice()
                {
                    text = node.Choices[i].Text
                };

                string nextNodeID = node.Choices[i].NodeID;
                if (!string.IsNullOrEmpty(nextNodeID))
                {
                    DialogNodeData nextNode = nodesByGuid[node.Choices[i].NodeID];
                    Dialog next;
                    if (dialogsByGuid.TryGetValue(nextNode.ID, out next))
                    {
                        choice.next = next;
                    }
                    else
                    {
                        next = CreateDialog(nextNode, nodesByGuid, dialogsByGuid);
                        choice.next = next;

                    }
                }

                choices[i] = choice;
            }

            dialog.choices = choices;
            return dialog;
        }
    }
}
