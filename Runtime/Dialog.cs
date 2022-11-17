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
        public DialogTransition transition;
    }
}
