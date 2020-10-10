using ImageMagick;
using System;
using System.Drawing;
using System.IO;
using System.Linq.Expressions;
using System.Windows.Forms;

namespace Gaze
{
    static class ImageLoader
    {
        public static Image Load(string path, int width=0, int height=0)
        {
            if (Path.GetExtension(path).ToLower() == ".afdesign")
            {
                var bytes = File.ReadAllBytes(path);
                byte[] header = { 137, 80, 78, 71, 13, 10, 26, 10 };

                int max = bytes.Length - 8;
                for (int i = 0; i < max; i++)
                {
                    if (bytes[i + 0] == header[0] &&
                        bytes[i + 1] == header[1] &&
                        bytes[i + 2] == header[2] &&
                        bytes[i + 3] == header[3] &&
                        bytes[i + 4] == header[4] &&
                        bytes[i + 5] == header[5] &&
                        bytes[i + 6] == header[6] &&
                        bytes[i + 7] == header[7])
                    {
                        int pngSize = bytes.Length - i;
                        byte[] pngBytes = new byte[pngSize];

                        Array.Copy(bytes, i, pngBytes, 0, pngSize);

                        using (var magickImage = new MagickImage(pngBytes, MagickFormat.Png))
                        {
                            if (width != 0 && height != 0)
                            {
                                return CreateThumbnail(magickImage, width, height);
                            }
                            else
                            {
                                return magickImage.ToBitmap();
                            }
                        }
                    }
                }

                return null;
            }
            else
            {
                using (var magickImage = new MagickImage(path))
                {
                    magickImage.AutoOrient();

                    if (width != 0 && height != 0)
                    {
                        return CreateThumbnail(magickImage, width, height);
                    }
                    else
                    {
                        return magickImage.ToBitmap();
                    }
                }
            }
        }

        private static Image CreateThumbnail(MagickImage magickImage, int width, int height)
        {
            magickImage.Thumbnail(width, height);
            var bmp = new Bitmap(width, height, System.Drawing.Imaging.PixelFormat.Format32bppArgb);

            using (var g = Graphics.FromImage(bmp))
            {
                using (var magicBitmap = magickImage.ToBitmap())
                {
                    g.DrawImage(magicBitmap, (width - magickImage.Width) / 2, (height - magicBitmap.Height) / 2);
                }
            }

            return bmp;
        }
    }
}
