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
        public virtual StateNode Create(StateNodeDescriptor descriptor)
        {
            if (descriptor == null)
                throw new ArgumentNullException(nameof(descriptor), "Cannot create Node. Descriptor is null.");

            StateNode node = (StateNode)Activator.CreateInstance(descriptor.type);
            node.Name = descriptor.displayName;
            return node;
        }
    }
}