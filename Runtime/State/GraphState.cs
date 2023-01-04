using System;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

namespace Fab.Dialog
{
    public interface ISerializable
    {
        void Serialize(GraphStateBase state);
        void Deserialize(GraphStateBase state);
    }


    /// <summary>
    /// Holds generic state data identifiable by a key.
    /// </summary>
    [Serializable]
    public class GraphStateData : Dictionary<string, object> { }


    public abstract class GraphStateBase
    {
        public GraphStateData data = new GraphStateData();

        /// <summary>
        /// Returns true if the state contains an entry with the given key.
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public bool Contains(string key)
        {
            return data.ContainsKey(key);
        }

        /// <summary>
        /// Adds a value with the given key to the state.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public void Add<T>(string key, T value)
        {
            data.Add(key, value);
        }

        /// <summary>
        /// Returns the value for the given key as a boolean.
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public bool GetBool(string key)
        {
            return Convert.ToBoolean(data[key]);
        }

        /// <summary>
        /// Returns the value for the given key as an integer.
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public int GetInt(string key)
        {
            return Convert.ToInt32(data[key]);
        }

        /// <summary>
        /// Returns the value for the given key as a float.
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public float GetFloat(string key)
        {
            return Convert.ToSingle(data[key]);
        }

        /// <summary>
        /// Returns the value for the given key as a string.
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public string GetString(string key)
        {
            object val = data[key];

            if (val == null)
                return null;

            return Convert.ToString(val);
        }

        /// <summary>
        /// Returns the value for the given key as a Vector3.
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public Vector2 GetVector2(string key)
        {
            object obj = data[key];
            if (obj is Vector2 vec2)
                return vec2;

            Dictionary<string, object> objDict = (Dictionary<string, object>)data[key];

            return new Vector2(
                Convert.ToSingle(objDict["x"]),
                Convert.ToSingle(objDict["y"]));
        }

        /// <summary>
        /// Returns the value for the given key as a Vector3.
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public Vector3 GetVector3(string key)
        {
            object obj = data[key];
            if (obj is Vector3 vec3)
                return vec3;

            Dictionary<string, object> objDict = (Dictionary<string, object>)obj;

            return new Vector3(
                Convert.ToSingle(objDict["x"]),
                Convert.ToSingle(objDict["y"]),
                Convert.ToSingle(objDict["z"]));
        }

        /// <summary>
        /// Returns the value for the given key as a Vector3.
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public Vector4 GetVector4(string key)
        {
            object obj = data[key];
            if (obj is Vector4 vec4)
                return vec4;

            Dictionary<string, object> objDict = (Dictionary<string, object>)data[key];

            return new Vector4(
                Convert.ToSingle(objDict["x"]),
                Convert.ToSingle(objDict["y"]),
                Convert.ToSingle(objDict["z"]),
                Convert.ToSingle(objDict["w"]));
        }


        /// <summary>
        /// Returns the value for the given key as a boolean array.
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public bool[] GetBoolArray(string key)
        {
            return Array.ConvertAll((object[])data[key], x => Convert.ToBoolean(x));
        }

        /// <summary>
        /// Returns the value for the given key as an integer array.
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public int[] GetIntArray(string key)
        {
            return Array.ConvertAll((object[])data[key], x => Convert.ToInt32(x));
        }

        /// <summary>
        /// Returns the value for the given key as a float array.
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public float[] GetFloatArray(string key)
        {
            object obj = data[key];
            if (obj == null)
                return null;

            if (obj is ICollection<float> collection)
            {
                float[] copy = new float[collection.Count];
                collection.CopyTo(copy, 0);
                return copy;
            }

            return Array.ConvertAll((object[])data[key], x =>
            {
                if (x == null)
                    return default(float);
                return Convert.ToSingle(x);
            });
        }

        /// <summary>
        /// Returns the value for the given key as a string array.
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public string[] GetStringArray(string key)
        {
            object obj = data[key];
            if (obj == null)
                return null;

            if (obj is ICollection<string> collection)
            {
                string[] copy = new string[collection.Count];
                collection.CopyTo(copy, 0);
                return copy;
            }

            return Array.ConvertAll((object[])data[key], x =>
            {
                if (x == null)
                    return default(string);
                return Convert.ToString(x);
            });
        }
    }

    [Serializable]
    public class GraphState : GraphStateBase
    {
        public List<NodeState> nodes = new List<NodeState>();
        public List<EdgeState> edges = new List<EdgeState>();
    }



    [Serializable]
    public class EdgeState : GraphStateBase
    {
        public string type;
        public string guid;
        public string output;
        public string input;
        public EdgeState() { }
    }



    /// <summary>
    /// Holds the state of a game object
    /// </summary>
    [Serializable]
    public class NodeState : GraphStateBase
    {
        public string type;
        public string guid;
        public string title;

        public float xPos;
        public float yPos;

        public string[] inputPorts;
        public string[] outputPorts;
        public NodeState() { }
    }
}
