using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Fab.Dialog
{
    public interface ITextProvider
    {
        string GetText();
    }

    public class PlainTextProvider : ITextProvider
    {
        public string text;

        public string GetText()
        {
           return text;
        }
    }

    public class ResolveTextProvider : ITextProvider
    {
        public List<PortData> inputs;
        public string rawText;
        public DialogAsset asset;
        public string GetText()
        {
            string text = rawText;
            foreach (PortData inputPort in inputs)
            {
                DialogEdgeData edge = asset.Edges.FirstOrDefault(e => e.Input == inputPort.Id);
                if (!string.IsNullOrEmpty(edge.Input))
                {
                    DialogNodeData node = asset.Nodes.First(n => n.Outputs.Any(p => p.Id == edge.Output));

                    text = text.Replace($"{{{inputPort.Name}}}", node.Text);
                }
            }
            return text;
        }

        public static ITextProvider CreateProvider(DialogNodeData nodeData, DialogAsset asset)
        {
            ResolveTextProvider textProvider = new ResolveTextProvider()
            {
                rawText = nodeData.Text,
                inputs = nodeData.Inputs.Skip(1).ToList(),
                asset = asset
            };
            return textProvider;
        }
    }



    public class Dialog
    {
        public string ID;
        public ITextProvider textProvider;
        public DialogChoice[] choices;

        public string Text => textProvider.GetText();
    }

    public class DialogChoice
    {
        public ITextProvider textProvider;
        public DialogTransition transition;

        public string Text => textProvider.GetText();
    }
}
