using Microsoft.Extensions.Configuration;

namespace Componyx.Search.Indexer;

public static class Program
{
    public static int Main(string[] args)
    {
        var configuration = new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("appsettings.json", optional: true, reloadOnChange: false)
            .Build();

        var options = new IndexerOptions
        {
            InputDirectories = [.. configuration
                .GetSection("InputDirectories")
                .GetChildren()
                .Select(x => x.Value ?? string.Empty)],

            Url = configuration["WebURL"] ?? string.Empty,
            Replacements = configuration["Replacements"] ?? string.Empty,
            FileInput = configuration["JSON:FileInput"] ?? string.Empty,
            FileOutput = configuration["JSON:FileOutput"] ?? string.Empty,
            Compress = bool.TryParse(configuration["JSON:Compress"], out var compress) && compress
        };

        ApplyArguments(options, args);

        if (options.InputDirectories.Length == 0)
        {
            Console.Error.WriteLine("No input directory configured.");
            Console.Error.WriteLine("Use InputDirectories in appsettings.json or --input <directory>.");
            return 1;
        }

        if (string.IsNullOrWhiteSpace(options.FileOutput))
            options.FileOutput = "KeywordIndex";

        options.FileOutput += options.Compress ? ".json.gz" : ".json";

        try
        {
            var indexer = new Indexer(options);
            indexer.Extract();

            return 0;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Error: {ex.Message}");
            return 1;
        }
    }

    private static void ApplyArguments(IndexerOptions options, string[] args)
    {
        for (var i = 0; i < args.Length; i++)
        {
            switch (args[i].ToLowerInvariant())
            {
                case "--input":
                    options.InputDirectories = options.InputDirectories
                        .Append(GetArgumentValue(args, ref i))
                        .ToArray();
                    break;

                case "--output":
                    options.FileOutput = GetArgumentValue(args, ref i);
                    break;

                case "--input-index":
                    options.FileInput = GetArgumentValue(args, ref i);
                    break;

                case "--url":
                    options.Url = GetArgumentValue(args, ref i);
                    break;

                case "--replacements":
                    options.Replacements = GetArgumentValue(args, ref i);
                    break;

                case "--files":
                    options.Files = GetArgumentValue(args, ref i);
                    break;

                case "--exclude":
                    options.Exclude = true;
                    break;

                case "--compress":
                    options.Compress = true;
                    break;

                case "--help":
                case "-h":
                case "/?":
                    PrintHelp();
                    Environment.Exit(0);
                    break;

                default:
                    throw new ArgumentException($"Unknown argument: {args[i]}");
            }
        }
    }

    private static string GetArgumentValue(string[] args, ref int index)
    {
        if (index + 1 >= args.Length)
            throw new ArgumentException($"Missing value for {args[index]}.");

        return args[++index];
    }

    private static void PrintHelp()
    {
        Console.WriteLine("Componyx Search Indexer");
        Console.WriteLine();
        Console.WriteLine("Usage:");
        Console.WriteLine("  Componyx.Search.Indexer [options]");
        Console.WriteLine();
        Console.WriteLine("Options:");
        Console.WriteLine("  --input <directory>       Input directory. Can be specified multiple times.");
        Console.WriteLine("  --output <file>           Output .json.gz file.");
        Console.WriteLine("  --input-index <file>      Existing index to load.");
        Console.WriteLine("  --url <url>               Base URL for rendered HTML.");
        Console.WriteLine("  --replacements <value>    Route replacements separated by '|'.");
        Console.WriteLine("  --files <files>           Files separated by '|'.");
        Console.WriteLine("  --exclude                 Treat --files as an exclusion list.");
        Console.WriteLine("  --compress                Compress the output as gzip.");
        Console.WriteLine("  --help, -h                Show this help.");
        Console.WriteLine();
        Console.WriteLine("Values from command-line arguments override appsettings.json.");
    }
}