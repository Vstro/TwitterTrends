using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

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
