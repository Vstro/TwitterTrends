using System;
using System.Collections.Generic;
using System.Text;

namespace TwitterTrends.Entities
{
    public class Coordinates
    {
        public double Lat { get; set; } = 0;
        public double Lon { get; set; } = 0;

        public static double operator -(Coordinates c1, Coordinates c2)
        {
            return Math.Sqrt(Math.Pow(c1.Lat - c2.Lat, 2) + Math.Pow(c1.Lon - c2.Lon, 2));
        }

    }
}
