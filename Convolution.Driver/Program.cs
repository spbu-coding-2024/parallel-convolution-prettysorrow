#pragma warning disable SA1200 // Using directives should be placed correctly

using System.CommandLine;
using Convolution.Core;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

void SaveImage(Image<RgbaVector> image, string path)
{
    string extension = Path.GetExtension(path).ToLowerInvariant();
    switch (extension)
    {
        case ".jpg":
        case ".jpeg":
            image.SaveAsJpeg(path);
            break;
        case ".png":
        default:
            image.SaveAsPng(path);
            break;
    }
}

void ApplyFilterAndSave(Image<RgbaVector> image, string outputPath, string filterName)
{
    Filter filter = filterName.ToLowerInvariant() switch
    {
        "id" => Filters.Identity,
        "edges" => Filters.Edges,
        "left" => Filters.ShiftLeft,
        "right" => Filters.ShiftRight,
        "top" => Filters.ShiftTop,
        "bottom" => Filters.ShiftBottom,
        _ => throw new ArgumentException($"unknown filter '{filterName}'")
    };

    using var resultImage = Convolution.Impl.Parallel.Apply(image, filter);
    SaveImage(resultImage, outputPath);
    Console.WriteLine($"filter '{filterName}' has been applied. result is in {Path.GetFullPath(outputPath)}");
}

var generateOption = new Option<bool>("--generate")
{
    Description = "generate",
    DefaultValueFactory = _ => false,
};

var widthOption = new Option<int>("--width")
{
    Description = "width of generated image",
    DefaultValueFactory = _ => 800,
};

var heightOption = new Option<int>("--height")
{
    Description = "height of generated image",
    DefaultValueFactory = _ => 600,
};

var seedOption = new Option<int?>("--seed")
{
    Description = "generating seed",
};

var countOption = new Option<int>("--count")
{
    Description = "amount of objects on generated image",
    DefaultValueFactory = _ => 20,
};

var filterOption = new Option<string>("--filter")
{
    Description = "filter",
    DefaultValueFactory = _ => "id",
};

var inputOption = new Option<string>("--input")
{
    Description = "input path",
};

var outputOption = new Option<string>("--output")
{
    Description = "output path",
    DefaultValueFactory = _ => "output.png",
};

var rootCommand = new RootCommand("driver")
{
    generateOption,
    widthOption,
    heightOption,
    seedOption,
    countOption,
    filterOption,
    inputOption,
    outputOption,
};

rootCommand.SetAction(parseResult =>
{
    bool generate = parseResult.GetValue(generateOption);
    string? input = parseResult.GetValue(inputOption);

    if (generate && !string.IsNullOrEmpty(input))
    {
        Console.Error.WriteLine("err: --generate and --input are incompatible");
        Environment.ExitCode = 1;
        return;
    }

    if (!generate && string.IsNullOrEmpty(input))
    {
        Console.Error.WriteLine("err: specify --generate or --input");
        Environment.ExitCode = 1;
        return;
    }

    string output = parseResult.GetValue(outputOption)!;
    string filterName = parseResult.GetValue(filterOption)!;

    try
    {
        if (generate)
        {
            int width = parseResult.GetValue(widthOption);
            int height = parseResult.GetValue(heightOption);
            int? seed = parseResult.GetValue(seedOption);
            int shapeCount = parseResult.GetValue(countOption);

            using var image = new ImageGenerator(seed).Next(width, height, shapeCount);
            ApplyFilterAndSave(image, output, filterName);
        }
        else
        {
            if (!File.Exists(input))
            {
                Console.Error.WriteLine($"err: not found: '{input}'");
                Environment.ExitCode = 1;
                return;
            }

            using var inputImage = Image.Load<RgbaVector>(input);
            ApplyFilterAndSave(inputImage, output, filterName);
        }
    }
    catch (Exception ex)
    {
        Console.Error.WriteLine($"err: exception: {ex.Message}");
        Environment.ExitCode = 1;
    }
});

return rootCommand.Parse(args).Invoke();
