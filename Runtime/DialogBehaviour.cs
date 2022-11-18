using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Fab.Dialog
{
    public class DialogBehaviour : MonoBehaviour
    {
        [field: SerializeField]
        public DialogAsset DialogAsset { get; set; }

        [field: SerializeField]
        public string StartDialog { get; set; }

        public Dialog Dialog { get; protected set; }

        private Dialog nextDialog;
        public Rect rect = new Rect(50, 50, 600, 250);
        public float scale = 1f;


        private TypewritterEffect typeWritter;

        private void Start()
        {
            DialogBuilder builder = new DialogBuilder(DialogAsset);

            Dialog = builder.BuildDialog(StartDialog);
            nextDialog = Dialog;

            typeWritter = new TypewritterEffect(this);
            typeWritter.TextToDisplay = nextDialog.Text;
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
                foreach (DialogChoice choice in current.choices)
                {
                    if (GUILayout.Button(choice.Text))
                    {
                        nextDialog = choice.transition.GetNext();

                        if (nextDialog != null)
                        {
                            typeWritter.TextToDisplay = nextDialog.Text;
                            typeWritter.Start();
                        }
                    }
                }
            }
            GUILayout.EndArea();
        }
    }
}
