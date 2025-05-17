using System.Drawing;
using System.IO;

namespace SpotTheDifferenceGame.Utils
{
    public static class ImageHelper
    {
        public static (Image left, Image right) LoadImagePair(string basePath, string difficulty, int level)
        {
            string leftPath = Path.Combine(basePath, difficulty, $"{level}_left.jpg");
            string rightPath = Path.Combine(basePath, difficulty, $"{level}_right.jpg");

            if (!File.Exists(leftPath) || !File.Exists(rightPath))
                return (null, null);

            return (Image.FromFile(leftPath), Image.FromFile(rightPath));
        }
    }
}
