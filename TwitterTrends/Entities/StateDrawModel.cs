using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

namespace TwitterTrends.Entities
{
    public class StateDrawModel
    {
        public PointF[][] Polygons { get; set; }
        public Coordinates InnerPoint { get; set; }
        public string Code { get; set; }
    }
}
