namespace Convolution.Core;

using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

public class Filter
{
    public double[,] Kernel { get; }

    public double Factor { get; }

    public double Bias { get; }

    public enum EdgeHandlingMode
    {
        Clamp,
        Wrap,
    }

    public EdgeHandlingMode EdgeHandling { get; }


    public Filter(double[,] kernel, double factor = 1.0, double bias = 0.0)
    {
        Kernel = kernel ?? throw new ArgumentNullException(nameof(kernel));
        if (kernel.GetLength(0) != kernel.GetLength(1))
            throw new ArgumentException("Kernel must be square.");
        if (kernel.GetLength(0) % 2 == 0)
            throw new ArgumentException("Kernel size must be odd.");
        Factor = factor;
        Bias = bias;
        EdgeHandling = EdgeHandlingMode.Clamp;
    }

    public static readonly Filter Identity = new(kernel: new double[,]
    {
        { 0, 0, 0 },
        { 0, 1, 0 },
        { 0, 0, 0 }
    }, factor: 1f, bias: 0f);

    public static readonly Filter Blur3 = new(kernel: new double[,]
    {
        { 0.0, 0.2, 0.0, },
        { 0.2, 0.2, 0.2 },
        { 0.0, 0.2, 0.0 }
    }, factor: 1f, bias: 0f);

    public static readonly Filter Edges = new(kernel: new double[,]
   {
        { 0.0, 0.0, -1f, 0.0, 0.0 },
        { 0.0, 0.0, -1f, 0.0, 0.0 },
        { 0.0, 0.0, 2f, 0.0, 0.0 },
        { 0.0, 0.0, 0.0, 0.0, 0.0 },
        { 0.0, 0.0, 0.0, 0.0, 0.0 },
   }, factor: 1f, bias: 0f);

    public static readonly Filter Sharpen = new(kernel: new double[,]
 {
        { -1f, -1f, -1f, },
        { -1f, 9f, -1f, },
        { -1f, -1f, -1f, },
 }, factor: 1f, bias: 0f);
}