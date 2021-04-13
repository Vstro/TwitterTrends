using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

namespace TwitterTrends.Entities
{
    public class Coordinates
    {
        public double Lat { get; set; } = 0;
        public double Lon { get; set; } = 0;

        public Coordinates() { }
        /// <param name="textCoords">Coordinates as string in format "[lat, lon]"</param>
        public Coordinates(string textCoords)
        {
            int coordStart = textCoords.IndexOf('[');
            int coordSeparator = textCoords.IndexOf(',');
            int coordEnd = textCoords.IndexOf(']');
            this.Lon = double.Parse(textCoords[(coordStart + 1)..coordSeparator].Replace('.', ','));
            this.Lat = double.Parse(textCoords[(coordSeparator + 2)..coordEnd].Replace('.', ','));
        }

        public static double operator -(Coordinates c1, Coordinates c2)
        {
            return Math.Sqrt(Math.Pow(c1.Lat - c2.Lat, 2) + Math.Pow(c1.Lon - c2.Lon, 2));
        }

        public static implicit operator PointF(Coordinates coords)
        {
            return new PointF((float)coords.Lat, (float)coords.Lon);
        }
    }
}
