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
    public class SKPathConverter : JsonConverter<SKPath>
    {
        public override SKPath Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var pathString = reader.GetString();
            return SKPath.ParseSvgPathData(pathString);
        }

        public override void Write(Utf8JsonWriter writer, SKPath value, JsonSerializerOptions options)
        {
            var svg = value.ToSvgPathData();
            writer.WriteStringValue(svg);
        }
    }
}
