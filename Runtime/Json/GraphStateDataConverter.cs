using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;

namespace Fab.Dialog
{
    /// <summary>
    /// Custom json converter for node state data.
    /// </summary>
    public class GraphStateDataConverter : JsonConverter<GraphStateData>
    {
        public override void WriteJson(JsonWriter writer, GraphStateData value, JsonSerializer serializer)
        {
            writer.WriteStartObject();
            foreach (var item in value)
            {
                writer.WritePropertyName(item.Key);
                serializer.Serialize(writer, item.Value);
            }
            writer.WriteEndObject();
        }

        public override GraphStateData ReadJson(JsonReader reader, Type objectType, GraphStateData existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            reader.Read();
            while (reader.TokenType == JsonToken.PropertyName)
            {
                string propName = ((string)reader.Value);
                reader.Read();
                existingValue[propName] = ReadValue(reader, serializer);
            }
            return existingValue;
        }

        private object ReadValue(JsonReader reader, JsonSerializer serializer)
        {
            object value;   
            switch (reader.TokenType)
            {
                case JsonToken.StartObject:
                    value = ReadObject(reader, serializer);
                    break;
                case JsonToken.StartArray:
                    value = ReadArray(reader, serializer);
                    break;
                case JsonToken.Integer:
                case JsonToken.Float:
                case JsonToken.String:
                case JsonToken.Boolean:
                case JsonToken.Null:
                case JsonToken.Undefined:
                case JsonToken.Date:
                case JsonToken.Bytes:
                    value = reader.Value;
                    break;
                default:
                    value = null;
                    break;
            }

            reader.Read();
            return value;
        }

        private Dictionary<string, object> ReadObject(JsonReader reader, JsonSerializer serializer)
        {
            Dictionary<string, object> dict = new Dictionary<string, object>();

            reader.Read();
            while (reader.TokenType == JsonToken.PropertyName)
            {
                string propName = ((string)reader.Value);
                reader.Read();
                dict[propName] = ReadValue(reader, serializer);
            }
            return dict;
        }

        private object[] ReadArray(JsonReader reader, JsonSerializer serializer)
        {
            List<object> list = new List<object>();
            reader.Read();
            while (reader.TokenType != JsonToken.EndArray)
                list.Add(ReadValue(reader, serializer));

            return list.ToArray();
        }
    }
}
