using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using Fab.Dialog.Editor.Elements;
using UnityEngine.UIElements;
using UnityEditor;
using UnityEditor.UIElements;
using System.Linq;

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
            graphAsset.Edges.Clear();
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
            graphView.Query<DialogNode>().ForEach(dialogNode =>
            {
                DialogNodeData data = dialogNode.Serialize();
                graphAsset.Nodes.Add(data);

                // add to group
                Scope scope = dialogNode.GetContainingScope();
                if (scope != null && groupDataByScope.TryGetValue(scope, out DialogGroupData groupData))
                {
                    groupData.NodeIDs.Add(dialogNode.ID);
                }
            });

            // save edges
            graphView.Query<Edge>().ForEach(edge =>
            {
                graphAsset.Edges.Add(new DialogEdgeData()
                {
                    Input = edge.input.viewDataKey,
                    Output = edge.output.viewDataKey,
                    Weight = edge.userData != null ? (float)edge.userData : 1f
                });
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

            // add nodes
            foreach (DialogNodeData nodeData in graphAsset.Nodes)
            {
                DialogNode node = graphView.CreateNode(nodeData);
                graphView.AddElement(node);
            }

            // connect nodes
            ConnectNodes(graphView, graphAsset.Edges);

            // add groups
            foreach (DialogGroupData groupData in graphAsset.Groups)
            {
                Group group = graphView.CreateGroup(groupData);
                graphView.Add(group);
                foreach (string nodeID in groupData.NodeIDs)
                {
                    Node node = graphView.GetNodeByGuid(nodeID);
                    group.AddElement(node);
                }
            }

            // load view
            graphView.UpdateViewTransform(graphAsset.ViewPosition, graphAsset.ViewScale);
            graphView.ValidateViewTransform();
        }

        public static void ConnectNodes(DialogGraphView graphView, List<DialogEdgeData> edges)
        {
            foreach (DialogEdgeData edge in edges)
            {
                Port input = graphView.GetPortByGuid(edge.Input);
                Port output = graphView.GetPortByGuid(edge.Output);

                Edge e = input.ConnectTo(output);

                //created weighted edge
                if (input.portType == null)
                    MakeWeightedEdge(e, edge.Weight);

                graphView.FlagRefresh(e);
                graphView.FlagRefresh(e.input);
                graphView.AddElement(e);
            }
        }

        public static void MakeWeightedEdge(Edge e, float weight)
        {
            e.userData = weight;
            FloatField weightField = new FloatField();
            weightField.name = "edge__weight-field";
            weightField.value = weight;
            weightField.style.marginBottom = StyleKeyword.Auto;
            weightField.style.marginTop = StyleKeyword.Auto;
            weightField.style.marginLeft = StyleKeyword.Auto;
            weightField.style.marginRight = StyleKeyword.Auto;
            weightField.RegisterValueChangedCallback(change => e.userData = change.newValue);
            e.edgeControl.Add(weightField);
        }
    }
}

