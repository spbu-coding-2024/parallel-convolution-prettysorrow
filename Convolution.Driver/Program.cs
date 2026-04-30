#pragma warning disable SA1200 // Using directives should be placed correctly
#pragma warning disable SA1010 // Opening square brackets should be spaced correctly
#pragma warning disable SA1011 // Closing square brackets should be spaced correctly

using System.CommandLine;
using Convolution.Core;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

static Filter ParseFilter(string[]? filterArgs)
{
    if (filterArgs is null || filterArgs.Length == 0)
    {
        throw new ArgumentException("Filter name missing");
    }

    string name = filterArgs[0].ToLowerInvariant();
    int? arg1 = filterArgs.Length > 1 ? int.Parse(filterArgs[1]) : null;

    return name switch
    {
        "id" => Filters.Identity,
        "edges" => Filters.Edges,
        "top" => arg1.HasValue ? Filters.ShiftTop(arg1.Value) : Filters.ShiftTop(),
        "left" => arg1.HasValue ? Filters.ShiftLeft(arg1.Value) : Filters.ShiftLeft(),
        "right" => arg1.HasValue ? Filters.ShiftRight(arg1.Value) : Filters.ShiftRight(),
        "bottom" => arg1.HasValue ? Filters.ShiftBottom(arg1.Value) : Filters.ShiftBottom(),
        _ => throw new ArgumentException($"unknown filter '{name}'")
    };
}

static void ApplyFilterAndSave(Image<RgbaVector> image, string outputPath, Filter filter)
{
    using var result = Convolution.Impl.Sequential.Apply(filter, image);

    switch (Path.GetExtension(outputPath).ToLowerInvariant())
    {
        case ".jpg":
        case ".jpeg":
            result.SaveAsJpeg(outputPath);
            break;
        case ".png":
        default:
            result.SaveAsPng(outputPath);
            break;
    }

    Console.WriteLine($"info: filter applied. result in {outputPath}");
}

var inputOption = new Option<string>("--input")
{
    Description = "Use specified image as input (is incompatible with --generate)",
};

var outputOption = new Option<string>("--output")
{
    Description = "Path of output file",
    DefaultValueFactory = _ => "output.png",
};

var generateOption = new Option<bool>("--generate")
{
    Description = "Use random generated image as input (is incompatible with --input)",
    DefaultValueFactory = _ => false,
};

var seedOption = new Option<int?>("--seed")
{
    Description = "Generating seed",
};

var filterOption = new Option<string[]>("--filter")
{
    Description = "Filter's name and it's arguments. Examples: --filter edges, --filter left 10",
    AllowMultipleArgumentsPerToken = true,
    Arity = ArgumentArity.OneOrMore,
    DefaultValueFactory = _ => ["id"],
};

var rootCommand = new RootCommand("driver")
{
    inputOption,
    outputOption,
    generateOption,
    seedOption,
    filterOption,
};

rootCommand.SetAction(parseResult =>
{
    bool generate = parseResult.GetValue(generateOption);
    string? input = parseResult.GetValue(inputOption);
    int? seed = parseResult.GetValue(seedOption);

    var image = (generate, input) switch
    {
        (true, null) => new ImageGenerator(seed).Next(),
        (false, not null) => Image.Load<RgbaVector>(input),
        (false, null) => throw new Exception("specify --generate or --input"),
        (true, not null) => throw new Exception("--generate and --input are incompatible")
    };

    string? output = parseResult.GetValue(outputOption);

    if (string.IsNullOrWhiteSpace(output))
    {
        throw new Exception("specify --output");
    }

    Filter filter = ParseFilter(parseResult.GetValue(filterOption));

    ApplyFilterAndSave(image, output, filter);
});

return rootCommand.Parse(args).Invoke();
