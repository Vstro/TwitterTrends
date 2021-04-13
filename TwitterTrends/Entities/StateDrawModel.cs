using System.Drawing;

namespace TwitterTrends.Entities
{
    public class StateDrawModel
    {
        public PointF[][] Polygons { get; set; }
        public PointF InnerPoint { get; set; }
        public string Name { get; set; }
        public Color Color { get; set; }
    }
}
