namespace Convolution.Impl;

using System.Threading.Channels;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

public sealed class AsyncPipelineOptions
{
    public Func<Image<RgbaVector>, Image<RgbaVector>> Convolve { get; init; }

    public (int read, int convolve, int write) WorkerCount { get; init; }

    public (int paths, int raw, int convolved) ChannelCapacity { get; init; }

    public AsyncPipelineOptions(
        Func<Image<RgbaVector>, Image<RgbaVector>> convolve,
        (int read, int convolve, int write) workerCount,
        (int paths, int raw, int convolved) channelCapacity)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(workerCount.read);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(workerCount.convolve);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(workerCount.write);

        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(channelCapacity.paths);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(channelCapacity.raw);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(channelCapacity.convolved);

        this.Convolve = convolve;
        this.WorkerCount = workerCount;
        this.ChannelCapacity = channelCapacity;
    }

    public AsyncPipelineOptions(Func<Image<RgbaVector>, Image<RgbaVector>> convolve)
    {
        this.Convolve = convolve;
        var pc = Environment.ProcessorCount;
        this.WorkerCount = (read: 2, convolve: pc, write: 2);
        this.ChannelCapacity = (paths: pc * 2, raw: pc * 2, convolved: pc * 2);
    }
}

/// <summary>
/// Provides several convolution pipelines for stream of images.
/// </summary>
public static class Pipeline
{
    public static void ProcessSync(IEnumerable<string> inputPaths, Func<string, string> makeOutputPath, Func<Image<RgbaVector>, Image<RgbaVector>> convolve)
    {
        foreach (var path in inputPaths)
        {
            using var image = Image.Load<RgbaVector>(path);
            using var result = convolve(image);
            string resultPath = makeOutputPath(path);
            result.Save(resultPath);
        }
    }

    public static void ProcessSequentialSync(IEnumerable<string> inputPaths, Func<string, string> makeOutputPath, Convolution.Core.Filter filter)
    {
        ProcessSync(inputPaths, makeOutputPath, convolve: image => Convolution.Impl.Sequential.Apply(filter, image));
    }

    public static void ProcessParallelSync(IEnumerable<string> inputPaths, Func<string, string> makeOutputPath, Convolution.Core.Filter filter)
    {
        ProcessSync(inputPaths, makeOutputPath, convolve: image => Convolution.Impl.Parallel.Apply(filter, image));
    }

    public static void ProccessUnsafeSync(IEnumerable<string> inputPaths, Func<string, string> makeOutputPath, Convolution.Core.Filter filter)
    {
        ProcessSync(inputPaths, makeOutputPath, convolve: image => Convolution.Impl.Unsafe.Apply(filter, image));
    }

    private readonly record struct ImageEnvelope(string path, Image<RgbaVector> image);

    public static async Task ProcessAsync(
        IEnumerable<string> inputPaths,
        Func<string, string> makeOutputPath,
        AsyncPipelineOptions options,
        CancellationToken ct = default)
    {
        var pathChannel = Channel.CreateBounded<string>(new BoundedChannelOptions(options.ChannelCapacity.paths) { FullMode = BoundedChannelFullMode.Wait });
        var rawChannel = Channel.CreateBounded<ImageEnvelope>(new BoundedChannelOptions(options.ChannelCapacity.raw) { FullMode = BoundedChannelFullMode.Wait });
        var convolvedChannel = Channel.CreateBounded<ImageEnvelope>(new BoundedChannelOptions(options.ChannelCapacity.convolved) { FullMode = BoundedChannelFullMode.Wait });

        async Task WritePaths()
        {
            foreach (var path in inputPaths)
            {
                await pathChannel.Writer.WriteAsync(path, ct);
            }
        }

        async Task ReadImages()
        {
            await foreach (string path in pathChannel.Reader.ReadAllAsync(ct))
            {
                var image = await Image.LoadAsync<RgbaVector>(path, ct);
                await rawChannel.Writer.WriteAsync(new ImageEnvelope(path, image), ct);
            }
        }

        async Task ConvolveImages()
        {
            await foreach (var envelope in rawChannel.Reader.ReadAllAsync(ct))
            {
                var convolved = options.Convolve(envelope.image);
                envelope.image.Dispose();
                await convolvedChannel.Writer.WriteAsync(new ImageEnvelope(envelope.path, convolved), ct);
            }
        }

        async Task WriteImages()
        {
            await foreach (var envelope in convolvedChannel.Reader.ReadAllAsync(ct))
            {
                var outputPath = makeOutputPath(envelope.path);
                await envelope.image.SaveAsync(outputPath, ct);
                envelope.image.Dispose();
            }
        }

        var pathWriter = Task.Run(WritePaths, ct);

        var readers = new Task[options.WorkerCount.read];
        for (var i = 0; i < options.WorkerCount.read; i++)
        {
            readers[i] = Task.Run(ReadImages, ct);
        }

        var modifiers = new Task[options.WorkerCount.convolve];
        for (var i = 0; i < options.WorkerCount.convolve; i++)
        {
            modifiers[i] = Task.Run(ConvolveImages, ct);
        }

        var writers = new Task[options.WorkerCount.write];
        for (var i = 0; i < options.WorkerCount.write; i++)
        {
            writers[i] = Task.Run(WriteImages, ct);
        }

        await pathWriter;
        pathChannel.Writer.TryComplete();

        await Task.WhenAll(readers);
        rawChannel.Writer.TryComplete();

        await Task.WhenAll(modifiers);
        convolvedChannel.Writer.TryComplete();

        await Task.WhenAll(writers);
    }

    public static async Task ProcessAsync(IEnumerable<string> inputPaths, Func<string, string> makeOutputPath, Func<Image<RgbaVector>, Image<RgbaVector>> convolve)
    {
        await ProcessAsync(inputPaths, makeOutputPath, new AsyncPipelineOptions(convolve));
    }

    public static async Task ProcessSequentialAsync(IEnumerable<string> inputPaths, Func<string, string> makeOutputPath, Convolution.Core.Filter filter)
    {
        await ProcessAsync(inputPaths, makeOutputPath, convolve: image => Convolution.Impl.Sequential.Apply(filter, image));
    }

    public static async Task ProcessParallelAsync(IEnumerable<string> inputPaths, Func<string, string> makeOutputPath, Convolution.Core.Filter filter)
    {
        await ProcessAsync(inputPaths, makeOutputPath, convolve: image => Convolution.Impl.Parallel.Apply(filter, image));
    }

    public static async Task ProcessUnsafeAsync(IEnumerable<string> inputPaths, Func<string, string> makeOutputPath, Convolution.Core.Filter filter)
    {
        await ProcessAsync(inputPaths, makeOutputPath, convolve: image => Convolution.Impl.Unsafe.Apply(filter, image));
    }
}
