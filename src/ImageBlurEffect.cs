using SkiaSharp;

namespace process_transparency
{
    public class ImageBlurEffect
    {
        private SKBitmap _image = null; // origin image

        public ImageBlurEffect(byte[] imageBytes)
        {
            _image = FromByteToSkiaImage(imageBytes);
        }

        // convert byte array to skbitmap using skiashap librar
        public SKBitmap FromByteToSkiaImage(byte[] imageBytes)
        {
            // create a memory stream from the byte array
            var stream = new MemoryStream(imageBytes);
            stream.Position = 0;
            // create a bitmap from the memory stream
            var bitmap = SKBitmap.Decode(stream);

            return bitmap;
        }

        public void BlurImage()
        {
            SKBitmap ogriginal = _image.Copy();

            int blurArea = 3;
            // loop through each pixel in the image and make it fully opaque if it is not
            for (int y = blurArea; y < _image.Height - blurArea; y++)
            {

                int sumRed = 0, sumGreen = 0, sumBlue = 0;
                for (int i = blurArea / 2 - 1; i < blurArea; i++)
                {
                    var color = _image.GetPixel(i, y);
                    sumRed += color.Red;
                    sumGreen += color.Green;
                    sumBlue += color.Blue;
                }

                for (int x = blurArea; x < _image.Width - blurArea; x++)
                {
                    // get the color of the pixel
                    var colorLeft = _image.GetPixel(x - blurArea, y);
                    var colorRight = _image.GetPixel(x + blurArea, y);
                    _image.SetPixel(x, y, BlurWithSuroundingPixels(sumRed, sumGreen, sumBlue, blurArea, colorLeft, colorRight));
                }
            }
        }

        private SKColor BlurWithSuroundingPixels(int sumRed, int sumGreen, int sumBlue, int blurArea, SKColor colorLeft, SKColor colorRight)
        {
            var avg = (int sum, int blurArea, byte left, byte right) => (byte)((sum - left + right) / blurArea);

            var newColor = new SKColor(avg(sumRed, blurArea, colorLeft.Red, colorRight.Red),
                    avg(sumGreen, blurArea, colorLeft.Green, colorRight.Green),
                    avg(sumBlue, blurArea, colorLeft.Blue, colorRight.Blue),
                    byte.MaxValue);

            return newColor;
        }

        private SKColor BlurWithLeftRightPixel(SKColor color, SKColor colorLeft, SKColor colorRight)
        {
            var avg = (byte a, byte b, byte c) => (byte)((a + b + c) / 3);

            var newColor = new SKColor(avg(color.Red, colorLeft.Red, colorRight.Red),
                avg(color.Green, colorLeft.Green, colorRight.Green),
                avg(color.Blue, colorLeft.Blue, colorRight.Blue),
                byte.MaxValue);

            return newColor;
        }

        private SKColor BlurWithIncreaseOrDecrease(SKColor color)
        {
            var add = (byte b, int adjust) => (byte)(b + adjust < byte.MaxValue ? b + adjust : byte.MaxValue);

            var newColor = new SKColor(add(color.Red, 60), add(color.Green, 60), add(color.Blue, 60));

            return newColor;
        }

    }
}
