using System;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace Fab.Dialog.Editor
{
    public class StateNodeViewFactory
    {
        public virtual StateNodeView Create(StateNodeDescriptor descriptor)
        {
            if (descriptor == null)
                throw new ArgumentNullException(nameof(descriptor), "Cannot create Node. Descriptor is null.");

            StateNode node = (StateNode)Activator.CreateInstance(descriptor.type);
            node.Name = descriptor.displayName;
            StateNodeView nodeView = new StateNodeView();
            nodeView.Initialize(node);

            return nodeView;
        }
    }
}