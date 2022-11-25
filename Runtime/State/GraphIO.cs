using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace Fab.Dialog
{
    public static class GraphIO
    {
        private static JsonConverter[] converters;

        static GraphIO()
        {
            converters = new JsonConverter[]
            {
                new GraphStateDataConverter(),
                new Vector2Converter(),
                new Vector3Converter(),
                new Vector4Converter(),
                new RectConverter(),
            };
        }

        public static string SerializeGraphState(GraphState graphState)
        {
            return JsonConvert.SerializeObject(graphState, Formatting.Indented, converters);
        }

        public static GraphState DesrializeGraphState(string data)
        {
            return JsonConvert.DeserializeObject<GraphState>(data, converters);
        }
    }
}
