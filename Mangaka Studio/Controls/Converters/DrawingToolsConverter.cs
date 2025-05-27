using Mangaka_Studio.Controls.Tools;
using Mangaka_Studio.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Mangaka_Studio.Controls.Converters
{
    public class DrawingToolsConverter : JsonConverter<DrawingTools>
    {
        public override DrawingTools Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var jsonObject = JsonDocument.ParseValue(ref reader).RootElement;

            if (jsonObject.TryGetProperty("Type", out var typeProperty))
            {
                var typeName = typeProperty.GetString();
                var json = jsonObject.GetRawText();

                return typeName switch
                {
                    nameof(PenTool) => jsonObject.TryGetProperty("Settings", out var settingsProperty)
                        ? new PenTool(JsonSerializer.Deserialize<PenToolSettingsViewModel>(settingsProperty.GetRawText(), options))
                        : throw new JsonException("Settings property is missing."),
                    nameof(PipetteTool) => jsonObject.TryGetProperty("Settings", out var settingsProperty)
                        ? new PipetteTool(JsonSerializer.Deserialize<PenToolSettingsViewModel>(settingsProperty.GetRawText(), options))
                        : throw new JsonException("Settings property is missing."),
                    nameof(SoftEraser) => jsonObject.TryGetProperty("Settings", out var settingsProperty)
                        ? new SoftEraser(JsonSerializer.Deserialize<EraserSoftToolSettingsViewModel>(settingsProperty.GetRawText(), options))
                        : throw new JsonException("Settings property is missing."),
                    nameof(HardEraser) => jsonObject.TryGetProperty("Settings", out var settingsProperty)
                        ? new HardEraser(JsonSerializer.Deserialize<EraserHardToolSettingsViewModel>(settingsProperty.GetRawText(), options))
                        : throw new JsonException("Settings property is missing."),
                    nameof(TextTool) => jsonObject.TryGetProperty("Settings", out var settingsProperty)
                    ? new TextTool(JsonSerializer.Deserialize<TextToolSettingsViewModel>(settingsProperty.GetRawText(), options))
                    : throw new JsonException("Settings property is missing."),
                    _ => throw new NotSupportedException($"Unknown type: {typeName}")
                };
            }

            throw new JsonException("Type property is missing.");
        }

        public override void Write(Utf8JsonWriter writer, DrawingTools value, JsonSerializerOptions options)
        {
            var typeName = value.GetType().Name;

            writer.WriteStartObject();
            writer.WriteString("Type", typeName);

            foreach (var property in value.GetType().GetProperties())
            {
                if (property.Name != "Type")
                {
                    writer.WritePropertyName(property.Name);
                    JsonSerializer.Serialize(writer, property.GetValue(value), options);
                }
            }

            writer.WriteEndObject();
        }

    }
}
