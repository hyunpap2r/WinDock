using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

public class CircularButton : Button
{
    protected override void OnPaint(PaintEventArgs pevent)
    {
        base.OnPaint(pevent);
        Graphics g = pevent.Graphics;
        g.SmoothingMode = SmoothingMode.AntiAlias;

        Rectangle bounds = new Rectangle(0, 0, this.Width, this.Height);
        GraphicsPath path = new GraphicsPath();
        path.AddEllipse(bounds);

        this.Region = new Region(path);

        using (SolidBrush brush = new SolidBrush(this.BackColor))
        {
            g.FillEllipse(brush, bounds);
        }

        TextRenderer.DrawText(g, this.Text, this.Font, bounds, this.ForeColor, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
    }
}
