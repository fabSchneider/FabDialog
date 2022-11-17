using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using Fab.Dialog.Editor.Elements;
using UnityEngine.UIElements;
using UnityEditor;

namespace Fab.Dialog.Editor
{
    public static class DialogGraphUtility
    {
        public static readonly string DialogIconPath = "Packages/com.fab.dialog/EditorResources/Icons/Dialog.png";

        public static void Save(DialogGraphView graphView, DialogGraphAsset graphAsset)
        {
            if (graphAsset == null)
            {
                return;
            }

            graphAsset.ViewPosition = graphView.viewTransform.position;
            graphAsset.ViewScale = graphView.viewTransform.scale;

            graphAsset.Nodes.Clear();
            graphAsset.Groups.Clear();

            Dictionary<Scope, DialogGroupData> groupDataByScope = new Dictionary<Scope, DialogGroupData>();

            // save groups
            graphView.Query<Group>().ForEach(group =>
            {
                DialogGroupData data = new DialogGroupData()
                {
                    Name = group.title,
                    Position = group.GetPosition().position,
                    NodeIDs = new List<string>()
                };

                groupDataByScope.Add(group, data);
                graphAsset.Groups.Add(data);
            });

            // save nodes
            graphView.Query<DialogChoiceNode>().ForEach(dialogNode =>
            {
                DialogNodeData data = dialogNode.ToNodeData();
                graphAsset.Nodes.Add(data);

                // add to group
                Scope scope = dialogNode.GetContainingScope();
                if (scope != null && groupDataByScope.TryGetValue(scope, out DialogGroupData groupData))
                {
                    groupData.NodeIDs.Add(dialogNode.ID);
                }
            });

            EditorUtility.SetDirty(graphAsset);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        public static void Load(DialogGraphView graphView, DialogGraphAsset graphAsset)
        {
            // remove all existing elements
            graphView.DeleteElements(graphView.graphElements.ToList());

            if (graphAsset == null)
                return;


            Dictionary<string, DialogChoiceNode> nodesById = new Dictionary<string, DialogChoiceNode>();


            // add nodes
            foreach (DialogNodeData nodeData in graphAsset.Nodes)
            {
                DialogChoiceNode node = graphView.CreateNode(nodeData);
                nodesById.Add(nodeData.ID, node);
                graphView.AddElement(node);
            }

            // connect nodes
            ConnectNodes(graphView, nodesById);

            // add groups
            foreach (DialogGroupData groupData in graphAsset.Groups)
            {
                Group group = graphView.CreateGroup(groupData);
                graphView.Add(group);
                foreach (string nodeID in groupData.NodeIDs)
                {
                    DialogChoiceNode node = nodesById[nodeID];
                    group.AddElement(node);
                }
            }

            // load view
            graphView.UpdateViewTransform(graphAsset.ViewPosition, graphAsset.ViewScale);
            graphView.ValidateViewTransform();
        }

        public static void ConnectNodes(DialogGraphView graphView, Dictionary<string, DialogChoiceNode> nodesById)
        {
            foreach (DialogChoiceNode node in nodesById.Values)
            {
                foreach (DialogChoiceData choice in node.Choices)
                {
                    for (int i = 0; i < choice.Paths.Count; i++)
                    {
                        WeightedPath transition = choice.Paths[i];

                        if (!string.IsNullOrEmpty(transition.TargetNodeID))
                        {
                            Port otherPort = nodesById[transition.TargetNodeID].inputContainer.Q<Port>();
                            Port port = node.outputContainer.Query<Port>().Where(port => ((DialogChoiceData)port.userData).ID == choice.ID).First();

                            WeightedEdge weightedEdge = port.ConnectTo<WeightedEdge>(otherPort);
                            weightedEdge.WeightedTransition = transition;
                            graphView.AddElement(weightedEdge);
                        }
                    }
                }
            }
        }
    }
}
