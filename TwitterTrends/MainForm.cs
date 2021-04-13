using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using TwitterTrends.Services;

namespace TwitterTrends
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();
        }

        private void MainForm_Paint(object sender, PaintEventArgs e)
        {
            e.Graphics.Clear(Color.White);
            var pen = new Pen(Color.Black)
            {
                Width = 3
            };
            var drawModels = MapService.GetStatesDrawModels(this.Size);
            foreach (var stateDrawModel in drawModels)
            {
                foreach (var polygon in stateDrawModel.Polygons)
                {
                    e.Graphics.DrawPolygon(pen, polygon);
                    e.Graphics.FillPolygon(new SolidBrush(stateDrawModel.Color), polygon);
                }               
            }
            foreach (var stateDrawModel in drawModels)
            {
                e.Graphics.DrawString(
                    stateDrawModel.Name,
                    new Font(FontFamily.GenericSansSerif, 8),
                    new SolidBrush(Color.Black),
                    new PointF(stateDrawModel.InnerPoint.X - 10, stateDrawModel.InnerPoint.Y - 10)
                );
            }
        }
    }
}
