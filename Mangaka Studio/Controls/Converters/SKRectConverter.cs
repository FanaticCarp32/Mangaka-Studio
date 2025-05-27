using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Text.Json;
using System.Threading.Tasks;

namespace Mangaka_Studio.Controls.Converters
{
    public class SKRectConverter : JsonConverter<SKRect>
    {
        public override SKRect Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            float left = 0, top = 0, right = 0, bottom = 0;

            if (reader.TokenType != JsonTokenType.StartObject)
                throw new JsonException();

            while (reader.Read())
            {
                if (reader.TokenType == JsonTokenType.EndObject)
                    break;

                string propertyName = reader.GetString();
                reader.Read();

                switch (propertyName)
                {
                    case "Left":
                        left = reader.GetSingle();
                        break;
                    case "Top":
                        top = reader.GetSingle();
                        break;
                    case "Right":
                        right = reader.GetSingle();
                        break;
                    case "Bottom":
                        bottom = reader.GetSingle();
                        break;
                }
            }

            return new SKRect(left, top, right, bottom);
        }

        public override void Write(Utf8JsonWriter writer, SKRect value, JsonSerializerOptions options)
        {
            writer.WriteStartObject();
            writer.WriteNumber("Left", value.Left);
            writer.WriteNumber("Top", value.Top);
            writer.WriteNumber("Right", value.Right);
            writer.WriteNumber("Bottom", value.Bottom);
            writer.WriteEndObject();
        }
    }
}
