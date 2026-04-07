namespace Convolution.Core;

using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

public interface IConvolution<TPixel>
    where TPixel : unmanaged, IPixel<TPixel>
{
    public static abstract void ApplyInPlace(Image<TPixel> image, Filter parameters);

    public static abstract Image<TPixel> ApplyOutOfPlace(Image<TPixel> source, Filter parameters);
}