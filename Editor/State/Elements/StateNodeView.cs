using System;
using System.Linq;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace Fab.Dialog.Editor
{
    public class StateNodeView : Node, ISerializable
    {
        protected TextField titleTextField;

        protected StateNode stateNode;
        public StateNode StateNode => stateNode;

        public StateGraphView owner;

        public StateNodeView() : base()
        {
            titleTextField = StateGraphGUI.CreateTextField(title, change =>
            {
                title = change.newValue;
                stateNode.Name = change.newValue;
            });

            titleTextField.AddToClassList(StateGraphGUI.textFieldClassname);
            titleTextField.AddToClassList(StateGraphGUI.nameTextFieldClassname);
            titleTextField.AddToClassList(StateGraphGUI.textFieldHiddenClassname);

            titleContainer.Q<Label>().RemoveFromHierarchy();
            titleContainer.Insert(0, titleTextField);
        }

        public virtual void Initialize(StateNode stateNode)
        {
            if (this.stateNode != null)
            {
                throw new InvalidOperationException("A node view may only be initialized once.");
            }

            if (stateNode == null)
                throw new ArgumentNullException(nameof(stateNode));

            this.stateNode = stateNode;
            titleTextField.value = StateNode.Name;

            // clear all data that might have been
            // created on prior initializations
            inputContainer.Clear();
            outputContainer.Clear();

            foreach (NodeParameter input in stateNode.Inputs)
                inputContainer.Add(CreateInputPort(input));

            foreach (NodeParameter output in stateNode.Outputs)
                outputContainer.Add(CreateOutputPort(output));

            stateNode.onAddedParameter += OnAddedParameter;
            stateNode.onRemovingParameter += OnRemovingParameter;

            stateNode.OnCreateGUI(this);
            RefreshExpandedState();
        }

        protected StatePort CreateInputPort(NodeParameter parameter)
        {
            // allow multiple inputs for execute ports
            StatePort inPort = StatePort.Create<Edge>(
                Orientation.Horizontal,
                Direction.Input,
                parameter.Type == null ? Port.Capacity.Multi : Port.Capacity.Single,
                parameter.Type);

            inPort.onConnect += OnPortConnect;
            inPort.onDisconnect += OnPortDisconnect;
            inPort.portName = parameter.Name;
            if (parameter.CanRename)
                inPort.ConfigureRenamablePort(parameter);
            return inPort;
        }

        protected StatePort CreateOutputPort(NodeParameter parameter)
        {
            StatePort outPort = StatePort.Create<Edge>(
                Orientation.Horizontal,
                Direction.Output,
                Port.Capacity.Multi,
                parameter.Type);
            outPort.onConnect -= OnPortConnect;
            outPort.onDisconnect -= OnPortDisconnect;
            outPort.portName = parameter.Name;
            if (parameter.CanRename)
                outPort.ConfigureRenamablePort(parameter);
            return outPort;
        }

        protected void OnPortConnect(StatePort port)
        {
            stateNode.NeedsResolve = true;
        }

        protected void OnPortDisconnect(StatePort port)
        {
            stateNode.NeedsResolve = true;
        }

        protected void OnAddedParameter(bool isInput, int index)
        {
            if (isInput)
            {
                StatePort port = CreateInputPort(stateNode.Inputs[index]);
                inputContainer.Insert(index, port);
            }
            else
            {
                StatePort port = CreateOutputPort(stateNode.Outputs[index]);
                outputContainer.Insert(index, port);
            }
        }

        protected void OnRemovingParameter(bool isInput, int index)
        {
            if (isInput)
            {
                StatePort port = (StatePort)inputContainer[index];
                owner.DeleteElements(port.connections);
                inputContainer.RemoveAt(index);
            }
            else
            {
                StatePort port = (StatePort)outputContainer[index];
                owner.DeleteElements(port.connections);
                outputContainer.RemoveAt(index);
            }
        }

        public virtual void Deserialize(GraphStateBase state)
        {
            NodeState nodeState = (NodeState)state;

            stateNode.Deserialize(nodeState);
            titleTextField.value = stateNode.Name;

            for (int i = 0; i < nodeState.inputPorts.Length; i++)
            {
                StatePort port = (StatePort)inputContainer[i];
                port.SetPortName(stateNode.Inputs[i].Name);
                port.viewDataKey = nodeState.inputPorts[i];
            }
            for (int i = 0; i < nodeState.outputPorts.Length; i++)
            {
                StatePort port = (StatePort)outputContainer[i];
                port.SetPortName(stateNode.Outputs[i].Name);
                port.viewDataKey = nodeState.outputPorts[i];
            }
        }

        public virtual void Serialize(GraphStateBase state)
        {
            stateNode.Serialize(state);
        }
    }
}
