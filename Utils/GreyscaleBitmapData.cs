using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Net;
using System.Security.Cryptography;

namespace RevitDoom.Utils
{
    class GreyscaleBitmapData
    {
        Bitmap _bitmap;

        static Bitmap GetBitmap(int w, int h, string url)
        {
            using (WebClient client = new WebClient())
            {
                byte[] data = client.DownloadData(url);

                using (Image img = Image.FromStream(new MemoryStream(data)))
                {
                    return new Bitmap(
                      img.GetThumbnailImage(w, h, null, IntPtr.Zero));
                }
            }
        }

        public static Bitmap RgbBufferToBitmap(byte[] buffer, int width, int height)
        {
            var bitmap = new Bitmap(width, height, System.Drawing.Imaging.PixelFormat.Format32bppArgb);

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    int i = (x * height + y) * 4;

                    if (i + 2 >= buffer.Length)
                        continue;

                    byte r = buffer[i];
                    byte g = buffer[i + 1];
                    byte b = buffer[i + 2];

                    Color color = Color.FromArgb(255, r, g, b); // Alpha = 255
                    bitmap.SetPixel(x, y, color);
                }
            }

            return bitmap;
        }


        public GreyscaleBitmapData(int w, int h, string url)
        {
            _bitmap = GetBitmap(w, h, url);
            Debug.Assert(null != _bitmap, "expected valid bitmap");
        }

        public GreyscaleBitmapData(int w, int h, byte[] buffer)
        {
            _bitmap = RgbBufferToBitmap(buffer, w, h);
            Debug.Assert(null != _bitmap, "expected valid bitmap");
        }

        public int Width
        {
            get
            {
                return _bitmap.Width;
            }
        }

        public int Height
        {
            get
            {
                return _bitmap.Height;
            }
        }

        public double GetBrightnessAt(int x, int y)
        {
            return _bitmap.GetPixel(x, y).GetBrightness();
        }

        public byte[] HashValue
        {
            get
            {
                // convert image to a byte array

                ImageConverter ic = new ImageConverter();

                byte[] bytes = (byte[])ic.ConvertTo(
                  _bitmap, typeof(byte[]));

                // compute a hash for image

                SHA256Managed shaM = new SHA256Managed();
                return shaM.ComputeHash(bytes);
            }
        }
    }
}
