using System;
using System.Drawing;

namespace SpotTheDifferenceGame.UI
{
    public static class FeedbackDrawer
    {
        public static Image DrawCircle(Image original, Point point, bool isCorrect)
        {
            Bitmap bmp = new Bitmap(original);
            using (Graphics g = Graphics.FromImage(bmp))
            {
                int radius = 20;
                Color color = isCorrect ? Color.LimeGreen : Color.Red;
                Pen pen = new Pen(color, 3);

                Rectangle circleRect = new Rectangle(point.X - radius, point.Y - radius, radius * 2, radius * 2);
                g.DrawEllipse(pen, circleRect);

                if (!isCorrect)
                {
                    g.DrawLine(pen, circleRect.Left, circleRect.Top, circleRect.Right, circleRect.Bottom);
                    g.DrawLine(pen, circleRect.Left, circleRect.Bottom, circleRect.Right, circleRect.Top);
                }
            }
            return bmp;
        }
    }
}
