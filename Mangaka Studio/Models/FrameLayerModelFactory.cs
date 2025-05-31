using Mangaka_Studio.Interfaces;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;

namespace Mangaka_Studio.Models
{
    public class FrameLayerModelFactory
    {
        private int CanvasPadding = 10;

        public List<FrameLayerModel> GenerateFromConfig(string config, float canvasWidth, float canvasHeight, ICanvasContext canvasContext)
        {
            try
            {
                var parsed = ParseConfig(config);
                var result = new List<FrameLayerModel>();
                int idCounter = 0;
                CanvasPadding = (int)((0.1 * canvasWidth + 0.1 * canvasHeight) / 2);
                float usableWidth = canvasWidth - CanvasPadding * 2;
                float usableHeight = canvasHeight - CanvasPadding * 2;

                GenerateRecursive(parsed, CanvasPadding, CanvasPadding, usableWidth, usableHeight, canvasContext, result, ref idCounter);
                return result;
            }
            catch
            {
                MessageBox.Show("Неверный формат данных");
                return null;
            }
        }

        private void GenerateRecursive(FrameConfig config, float startX, float startY, float totalWidth, float totalHeight, ICanvasContext canvasContext, List<FrameLayerModel> result, ref int idCounter)
        {
            float spacing = config.Spacing;
            float cellWidth = (totalWidth - (config.Columns - 1) * spacing) / config.Columns;
            float cellHeight = (totalHeight - (config.Rows - 1) * spacing) / config.Rows;

            for (int row = 0; row < config.Rows; row++)
            {
                for (int col = 0; col < config.Columns; col++)
                {
                    float x = startX + col * (cellWidth + spacing);
                    float y = startY + row * (cellHeight + spacing);

                    if (config.Children != null && config.Children.Count > row * config.Columns + col && config.Children[row * config.Columns + col] != null)
                    {
                        GenerateRecursive(config.Children[row * config.Columns + col], x, y, cellWidth, cellHeight, canvasContext, result, ref idCounter);
                    }
                    else
                    {
                        var rect = new SKRect(x, y, x + cellWidth, y + cellHeight);
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
                            BoundsWidth = config.BorderWidth,
                            IsSelected = false
                        };

                        result.Add(frameLayer);
                        idCounter++;
                    }
                }
            }
        }

        public FrameConfig ParseConfig(string config)
        {
            config = config.Replace(" ", "");
            return ParseSection(config);
        }

        private FrameConfig ParseSection(string section)
        {
            var outerMatch = Regex.Match(section, @"^(\d+)x(\d+)(?::(\d+))?(?::(\d+))?(\[(.*)\])?");
            if (!outerMatch.Success) throw new FormatException("Invalid section format");

            var cfg = new FrameConfig
            {
                Columns = int.Parse(outerMatch.Groups[1].Value),
                Rows = int.Parse(outerMatch.Groups[2].Value),
                Spacing = outerMatch.Groups[3].Success ? int.Parse(outerMatch.Groups[3].Value) : 0,
                BorderWidth = outerMatch.Groups[4].Success ? int.Parse(outerMatch.Groups[4].Value) : 2
            };

            if (outerMatch.Groups[6].Success)
            {
                string inner = outerMatch.Groups[6].Value;
                var parts = SplitTopLevel(inner);
                cfg.Children = parts.Select(p => p == "-" ? null : ParseSection(p)).ToList();
            }

            return cfg;
        }

        private List<string> SplitTopLevel(string input)
        {
            var result = new List<string>();
            int depth = 0;
            int lastSplit = 0;

            for (int i = 0; i < input.Length; i++)
            {
                if (input[i] == '[') depth++;
                else if (input[i] == ']') depth--;
                else if (input[i] == ',' && depth == 0)
                {
                    result.Add(input.Substring(lastSplit, i - lastSplit));
                    lastSplit = i + 1;
                }
            }

            if (lastSplit < input.Length)
                result.Add(input.Substring(lastSplit));

            return result;
        }
    }
}
