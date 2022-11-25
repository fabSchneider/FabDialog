using System;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace Fab.Dialog.Editor
{

    public class StatePort : Port
    {
        private class DefaultEdgeConnectorListener : IEdgeConnectorListener
        {
            private GraphViewChange m_GraphViewChange;
            private List<Edge> m_EdgesToCreate;
            private List<GraphElement> m_EdgesToDelete;

            public DefaultEdgeConnectorListener()
            {
                this.m_EdgesToCreate = new List<Edge>();
                this.m_EdgesToDelete = new List<GraphElement>();
                this.m_GraphViewChange.edgesToCreate = this.m_EdgesToCreate;
            }

            public void OnDropOutsidePort(Edge edge, Vector2 position)
            {
            }

            public void OnDrop(GraphView graphView, Edge edge)
            {
                this.m_EdgesToCreate.Clear();
                this.m_EdgesToCreate.Add(edge);
                this.m_EdgesToDelete.Clear();
                if (edge.input.capacity == Port.Capacity.Single)
                {
                    foreach (Edge connection in edge.input.connections)
                    {
                        if (connection != edge)
                            this.m_EdgesToDelete.Add((GraphElement)connection);
                    }
                }
                if (edge.output.capacity == Port.Capacity.Single)
                {
                    foreach (Edge connection in edge.output.connections)
                    {
                        if (connection != edge)
                            this.m_EdgesToDelete.Add((GraphElement)connection);
                    }
                }
                if (this.m_EdgesToDelete.Count > 0)
                    graphView.DeleteElements((IEnumerable<GraphElement>)this.m_EdgesToDelete);
                List<Edge> edgesToCreate = this.m_EdgesToCreate;
                if (graphView.graphViewChanged != null)
                    edgesToCreate = graphView.graphViewChanged(this.m_GraphViewChange).edgesToCreate;
                foreach (Edge edge1 in edgesToCreate)
                {
                    graphView.AddElement((GraphElement)edge1);
                    edge.input.Connect(edge1);
                    edge.output.Connect(edge1);
                }
            }
        }

        public event Action<StatePort> onConnect;
        public event Action<StatePort> onDisconnect;

        private Type defaultPortType;
        public Type DefaultPortType => defaultPortType;

        protected StatePort(Orientation portOrientation, Direction portDirection, Capacity portCapacity, Type type) : base(portOrientation, portDirection, portCapacity, type)
        {
            defaultPortType = portType;

            if(portType == null)
            {
                AddToClassList("execute-port");
            }
        }

        public void SetPortName(string name)
        {

            TextField renameTextField = this.Q<TextField>();
            if (renameTextField != null)
                renameTextField.value = name;
            else
                portName = name;
        }

        public override void Connect(Edge edge)
        {
            base.Connect(edge);
            onConnect?.Invoke(this);
        }

        public override void Disconnect(Edge edge)
        {
            base.Disconnect(edge);
            onDisconnect?.Invoke(this);
        }

        public override void DisconnectAll()
        {
            base.DisconnectAll();
            onDisconnect?.Invoke(this);
        }

        public new static StatePort Create<TEdge>(
            Orientation orientation,
            Direction direction,
            Port.Capacity capacity,
            System.Type type)
            where TEdge : Edge, new()
        {
            var listener = new StatePort.DefaultEdgeConnectorListener();
            var ele = new StatePort(orientation, direction, capacity, type)
            {
                m_EdgeConnector = (EdgeConnector)new EdgeConnector<TEdge>((IEdgeConnectorListener)listener)
            };
            ele.AddManipulator((IManipulator)ele.m_EdgeConnector);
            return ele;
        }

        public void ConfigureRenamablePort(NodeParameter parameter)
        {
            m_ConnectorText.style.display = DisplayStyle.None;
           
            TextField textField = new TextField();
            textField.value = parameter.Name;
            textField.RegisterValueChangedCallback(change =>
            {
                parameter.Name = change.newValue;
                m_ConnectorText.text = change.newValue;
            });

            textField.AddToClassList("dialog-node__text-field");
            textField.AddToClassList("dialog-node__text-field--hidden");
            textField.AddToClassList("dialog-node__choice-text-field");
            Add(textField);
        }
    }
}
