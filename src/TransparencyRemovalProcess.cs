using SkiaSharp;

namespace RemoveImageTransparency;

public class TransparencyRemovalProcess : IDisposable
{
    private SKBitmap _image = null; // origin image

    public TransparencyRemovalProcess(byte[] imageBytes)
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

    // check transparency of an image using Skiashap library
    public void SetColorToTransparentPixels(FillTransparency fillTransparencyValue, bool smoothEdging)
    {
        int countTransparentPixels = 0;
        SKColor requestedColor = new SKColor();

        if (fillTransparencyValue == FillTransparency.White)
            requestedColor = new SKColor(byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue);
        else if (fillTransparencyValue == FillTransparency.Black)
            requestedColor = new SKColor(0, 0, 0, byte.MaxValue);

        // loop through each pixel in the image and make it fully opaque if it is not
        for (int y = 0; y < _image.Height; y++)
        {
            for (int x = 0; x < _image.Width; x++)
            {
                // get the color of the pixel
                var color = _image.GetPixel(x, y);

                // if the pixel has transparency
                if (color.Alpha < byte.MaxValue)
                {
                    if (color.Alpha == byte.MinValue)
                    {
                        _image.SetPixel(x, y, requestedColor);
                    }
                    else
                    {
                        if (smoothEdging)
                            _image.SetPixel(x, y, SmoothPixel(color, fillTransparencyValue));
                        else
                            _image.SetPixel(x, y, new SKColor(color.Red, color.Green, color.Blue, 255)); // just make the pixel opaque
                    }

                    //countTransparentPixels++;
                }
            }
        }
    }

    public byte[] SkBitmapToByteArray()
    {
        var imageStream = new MemoryStream();

        // convert the image to a byte array
        _image.Encode(imageStream, SKEncodedImageFormat.Png, 100);

        imageStream.Position = 0;
        // memorystream to byte array
        byte[] imageBytes = imageStream.ToArray();

        return imageBytes;
    }

    // if an image contains transparent pixels using skiashap library
    public SKBitmap FromByteToSkiaImageUsingSkCodec(byte[] imageBytes)
    {
        // get height and width of the image from byte array using skiashap library
        var imageInfo = SKImageInfo.Empty;
        
        using (var stream = new MemoryStream(imageBytes))
        using (var codec = SKCodec.Create(stream))
        {
            imageInfo = codec.Info;
        }

        // create an empty bitmap
        var bitmap = new SKBitmap(imageInfo);

        return bitmap;
    }

    public byte[] ReturnImageBytes()
    {
        var bytes = SkBitmapToByteArray();
        Dispose();

        return bytes;
    }

    public void Dispose()
    {
        _image.Dispose();
    }

    // Make the color lighter or darker according to the fill-transparency value
    private SKColor SmoothPixel(SKColor color, FillTransparency fillTransparency)
    {
        SKColor newColor = color;
        int colorAdjustment = (byte.MaxValue - color.Alpha);
        // int moreSmoothColorAdjustment = (byte.MaxValue - color.Alpha) / 2; // but if transparency is used badly, transparent pixels will become prominent.

        if (fillTransparency == FillTransparency.White)
        {
            // make the color lighter
            newColor = new SKColor(
                (byte)(color.Red + colorAdjustment < byte.MaxValue ? color.Red + colorAdjustment : byte.MaxValue),
                (byte)(color.Green + colorAdjustment < byte.MaxValue ? color.Green + colorAdjustment : byte.MaxValue),
                (byte)(color.Blue + colorAdjustment < byte.MaxValue ? color.Blue + colorAdjustment : byte.MaxValue),
                byte.MaxValue);
        }
        else if (fillTransparency == FillTransparency.Black)
        {
            // make the color darker
            newColor = new SKColor(
                (byte)(color.Red - colorAdjustment > byte.MinValue ? color.Red - colorAdjustment : byte.MinValue),
                (byte)(color.Green - colorAdjustment > byte.MinValue ? color.Green - colorAdjustment : byte.MinValue),
                (byte)(color.Blue - colorAdjustment > byte.MinValue ? color.Blue - colorAdjustment : byte.MinValue),
                byte.MaxValue);
        }

        return newColor;
    }
}
