namespace Convolution.Impl;

using Convolution.Core;
using Convolution.Extensions;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

public static class Parallel
{
    public enum PartitionMode
    {
        Rows,
        Columns,
        Tiles
    }

    public static Image<Rgb24> Apply(Image<Rgb24> source, Filter filter)
        => Apply(source, filter, PartitionMode.Rows, null);

    public static Image<Rgb24> Apply(Image<Rgb24> source, Filter filter, PartitionMode mode, int? tileSize = null)
    {
        int w = source.Width;
        int h = source.Height;

        var result = new Image<Rgb24>(w, h);

        // TODO: get rid of explicit pixels sequence
        var regions = new List<(int x0, int y0, int x1, int y1)>();

        // TODO: revisit
        switch (mode)
        {
            case PartitionMode.Rows:
                int rowsPerPart = Math.Max(1, h / Environment.ProcessorCount);
                for (int y0 = 0; y0 < h; y0 += rowsPerPart)
                {
                    int y1 = Math.Min(y0 + rowsPerPart, h);
                    regions.Add((0, y0, w, y1));
                }
                break;

            case PartitionMode.Columns:
                int colsPerPart = Math.Max(1, w / Environment.ProcessorCount);
                for (int x0 = 0; x0 < w; x0 += colsPerPart)
                {
                    int x1 = Math.Min(x0 + colsPerPart, w);
                    regions.Add((x0, 0, x1, h));
                }
                break;

            case PartitionMode.Tiles:
                int tile = tileSize ?? (int)Math.Sqrt(w * h / Environment.ProcessorCount);
                tile = Math.Max(16, tile);
                for (int y0 = 0; y0 < h; y0 += tile)
                {
                    int y1 = Math.Min(y0 + tile, h);
                    for (int x0 = 0; x0 < w; x0 += tile)
                    {
                        int x1 = Math.Min(x0 + tile, w);
                        regions.Add((x0, y0, x1, y1));
                    }
                }
                break;
        }

        System.Threading.Tasks.Parallel.ForEach(regions, region =>
        {
            for (int y = region.y0; y < region.y1; y++)
                for (int x = region.x0; x < region.x1; x++)
                    result[x, y] = filter.ApplyToRgb24(source, x, y);
        });

        return result;
    }
}