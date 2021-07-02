using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;

namespace MyUtils.Standard.Drawing
{
    public static class DrawingUtil
    {
        /// <summary>
        /// Get <see cref="Point"/> of <paramref name="src"/>
        /// </summary>
        /// <param name="src"></param>
        /// <returns></returns>
        public static Point TopLeft(this Rectangle src) => new Point(src.Left, src.Top);

        /// <summary>
        /// Get <see cref="Point"/> of <paramref name="src"/> - <paramref name="other"/>
        /// </summary>
        /// <param name="src"></param>
        /// <param name="other"></param>
        /// <returns></returns>
        public static Point Substract(this Point src, Point other) => new Point(src.X - other.X, src.Y - other.Y);

        public static Bitmap CropEdge(this Bitmap bmp, Color backgroundColor, int padding = 5)
        {
            int w = bmp.Width;
            int h = bmp.Height;

            Func<int, bool> allWhiteRow = row =>
            {
                for (int i = 0; i < w; ++i)
                    //if (bmp.GetPixel(i, row).R != 255)
                    if (!bmp.GetPixel(i, row).GetBrightness().Equals(backgroundColor.GetBrightness()) ||
                        !bmp.GetPixel(i, row).GetSaturation().Equals(backgroundColor.GetSaturation()))
                        return false;
                return true;
            };

            Func<int, bool> allWhiteColumn = col =>
            {
                for (int i = 0; i < h; ++i)
                    //if (bmp.GetPixel(col, i).R != 255)
                    if (!bmp.GetPixel(col, i).GetBrightness().Equals(backgroundColor.GetBrightness()) ||
                        !bmp.GetPixel(col, i).GetSaturation().Equals(backgroundColor.GetSaturation()))
                        return false;
                return true;
            };

            int topmost = 0;
            for (int row = 0; row < h; ++row)
            {
                if (allWhiteRow(row))
                    topmost = row;
                else break;
            }

            int bottommost = 0;
            for (int row = h - 1; row >= 0; --row)
            {
                if (allWhiteRow(row))
                    bottommost = row;
                else break;
            }

            int leftmost = 0;
            for (int col = 0; col < w; ++col)
            {
                if (allWhiteColumn(col))
                    leftmost = col;
                else
                    break;
            }

            int rightmost = 0;
            for (int col = w - 1; col >= 0; --col)
            {
                if (allWhiteColumn(col))
                    rightmost = col;
                else
                    break;
            }

            if (rightmost == 0) rightmost = w; // As reached left
            if (bottommost == 0) bottommost = h; // As reached top.

            int croppedWidth = rightmost - leftmost;
            int croppedHeight = bottommost - topmost;

            if (croppedWidth == 0) // No border on left or right
            {
                leftmost = 0;
                croppedWidth = w;
            }

            if (croppedHeight == 0) // No border on top or bottom
            {
                topmost = 0;
                croppedHeight = h;
            }

            try
            {
                var target = new Bitmap(croppedWidth + padding * 2, croppedHeight + padding * 2);
                using (Graphics g = Graphics.FromImage(target))
                using (SolidBrush brush = new SolidBrush(backgroundColor))
                {
                    g.FillRectangle(brush, 0, 0, target.Width, target.Height);
                    g.DrawImage(bmp,
                      new RectangleF(0 + padding, 0 + padding, croppedWidth, croppedHeight),
                      new RectangleF(leftmost, topmost, croppedWidth, croppedHeight),
                      GraphicsUnit.Pixel);
                }
                return target;
            }
            catch (Exception ex)
            {
                throw new Exception(
                  string.Format("Values are topmost={0} btm={1} left={2} right={3} croppedWidth={4} croppedHeight={5}", topmost, bottommost, leftmost, rightmost, croppedWidth, croppedHeight),
                  ex);
            }
        }

        /// <summary>
        /// As Save() will sometimes raise exception "A generic error occurred in GDI+.", use this method instead.
        /// </summary>
        /// <remarks>Refer to https://stackoverflow.com/a/15862892/4684232 </remarks>
        /// <param name="image"></param>
        /// <param name="filename"></param>
        /// <param name="format"></param>
        public static void SaveSafe(this Image image, string filename, ImageFormat format = null)
        {
            using (MemoryStream memory = new MemoryStream())
            {
                using (FileStream fs = new FileStream(filename, FileMode.Create, FileAccess.ReadWrite))
                {
                    image.Save(memory, format ?? image.RawFormat);
                    byte[] bytes = memory.ToArray();
                    fs.Write(bytes, 0, bytes.Length);
                }
            }
        }
        
        /// <summary>
        /// Scale the <paramref name="src"/> by <paramref name="scalingFactor"/>
        /// </summary>
        /// <param name="src"></param>
        /// <param name="scalingFactor"></param>
        /// <returns></returns>
        public static Rectangle Scale(this Rectangle src, double scalingFactor) => scalingFactor == 1d ? src : new Rectangle((int)(src.Left * scalingFactor), (int)(src.Top * scalingFactor), (int)(src.Width * scalingFactor), (int)(src.Height * scalingFactor));

        /// <summary>
        /// Get a Relative Rectangle of a box within a canvas
        /// </summary>
        /// <param name="box"></param>
        /// <param name="canvas"></param>
        /// <returns></returns>
        public static Rectangle RelativeTo(this Rectangle box, Rectangle canvas) => new Rectangle(new Point(box.Left - canvas.Left, box.Top - canvas.Top), box.Size);
        
