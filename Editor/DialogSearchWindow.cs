using Fab.Dialog.Editor.Elements;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

namespace Fab.Dialog.Editor
{
    public class DialogSearchWindow : ScriptableObject, ISearchWindowProvider
    {
        private DialogGraphView graphView;
        private Texture2D indentationIcon; 
        public void Initialize(DialogGraphView graphView)
        {
            this.graphView = graphView;
            indentationIcon = new Texture2D(1, 1);
            indentationIcon.SetPixel(0, 0, Color.clear);
            indentationIcon.Apply();
        }

        public List<SearchTreeEntry> CreateSearchTree(SearchWindowContext context)
        {
            List<SearchTreeEntry> searchTreeEntries = new List<SearchTreeEntry>()
            {
                new SearchTreeGroupEntry(new GUIContent("Create Element")),
                new SearchTreeGroupEntry(new GUIContent("Dialog Node"), 1),
                new SearchTreeEntry(new GUIContent("Single Choice", indentationIcon))
                {
                    level = 2,
                    userData = DialogType.SingleChoice
                },
                new SearchTreeEntry(new GUIContent("Multi Choice", indentationIcon))
                {
                    level = 2,
                    userData = DialogType.MultiChoice
                },
                new SearchTreeGroupEntry(new GUIContent("Dialog Group"), 1),
                new SearchTreeEntry(new GUIContent("Single Group", indentationIcon))
                {
                    level = 2,
                    userData = new Group()
                },
            };
            return searchTreeEntries;
        }

        public bool OnSelectEntry(SearchTreeEntry SearchTreeEntry, SearchWindowContext context)
        {
            Vector2 localMousePosition = graphView.GetLocalMousePosition(context.screenMousePosition, true);

            switch (SearchTreeEntry.userData)
            {
                case DialogType.SingleChoice:
                    {
                        DialogChoiceNode node = graphView.CreateNode(DialogType.SingleChoice, localMousePosition);
                        graphView.AddElement(node);
                        return true;
                    }
                case DialogType.MultiChoice:
                    {
                        DialogChoiceNode node = graphView.CreateNode(DialogType.MultiChoice, localMousePosition);
                        graphView.AddElement(node);
                        return true;
                    }
                case Group _:
                    {
                        Group group = graphView.CreateGroup("New Group", localMousePosition);
                        graphView.AddElement(group);
                        return true;
                    }
                default:
                    return false;
            }
        }
    }
}
