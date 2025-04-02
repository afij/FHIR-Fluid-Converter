using Fhir.Fluid.Converter.CustomRegex;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace Fhir.Fluid.Converter.Converters
{
    /// <summary>
    /// One-way JsonConverter to deserialize XML-converted JSON string to IDictionary
    /// </summary>
    internal class DictionaryJsonConverter : JsonConverter
    {
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            return ReadValue(reader);
        }

        private object ReadValue(JsonReader reader)
        {
            while (reader.TokenType == JsonToken.Comment)
            {
                if (!reader.Read())
                {
                    throw new Exception();
                }
            }

            switch (reader.TokenType)
            {
                case JsonToken.StartObject:
                    return ReadObject(reader);
                case JsonToken.StartArray:
                    return ReadArray(reader);
                case JsonToken.String:
                    // Remove line breaks to avoid invalid line breaks in json value
                    // A line break is a normal character in XML but invalid in JSON
                    return CCDRegex.InvalidLineBreakRegex().Replace(reader.Value.ToString(), string.Empty);
                case JsonToken.Integer:
                case JsonToken.Float:
                case JsonToken.Boolean:
                case JsonToken.Undefined:
                case JsonToken.Null:
                case JsonToken.Date:
                case JsonToken.Bytes:
                    return reader.Value;
                default:
                    throw new Exception();
            }
        }

        private List<object> ReadArray(JsonReader reader)
        {
            List<object> list = new();

            while (reader.Read())
            {
                switch (reader.TokenType)
                {
                    case JsonToken.Comment:
                        break;
                    case JsonToken.EndArray:
                        return list;
                    default:
                        var v = ReadValue(reader);
                        list.Add(v);
                        break;
                }
            }

            throw new Exception();
        }

        private Dictionary<string, object> ReadObject(JsonReader reader)
        {
            var obj = new Dictionary<string, object>();

            while (reader.Read())
            {
                switch (reader.TokenType)
                {
                    case JsonToken.PropertyName:
                        var propertyName = reader.Value.ToString();

                        if (!reader.Read())
                        {
                            throw new Exception();
                        }

                        // Remove "@" if it is attribute
                        if (propertyName.StartsWith('@'))
                        {
                            propertyName = propertyName[1..];
                        }

                        var v = ReadValue(reader);
                        obj[propertyName] = v;
                        break;
                    case JsonToken.Comment:
                        break;
                    case JsonToken.EndObject:
                        return obj;
                }
            }

            throw new Exception();
        }

        public override bool CanConvert(Type objectType)
        {
            return typeof(IDictionary<string, object>).IsAssignableFrom(objectType);
        }
    }
}