        /// <summary>
        /// From <paramref name="sourceImage"/>, crop a <see cref="Bitmap"/> with stated <paramref name="region"/>
        /// </summary>
        /// <param name="sourceImage"></param>
        /// <param name="region"></param>
        /// <param name="pixelFormat"></param>
        /// <returns></returns>
        public static Bitmap CropWithGraphics(this Bitmap sourceImage, Rectangle region, PixelFormat pixelFormat = PixelFormat.DontCare)
        {
            Bitmap b = new Bitmap(region.Width, region.Height, pixelFormat == PixelFormat.DontCare ? sourceImage.PixelFormat : pixelFormat);
            using (Graphics g = Graphics.FromImage(b))
            {
                g.DrawImage(sourceImage, new Rectangle(0, 0, region.Width, region.Height), region, GraphicsUnit.Pixel);
            }

            return b;
        }

        /// <summary>
        /// From <paramref name="sourceImage"/>, crop a <see cref="Bitmap"/> with stated <paramref name="region"/>, 
        /// and also replace a color with <paramref name="colorCode"/> (default #ebebeb) to white
        /// </summary>
        /// <param name="sourceImage"></param>
        /// <param name="region"></param>
        /// <param name="pixelFormat"></param>
        /// <returns></returns>
        public static Bitmap CropWithGraphicsRemoveGrey(this Bitmap sourceImage, Rectangle region, PixelFormat pixelFormat = PixelFormat.DontCare, string colorCode = "#ebebeb")
        {
            ColorMap[] colorMap = new ColorMap[1];
            colorMap[0] = new ColorMap();
            colorMap[0].OldColor = ColorTranslator.FromHtml(colorCode);
            colorMap[0].NewColor = Color.White;
            ImageAttributes attr = new ImageAttributes();
            attr.SetRemapTable(colorMap);
            
            Bitmap b = new Bitmap(region.Width, region.Height, pixelFormat == PixelFormat.DontCare ? sourceImage.PixelFormat : pixelFormat);
            using (Graphics g = Graphics.FromImage(b))
            {
                g.DrawImage(sourceImage, new Rectangle(0, 0, region.Width, region.Height), region.X, region.Y, region.Width, region.Height, GraphicsUnit.Pixel, attr);
            }

            return b;
        }

        /// <summary>
        /// From <paramref name="sourceImage"/>, make a clonsed version with stated <paramref name="pixelFormat"/>.
        /// </summary>
        /// <param name="sourceImage"></param>
        /// <param name="pixelFormat">CAUTION: CANNOT use Indexed <see cref="PixelFormat"/>, e.g. <see cref="PixelFormat.Format8bppIndexed"/>, or exception will be thrown</param>
        /// <param name="disposeSourceImage">Tell whether the source image should be disposed on return</param>
        /// <returns></returns>
        public static Bitmap CloneToFormat(this Bitmap sourceImage, PixelFormat pixelFormat = PixelFormat.Format24bppRgb, bool disposeSourceImage = false)
        {
            Bitmap b = new Bitmap(sourceImage.Width, sourceImage.Height, pixelFormat);
            using (Graphics g = Graphics.FromImage(b))
            {
                var region = new Rectangle(0, 0, sourceImage.Width, sourceImage.Height);
                g.DrawImage(sourceImage, region, region, GraphicsUnit.Pixel);
            }
            
            if (disposeSourceImage)
            {
                sourceImage.Dispose();
            }
            return b;
        }

        ///// <summary>
        ///// From <paramref name="sourceImage"/>, make a clonsed version with stated <paramref name="pixelFormat"/>
        ///// </summary>
        ///// <param name="sourceImage"></param>
        ///// <param name="pixelFormat"></param>
        ///// <returns></returns>
        //public static Bitmap CloneToFormat(this Bitmap sourceImage, PixelFormat pixelFormat)
        //{
        //    Bitmap b = new Bitmap(sourceImage.Width, sourceImage.Height, pixelFormat);
        //    using (Graphics g = Graphics.FromImage(b))
        //    {
        //        var region = new Rectangle(0, 0, sourceImage.Width, sourceImage.Height);
        //        g.DrawImage(sourceImage, region, region, GraphicsUnit.Pixel);
        //    }

        //    return b;
        //}

        /// <summary>
        /// A bad cropping approach that will easily cause <see cref="OutOfMemoryException"/>. Keep it here as reference only.
        /// </summary>
        /// <param name="src"></param>
        /// <param name="rect"></param>
        /// <returns></returns>
        [Obsolete("Will cause memory error. Keep it here as reminder only", true)]
        public static Bitmap CropBitmapClone(this Bitmap src, Rectangle rect)
        {
            Bitmap cropped = src.Clone(rect, src.PixelFormat);
            return cropped;
        }

        /// <summary>
        /// Capture Screen region by Graphics
        /// </summary>
        /// <param name="region"></param>
        /// <returns></returns>
        public static Bitmap CaptureScreen(Rectangle region) => CaptureScreen(region.X, region.Y, region.Width, region.Height);

        /// <summary>
        /// Capture Screen region by Graphics
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <returns></returns>
        public static Bitmap CaptureScreen(int x, int y, int width, int height)
        {
            Bitmap b = new Bitmap(width, height);
            using (Graphics g = Graphics.FromImage(b))
            {
                g.CopyFromScreen(x, y, 0, 0, new Size(width, height), CopyPixelOperation.SourceCopy);
            }

            return b;
        }
    }
}
