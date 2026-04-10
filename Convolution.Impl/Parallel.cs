namespace Convolution.Impl;

using Convolution.Core;
using Convolution.Extensions;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

public static class Parallel
{
    public static Image<Rgb24> Apply(Image<Rgb24> source, Filter filter)
        => ApplyRows(source, filter);


    public static Image<Rgb24> ApplyRows(Image<Rgb24> source, Filter filter)
    {
        Image<Rgb24> result = new(source.Width, source.Height);

        int batchSize = Math.Max(1, source.Height / Environment.ProcessorCount);
        int batchesAmount = (source.Height + batchSize - 1) / batchSize;  // ceiled source.Height / batchSize

        System.Threading.Tasks.Parallel.For(0, batchesAmount, batchIndex =>
        {
            int startX = 0;
            int endX = source.Width;
            int startY = batchIndex * batchSize;
            int endY = Math.Min(startY + batchSize, source.Height);
            ApplyOneTile(source, result, filter, startY, endY, startX, endX);
        });

        return result;
    }

    public static Image<Rgb24> ApplyColumns(Image<Rgb24> source, Filter filter)
    {
        Image<Rgb24> result = new(source.Width, source.Height);

        int batchSize = Math.Max(1, source.Width / Environment.ProcessorCount);
        int batchesAmount = (source.Width + batchSize - 1) / batchSize;  // ceiled source.Width / batchSize

        System.Threading.Tasks.Parallel.For(0, batchesAmount, batchIndex =>
        {
            int startX = batchIndex * batchSize;
            int endX = Math.Min(startX + batchSize, source.Width);
            int startY = 0;
            int endY = source.Height;

            ApplyOneTile(source, result, filter, startY, source.Height, startX, endX);
        });

        return result;
    }

    public static Image<Rgb24> ApplyTiles(Image<Rgb24> source, Filter filter, int? tileSize)
    {
        Image<Rgb24> result = new(source.Width, source.Height);

        int _tileSize = tileSize ?? EstimateOptimalTileSize(source.Width, source.Height); // TODO: weird

        int tilesAcross = (source.Width + _tileSize - 1) / _tileSize;  // ceiled source.Width / _tileSize
        int tilesDown = (source.Height + _tileSize - 1) / _tileSize;   // ceiled source.Height / _tileSize
        int totalTiles = tilesAcross * tilesDown;

        System.Threading.Tasks.Parallel.For(0, totalTiles, tileIndex =>
        {
            // left-to-right-top-to-bottom tiles traverse order
            int tileRow = tileIndex / tilesAcross;
            int tileColumn = tileIndex % tilesAcross;

            int startX = tileColumn * _tileSize;
            int endX = Math.Min(startX + _tileSize, source.Width);
            int startY = tileRow * _tileSize;
            int endY = Math.Min(startY + _tileSize, source.Height);

            ApplyOneTile(source, result, filter, startY, endY, startX, endX);
        });

        return result;
    }

    private static int EstimateOptimalTileSize(int width, int height)
    {
        int totalPixels = width * height;
        int pixelsPerTile = totalPixels / Environment.ProcessorCount;
        int estimate = (int)Math.Max(16, Math.Sqrt(pixelsPerTile)); // TODO: is 16 OK?
        return estimate;
    }

    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    private static void ApplyOneTile(
        Image<Rgb24> source,
        Image<Rgb24> destination,
        Filter filter,
        int startY,
        int endY,
        int startX,
        int endX)
    {
        for (int y = startY; y < endY; y++)
            for (int x = startX; x < endX; x++)
                destination[x, y] = filter.ApplyToRgb24(source, x, y);
    }
}
