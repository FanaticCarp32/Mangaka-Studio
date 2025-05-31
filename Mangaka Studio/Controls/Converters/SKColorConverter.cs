using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Mangaka_Studio.Controls.Converters
{
    public class SKColorConverter : JsonConverter<SKColor>
    {
        public override SKColor Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var jsonObject = JsonDocument.ParseValue(ref reader).RootElement;

            if (jsonObject.TryGetProperty("Alpha", out var alphaProperty) &&
                jsonObject.TryGetProperty("Red", out var redProperty) &&
                jsonObject.TryGetProperty("Green", out var greenProperty) &&
                jsonObject.TryGetProperty("Blue", out var blueProperty))
            {
                byte alpha = alphaProperty.GetByte();
                byte red = redProperty.GetByte();
                byte green = greenProperty.GetByte();
                byte blue = blueProperty.GetByte();

                return new SKColor(red, green, blue, alpha);
            }

            throw new JsonException("Invalid SKColor format.");
        }

        public override void Write(Utf8JsonWriter writer, SKColor value, JsonSerializerOptions options)
        {
            writer.WriteStartObject();
            writer.WriteNumber("Alpha", value.Alpha);
            writer.WriteNumber("Red", value.Red);
            writer.WriteNumber("Green", value.Green);
            writer.WriteNumber("Blue", value.Blue);
            writer.WriteEndObject();
        }
    }
}
