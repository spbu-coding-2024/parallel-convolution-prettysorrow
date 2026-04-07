namespace Convolution.Extentions;

using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;


public static class ImageSharpExtentions
{
    public static bool Equal<TPixel>(this Image<TPixel> source, Image<TPixel>? other)
        where TPixel : unmanaged, IPixel<TPixel>
    {
        if (ReferenceEquals(source, other))
            return true;

        if (other is null)
            return false;

        if (source.Width != other.Width || source.Height != other.Height)
            return false;

        for (int y = 0; y < source.Height; y++)
        {
            for (int x = 0; x < source.Width; x++)
            {
                if (!source[x, y].Equals(other[x, y]))
                    return false;
            }
        }

        return true;
    }
}