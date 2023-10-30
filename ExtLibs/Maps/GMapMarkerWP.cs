using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using GMap.NET;
using GMap.NET.WindowsForms;
using GMap.NET.WindowsForms.Markers;

namespace MissionPlanner.Maps
{
    [Serializable]
    public class GMapMarkerWP : GMarkerGoogle
    {
        string wpno = "";
        bool showToolTip = false;
        public bool selected = false;
        SizeF txtsize = SizeF.Empty;
        static Dictionary<string, Bitmap> fontBitmaps = new Dictionary<string, Bitmap>();
        static Font font;

        public GMapMarkerWP(PointLatLng p, string wpno)
            : base(p, Resources.markerarrow2)
        {
            this.wpno = wpno;
            ToolTipText = "Waypoint Number: "+ wpno;
            if (font == null)
                font = SystemFonts.DefaultFont;

            if (!fontBitmaps.ContainsKey(wpno))
            {
                Bitmap temp = new Bitmap(100,40, PixelFormat.Format32bppArgb);
                using (Graphics g = Graphics.FromImage(temp))
                {
                    g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                    g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
                    txtsize = g.MeasureString(wpno, font);

                    g.DrawString(wpno, font, Brushes.Black, new PointF(0, 0));
                }
                fontBitmaps[wpno] = temp;
            }
        }
        //public override string ToolTipText => string.Empty;
        //public void ShowToolTip(bool show)
        //{
        //    showToolTip = show;
        //    if (Overlay != null)
        //    {
        //        Overlay.Control.Invalidate();
        //    }
        //}
        public override void OnRender(IGraphics g)
        {
            //g.FillEllipse(Brushes.Gray, new Rectangle(this.LocalPosition.X + 3, this.LocalPosition.Y + 3, this.Size.Width, this.Size.Height));
            //g.DrawArc(Pens.Gray, new Rectangle(this.LocalPosition.X + 3, this.LocalPosition.Y + 3, this.Size.Width, this.Size.Height), 0, 360);

            if (selected)
            {
                g.FillEllipse(Brushes.Red, new Rectangle(this.LocalPosition, this.Size));
                g.DrawArc(Pens.Red, new Rectangle(this.LocalPosition, this.Size), 0, 360);
            }
            
            base.OnRender(g);

            var midw = LocalPosition.X + 20;
            var midh = LocalPosition.Y - 12;

            var txtsize = g.MeasureString(wpno, SystemFonts.DefaultFont);
            if (txtsize.Width > 15)
                midw -= 4;
            Font boldF = new Font("Arial", 15, FontStyle.Bold);

            g.DrawString(wpno, boldF, Brushes.Black, new PointF(midw, midh));
        }
    }
}