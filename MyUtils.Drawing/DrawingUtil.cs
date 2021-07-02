using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;

namespace MyUtils.Drawing
{
    public static class DrawingUtil
    {
        /// <summary>
        /// Get <see cref="System.Windows.Point"/> of <paramref name="src"/>
        /// </summary>
        /// <param name="src"></param>
        /// <returns></returns>
        public static System.Windows.Point TopLeft(this Rectangle src) => new System.Windows.Point(src.Left, src.Top);

        /// <summary>
        /// Convert <see cref="System.Windows.Rect"/> object to <see cref="Rectangle"/>
        /// </summary>
        /// <param name="src"></param>
        /// <returns></returns>
        public static Rectangle ToRectangle(this System.Windows.Rect src) => new Rectangle((int)src.Left, (int)src.Top, (int)src.Width, (int)src.Height);
        
        /// <summary>
        /// Scale the <paramref name="src"/> by <paramref name="scalingFactor"/>
        /// </summary>
        /// <param name="src"></param>
        /// <param name="scalingFactor"></param>
        /// <returns></returns>
        public static System.Windows.Rect Scale(this System.Windows.Rect src, double scalingFactor) => scalingFactor == 1d ? src : new System.Windows.Rect(src.Left * scalingFactor, src.Top * scalingFactor, src.Width * scalingFactor, src.Height * scalingFactor);
        
    }
}
