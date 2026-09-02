using Newtonsoft.Json;
using UnityEngine;
using System;

namespace Galactic1.Core
{
    public class Vector2Converter : JsonConverter
    {
        // Determines if this converter can convert the specified object type.
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(Vector2);
        }

        // Writes the JSON representation of the object.
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            Vector2 vector = (Vector2)value;
            writer.WriteStartObject();
            writer.WritePropertyName("x");
            writer.WriteValue(vector.x);
            writer.WritePropertyName("y");
            writer.WriteValue(vector.y);
            writer.WriteEndObject();
        }

        // Reads the JSON representation of the object.
        public override object ReadJson(JsonReader reader, Type objectType, object existingValue,
            JsonSerializer serializer)
        {
            // Ensure the reader is at the start of an object.
            if (reader.TokenType != JsonToken.StartObject)
            {
                throw new JsonSerializationException("Expected StartObject token when deserializing Vector2.");
            }

            float x = 0f;
            float y = 0f;

            // Read properties until the end of the object.
            while (reader.Read())
            {
                if (reader.TokenType == JsonToken.PropertyName)
                {
                    string propertyName = reader.Value.ToString();
                    reader.Read(); // Advance to the property value.

                    switch (propertyName.ToLowerInvariant())
                    {
                        case "x":
                            x = (float)Convert.ToDouble(reader.Value);
                            break;
                        case "y":
                            y = (float)Convert.ToDouble(reader.Value);
                            break;
                        default:
                            // Optionally handle unknown properties or ignore them.
                            break;
                    }
                }
                else if (reader.TokenType == JsonToken.EndObject)
                {
                    break; // End of object reached.
                }
            }

            return new Vector2(x, y);
        }
    }
}