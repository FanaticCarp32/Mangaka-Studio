using Mangaka_Studio.Interfaces;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mangaka_Studio.Models
{
    public class FrameLayerModelFactory
    {
        public List<FrameLayerModel> GenerateFromConfig(string config, float canvasWidth, float canvasHeight, ICanvasContext canvasContext)
        {
            var result = new List<FrameLayerModel>();
            var parsed = ParseConfig(config);
            int idCounter = 0;

            float totalRows = parsed.Count;
            float yOffset = 0;

            foreach (int columns in parsed)
            {
                float cellWidth = canvasWidth / columns;
                float cellHeight = canvasHeight / totalRows;
                float xOffset = 0;

                for (int i = 0; i < columns; i++)
                {
                    var rect = new SKRect(xOffset, yOffset, xOffset + cellWidth, yOffset + cellHeight);
                    var path = new SKPath();
                    path.AddRect(rect);

                    var model = new FrameModel
                    {
                        Id = idCounter,
                        Name = $"Frame {idCounter + 1}",
                        Bounds = path,
                        IsVisible = true
                    };

                    var frameLayer = new FrameLayerModel(model, canvasContext) 
                    {
                        IsDrawBounds = true,
                        BoundsWidth = 5,
                        IsSelected = false
                    };
                    result.Add(frameLayer);

                    idCounter++;
                    xOffset += cellWidth;
                }

                yOffset += cellHeight;
            }

            return result;
        }

        private List<int> ParseConfig(string config)
        {
            var rows = new List<int>();
            var parts = config.Split(new[] { '+', '/' }, StringSplitOptions.RemoveEmptyEntries);

            foreach (var part in parts)
            {
                var trimmed = part.Trim();
                var dims = trimmed.Split('x');

                if (dims.Length == 2 &&
                    int.TryParse(dims[0], out int cols) &&
                    int.TryParse(dims[1], out int rowCount))
                {
                    for (int i = 0; i < rowCount; i++)
                        rows.Add(cols);
                }
            }

            return rows;
        }
    }
}
