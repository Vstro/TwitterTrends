using System;

namespace TwitterTrends.Entities
{
    public class WeightedCenter
    {
        public Coordinates Center { get; set; } = new Coordinates();
        public double Area { get; set; }
    }
}
