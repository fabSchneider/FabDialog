using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace Fab.Dialog
{
    public class StateNodeDescriptor
    {
        public readonly Guid identifier;
        public readonly string path;
        public readonly string displayName;
        public readonly Type type;

        public StateNodeDescriptor(Guid identifier, string path, string displayName, Type type)
        {
            this.identifier = identifier;
            this.path = path;
            this.displayName = displayName;
            this.type = type;
        }
    }

    public class StateNodeLibrary
    {
        private static Dictionary<Type, StateNodeDescriptor> nodeLibrary;
        public static IEnumerable<StateNodeDescriptor> Descriptors => nodeLibrary.Values;

        public static StateNodeDescriptor MissingNodeDescriptor = new StateNodeDescriptor(Guid.Empty, null, "Missing Node", typeof(MissingNode));

#if UNITY_EDITOR
        [UnityEditor.InitializeOnLoadMethod]

#else
        [RuntimeInitializeOnLoadMethod]
#endif
        static void Initialize()
        {
            nodeLibrary = new Dictionary<Type, StateNodeDescriptor>();
            AddAssembly(Assembly.GetAssembly(typeof(StateNodeLibrary)));
        }

        public static void AddAssembly(Assembly assembly)
        {
            foreach (Type type in GetStateNodeTypes(assembly))
            {
                StateNodeAttribute attr = type.GetCustomAttribute<StateNodeAttribute>();

                string nodePath = attr.Name;
                int splitIndex = nodePath.LastIndexOf('/');
                string name = nodePath.Substring(splitIndex + 1, nodePath.Length - splitIndex - 1);

                nodeLibrary.Add(type, new StateNodeDescriptor(attr.Identifier, nodePath, name, type));
            }
        }

        private static IEnumerable<Type> GetStateNodeTypes(Assembly assembly)
        {
            foreach (Type type in assembly.GetTypes())
            {
                if (type.IsSubclassOf(typeof(StateNode)) &&
                    type.GetCustomAttributes(typeof(StateNodeAttribute), true).Length > 0)
                {
                    yield return type;
                }
            }
        }

        /// <summary>
        /// Gets the node descriptor for the given id. Returns the missing node descriptor if no matching descriptor was found.
        /// </summary>
        /// <param name="identifier"></param>
        /// <returns></returns>
        public static StateNodeDescriptor GetDescriptor(Guid identifier)
        {
            foreach (StateNodeDescriptor descriptor in nodeLibrary.Values)
            {
                if (descriptor.identifier == identifier)
                    return descriptor;
            }
            return MissingNodeDescriptor;
        }

        /// <summary>
        /// Gets the node descriptor for the given type. Returns the missing node descriptor if no matching descriptor was found.
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static StateNodeDescriptor GetDescriptor(Type type)
        {
            if(nodeLibrary.TryGetValue(type, out StateNodeDescriptor descriptor))
                return descriptor;

            return MissingNodeDescriptor;
        }
    }
}