using UnityEngine;

namespace Fab.Dialog
{
    [CreateAssetMenu(fileName = "NewStateGraph.asset", menuName = "Fab/State Graph")]
    public class StateGraphAsset : ScriptableObject, ISerializationCallbackReceiver
    {
        public GraphState GraphState { get; set; } = new GraphState();

        [SerializeField]
        private string graphData;

        public void OnBeforeSerialize()
        {
            if(GraphState != null)
                graphData = GraphIO.SerializeGraphState(GraphState);
            else
                graphData = null;
        }

        public void OnAfterDeserialize()
        {
            if (graphData != null)
                GraphState = GraphIO.DesrializeGraphState(graphData);
            else
                GraphState = null;
        }
    }
}
