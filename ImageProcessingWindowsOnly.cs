using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace process_transparency;

public class ImageProcessingWindowsOnly
{
    public static Bitmap SetWhiteBackground_WindowsOnly(Bitmap image)
    {
        // Bitmap image = new Bitmap(new MemoryStream(imageBytes));

        Bitmap newImage = null;

        System.Drawing.Color color = System.Drawing.Color.FromArgb(255, 255, 255, 255);

        newImage = new Bitmap(image.Width, image.Height);

        newImage.SetResolution(image.HorizontalResolution, image.VerticalResolution);
        using (var g = Graphics.FromImage(newImage))
        {
            g.Clear(Color.White);
            g.DrawImageUnscaled(image, 0, 0);
        }
        using (var ms = new MemoryStream())
        {
            newImage.Save(ms, ImageFormat.Png);
        }

        return newImage;
    }

    public static bool ContainsTransparent_WindowsOnly(Bitmap image)
    {
        for (int y = 0; y < image.Height; ++y)
        {
            for (int x = 0; x < image.Width; ++x)
            {
                if (image.GetPixel(x, y).A != 255)
                {
                    return true;
                }
            }
        }
        return false;
    }
}