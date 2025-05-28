using Emgu.CV;
using Emgu.CV.Structure;
using Emgu.CV.Util;
using System.Drawing;
using System.Collections.Generic;
using System;
using Emgu.CV.CvEnum;
using System.Drawing.Imaging;

namespace SpotTheDifferenceGame.Logic
{
    public enum DifferenceType
    {
        ColorChange
    }

    public class Difference
    {
        public Rectangle BoundingBox { get; set; }
        public Point CenterPoint { get; set; }
        public DifferenceType Type { get; set; }
        public double Confidence { get; set; }

        public Difference(Rectangle boundingBox, DifferenceType type, double confidence = 1.0)
        {
            BoundingBox = boundingBox;
            CenterPoint = new Point(
                boundingBox.X + boundingBox.Width / 2,
                boundingBox.Y + boundingBox.Height / 2
            );
            Type = type;
            Confidence = confidence;
        }

        public bool IsNear(int x, int y)
        {
            return BoundingBox.Contains(x, y);
        }
    }

    public class DifferenceDetector
    {
        private const int MinContourArea = 100;
        private const int MinMeanDifference = 15;
        private const int MergeOverlapThreshold = 20;
        private const int GaussianBlurSize = 5;
        private const int DilationIterations = 2;

        public List<Difference> GetDifferences(Bitmap image1, Bitmap image2)
        {
            if (image1.Size != image2.Size)
                throw new ArgumentException("Images must have the same dimensions");

            using (Mat mat1 = BitmapToMat(image1))
            using (Mat mat2 = BitmapToMat(image2))
            using (Mat gray1 = new Mat())
            using (Mat gray2 = new Mat())
            {
                CvInvoke.CvtColor(mat1, gray1, ColorConversion.Bgr2Gray);
                CvInvoke.CvtColor(mat2, gray2, ColorConversion.Bgr2Gray);

                CvInvoke.GaussianBlur(gray1, gray1, new Size(GaussianBlurSize, GaussianBlurSize), 0);
                CvInvoke.GaussianBlur(gray2, gray2, new Size(GaussianBlurSize, GaussianBlurSize), 0);

                using (Mat diff = new Mat())
                {
                    CvInvoke.AbsDiff(gray1, gray2, diff);

                    using (Mat thresh = new Mat())
                    {
                        CvInvoke.Threshold(diff, thresh, 0, 255, ThresholdType.Binary | ThresholdType.Otsu);

                        using (Mat kernel = CvInvoke.GetStructuringElement(ElementShape.Rectangle,
                            new Size(GaussianBlurSize, GaussianBlurSize), new Point(-1, -1)))
                        {
                            CvInvoke.MorphologyEx(thresh, thresh, MorphOp.Open, kernel, new Point(-1, -1),
                                1, BorderType.Default, new MCvScalar());
                            CvInvoke.Dilate(thresh, thresh, kernel, new Point(-1, -1),
                                DilationIterations, BorderType.Default, new MCvScalar());

                            return FindAndProcessContours(thresh, diff);
                        }
                    }
                }
            }
        }

        private List<Difference> FindAndProcessContours(Mat thresholdImage, Mat differenceImage)
        {
            List<Difference> differences = new List<Difference>();
            List<Rectangle> boxes = new List<Rectangle>();

            using (VectorOfVectorOfPoint contours = new VectorOfVectorOfPoint())
            {
                CvInvoke.FindContours(thresholdImage, contours, null, RetrType.External,
                    ChainApproxMethod.ChainApproxSimple);

                for (int i = 0; i < contours.Size; i++)
                {
                    double area = CvInvoke.ContourArea(contours[i]);
                    if (area > MinContourArea)
                    {
                        Rectangle box = CvInvoke.BoundingRectangle(contours[i]);

                        using (Mat roi = new Mat(differenceImage, box))
                        {
                            MCvScalar meanVal = CvInvoke.Mean(roi);
                            if (meanVal.V0 >= MinMeanDifference)
                            {
                                boxes.Add(box);
                            }
                        }
                    }
                }
            }

            List<Rectangle> mergedBoxes = MergeOverlappingRectangles(boxes, MergeOverlapThreshold);
            foreach (var box in mergedBoxes)
            {
                differences.Add(new Difference(box, DifferenceType.ColorChange, 1.0));
            }

            return differences;
        }

        private List<Rectangle> MergeOverlappingRectangles(List<Rectangle> rectangles, int overlapAreaThreshold)
        {
            if (rectangles.Count == 0)
                return new List<Rectangle>();

            List<Rectangle> merged = new List<Rectangle>();
            rectangles.Sort((a, b) => a.X.CompareTo(b.X));

            bool[] mergedFlags = new bool[rectangles.Count];

            for (int i = 0; i < rectangles.Count; i++)
            {
                if (mergedFlags[i]) continue;

                Rectangle current = rectangles[i];
                bool wasMergedIteration;

                do
                {
                    wasMergedIteration = false;
                    for (int j = i + 1; j < rectangles.Count; j++)
                    {
                        if (mergedFlags[j]) continue;

                        if (RectanglesOverlap(current, rectangles[j], overlapAreaThreshold))
                        {
                            current = Rectangle.Union(current, rectangles[j]);
                            mergedFlags[j] = true;
                            wasMergedIteration = true;
                        }
                    }
                } while (wasMergedIteration);

                merged.Add(current);
                mergedFlags[i] = true;
            }
            return merged;
        }

        private bool RectanglesOverlap(Rectangle r1, Rectangle r2, int areaThreshold)
        {
            Rectangle intersect = Rectangle.Intersect(r1, r2);
            return intersect.Width > 0 && intersect.Height > 0 && (intersect.Width * intersect.Height >= areaThreshold);
        }

        private Mat BitmapToMat(Bitmap bmp)
        {
            BitmapData bmpData = bmp.LockBits(
                new Rectangle(0, 0, bmp.Width, bmp.Height),
                ImageLockMode.ReadOnly,
                bmp.PixelFormat == PixelFormat.Format32bppArgb ? PixelFormat.Format32bppArgb : PixelFormat.Format24bppRgb);

            Mat matWithAlpha = new Mat(bmp.Height, bmp.Width,
                                (bmp.PixelFormat == PixelFormat.Format32bppArgb) ? DepthType.Cv8U : DepthType.Cv8U,
                                (bmp.PixelFormat == PixelFormat.Format32bppArgb) ? 4 : 3,
                                bmpData.Scan0,
                                bmpData.Stride);

            Mat result = new Mat();
            try
            {
                if (bmp.PixelFormat == PixelFormat.Format32bppArgb)
                {
                    CvInvoke.CvtColor(matWithAlpha, result, ColorConversion.Bgra2Bgr);
                }
                else
                {
                    matWithAlpha.CopyTo(result);
                }
            }
            finally
            {
                bmp.UnlockBits(bmpData);
                matWithAlpha.Dispose();
            }
            return result;
        }
    }
}