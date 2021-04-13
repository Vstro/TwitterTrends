using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using TwitterTrends.Entities;

namespace TwitterTrends.Services
{
    public static class MapService
    {
        public static Size CanvasSize { get; set; }
        public static double XOffset { get; set; }
        public static double YOffset { get; set; }
        public static double Scale { get; set; } = 0;

        public static List<StateDrawModel> GetStatesDrawModels(Size canvasSize)
        {
            var statesDrawModels = new List<StateDrawModel>();
            var statesCenters = StateService.GetStatesCenters();
            var statesSentiments = StateService.CalculateAverageSentiments();
            var statesCoords = StateService.GetStatesCoordinates();
            CalculateCanvasParams(canvasSize, statesCoords
                .Select(pair => pair.Value)
                .Aggregate((state1, state2) => state1.Concat(state2).ToArray())
                .Aggregate((poly1, poly2) => poly1.Concat(poly2).ToArray())
            );
            foreach (var state in statesCoords)
            {
                statesDrawModels.Add(new StateDrawModel 
                { 
                    Name = state.Key,
                    Polygons = state.Value.Select(
                        poly => poly.Select(
                            c => ConvertToCanvasPoint(c)
                        ).ToArray()
                    ).ToArray(),
                    InnerPoint = ConvertToCanvasPoint(statesCenters[state.Key]),
                    Color = GetSentimentColor(
                        statesSentiments.ContainsKey(state.Key) ? 
                        statesSentiments[state.Key] : new double?()
                    )
                });
            }
            return statesDrawModels;
        }

        private static Color GetSentimentColor(double? sentiment)
        {
            int diversity = 5;
            if (!sentiment.HasValue) return Color.Gray;
            int greenShape;
            if (Math.Abs(sentiment.Value) <= 0.004)
            {
                return Color.White;
            }
            if (sentiment.Value < 0)
            {
                greenShape = (int)(255 * (1 + diversity * sentiment.Value));
                if (greenShape < 0) greenShape = 0;
                return Color.FromArgb(0, greenShape, 255);
            }
            greenShape = (int)(255 * (1 - diversity * sentiment.Value));
            if (greenShape < 0) greenShape = 0;
            return Color.FromArgb(255, greenShape, 0);
        }

        private static void CalculateCanvasParams(Size canvasSize, Coordinates[] points)
        {
            CanvasSize = canvasSize;
            points = points.Where(c => c.Lon < 0).ToArray();
            XOffset = points.Select(c => c.Lon).Min();
            YOffset = points.Select(c => c.Lat).Min();
            double xScale = (canvasSize.Width - 50) / (points.Select(c => c.Lon).Max() - XOffset);
            double yScale = (canvasSize.Height - 50) / (points.Select(c => c.Lat).Max() - YOffset);
            Scale = Math.Min(xScale, yScale);
        }

        private static PointF ConvertToCanvasPoint(Coordinates coords)
        {
            if (Scale == 0) return new PointF((float)coords.Lat, (float)coords.Lon);
            return new PointF
            {
                X = (float)((coords.Lon - XOffset) * Scale),
                // Invert axis Y to not turn map upside-down
                Y = CanvasSize.Height - 150 - (float)((coords.Lat - YOffset) * Scale)
            };
        }
    }
}
