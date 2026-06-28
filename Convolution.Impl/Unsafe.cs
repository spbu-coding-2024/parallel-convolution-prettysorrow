namespace Convolution.Impl;

using Convolution.Core;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Advanced;
using System.Runtime.CompilerServices;

/// <summary>
/// Memory-access optimized parallel row-by-row image convolution implementation.
/// </summary>
public static unsafe class Unsafe
{
    public static Image<RgbaVector> Apply(this Filter filter, Image<RgbaVector> image, ParallelOptions options)
    {
        var width = image.Width;
        var height = image.Height;

        var kernel = filter.Kernel;
        var kernelSize = filter.KernelSize;
        var offset = kernelSize / 2;
        var factor = filter.Factor;
        var bias = filter.Bias;
        var handleEdges = ChoiceEdgeHandling(filter, width, height);

        var result = new Image<RgbaVector>(width, height);

        RgbaVector* pSrcBase = null;
        RgbaVector* pDstBase = null;
        var srcHandle = default(System.Buffers.MemoryHandle);
        var dstHandle = default(System.Buffers.MemoryHandle);

        try
        {
            srcHandle = image.DangerousGetPixelRowMemory(0).Pin();
            dstHandle = result.DangerousGetPixelRowMemory(0).Pin();
            pSrcBase = (RgbaVector*)srcHandle.Pointer;
            pDstBase = (RgbaVector*)dstHandle.Pointer;

            fixed (float* pKernel = kernel)
            {
                Convolve(options, handleEdges, pSrcBase, pDstBase, width, height, kernelSize, offset, factor, bias, pKernel);
            }

            return result;
        }
        finally
        {
            srcHandle.Dispose();
            dstHandle.Dispose();
        }
    }

    public static Image<RgbaVector> Apply(this Filter filter, Image<RgbaVector> image)
        => Apply(filter, image, new ParallelOptions());

    private static unsafe void Convolve(
        ParallelOptions options,
        delegate*<int, int, int> handleEdges,
        RgbaVector* pSrcBase,
        RgbaVector* pDstBase,
        int width,
        int height,
        int kernelSize,
        int offset,
        float factor,
        float bias,
        float* pKernel)
    {
        System.Threading.Tasks.Parallel.For(0, height, options, y =>
        {
            RgbaVector* pDstRow = pDstBase + (y * width);

            for (var x = 0; x < width; x++)
            {
                float r = 0f, g = 0f, b = 0f, a = 0f;

                for (var ky = 0; ky < kernelSize; ky++)
                {
                    var srcY = handleEdges(y + ky - offset, height);

                    RgbaVector* pSrcRow = pSrcBase + (srcY * width);

                    for (var kx = 0; kx < kernelSize; kx++)
                    {
                        var srcX = handleEdges(x + kx - offset, width);

                        ref readonly RgbaVector px = ref pSrcRow[srcX];
                        float weight = pKernel[(ky * kernelSize) + kx];

                        r += px.R * weight;
                        g += px.G * weight;
                        b += px.B * weight;
                        a += px.A * weight;
                    }
                }

                r = (r * factor) + bias;
                g = (g * factor) + bias;
                b = (b * factor) + bias;
                a = (a * factor) + bias;

                pDstRow[x] = new(r, g, b, a);
            }
        });
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static int EdgeClamp(int coord, int size)
        => coord < 0 ? 0 : (coord >= size ? size - 1 : coord);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static int EdgeWrapDefault(int coord, int size)
    {
        int mod = coord % size;
        return mod < 0 ? mod + size : mod;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static int EdgeWrapFast(int coord, int size)
        => coord < 0 ? coord + size : (coord >= size ? coord - size : coord);

    private static unsafe delegate*<int, int, int> ChoiceEdgeHandling(Filter filter, int width, int height)
    {
        if (filter.EdgeMode == EdgeMode.Clamp)
        {
            return &EdgeClamp;
        }
        else if ((filter.EdgeMode == EdgeMode.Wrap) && (filter.KernelSize <= width) && (filter.KernelSize <= height))
        {
            return &EdgeWrapFast;
        }
        else if (filter.EdgeMode == EdgeMode.Wrap)
        {
            return &EdgeWrapDefault;
        }

        throw new NotSupportedException("Unknown convolution edge handling mode");
    }
}
