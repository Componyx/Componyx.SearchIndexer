using HtmlAgilityPack;
using System.Text;
using System.Text.RegularExpressions;

namespace Componyx.Search.Indexer;

public sealed class Indexer
{
    private readonly IndexerOptions _options;
    private readonly JsonPlugin _plugin;

    public Indexer(IndexerOptions options)
    {
        _options = options;
        _plugin = new JsonPlugin(options.FileInput, options.FileOutput, options.Compress);
    }

    public string Extract()
    {
        var files = GetFiles();

        _plugin.Init();

        Console.WriteLine($"Found {files.Length} file(s).");

        for (var index = 0; index < files.Length; index++)
        {
            var filePath = files[index];

            if (ShouldExclude(filePath))
                continue;

            Console.WriteLine($"[{index + 1}/{files.Length}] {GetRelativePath(filePath)}");

            var doc = LoadDocument(filePath);
            var route = GetRoute(filePath);

            if (!string.IsNullOrEmpty(_options.Replacements))
                route = ReplaceRoute(route, _options.Replacements);

            var title = GetTitle(doc);

            RemoveNodes(doc);

            // Text before the first heading with an id belongs to the page itself;
            // all text after such a heading belongs to that heading, which becomes a #bookmark route
            var segments = new List<(string? Id, string Heading, StringBuilder Text)>
            {
                (null, title, new StringBuilder())
            };

            foreach (var node in doc.DocumentNode.Descendants())
            {
                if (IsHeading(node) && node.GetAttributeValue("id", "") is var id && id != "")
                    segments.Add((id, CleanText(node.InnerText), new StringBuilder()));
                else if (node.NodeType == HtmlNodeType.Text)
                    segments[^1].Text.Append(' ').Append(HtmlEntity.DeEntitize(node.InnerText));
            }

            foreach (var segment in segments)
            {
                var text = segment.Text.ToString();

                if (string.IsNullOrWhiteSpace(text))
                    continue;

                var segmentRoute = segment.Id == null ? route : $"{route}#{segment.Id}";
                var segmentTitle = segment.Id == null ? title : $"{title} - {segment.Heading}";

                _plugin.Add(index, files.Length, filePath, segmentRoute, segmentTitle, Extractor.Extract(text));
            }
        }

        return _plugin.Data;
    }

    private static bool IsHeading(HtmlNode node) => node.Name.Length == 2 && node.Name[0] == 'h' && node.Name[1] >= '1' && node.Name[1] <= '6';

    private static string CleanText(string text) => Regex.Replace(HtmlEntity.DeEntitize(text), @"\s+", " ").Trim();

    private string[] GetFiles()
    {
        if (!string.IsNullOrWhiteSpace(_options.Files))
        {
            return _options.Files
                .Split('|', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Select(x => Path.IsPathRooted(x)
                    ? x
                    : Path.Combine(_options.InputDirectories[0], x))
                .ToArray();
        }

        return new[] { "*.html", "*.aspx", "*.cshtml", "*.vbhtml" }
            .SelectMany(pattern => _options.InputDirectories
                .SelectMany(directory =>
                    Directory.GetFiles(directory, pattern, SearchOption.AllDirectories)))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(x => x, StringComparer.OrdinalIgnoreCase)
            .ToArray();
    }

    private bool ShouldExclude(string filePath)
    {
        if (!_options.Exclude || string.IsNullOrWhiteSpace(_options.Files))
            return false;

        var relativePath = GetRelativePath(filePath);

        return _options.Files
            .Split('|', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Any(x => x.Equals(relativePath, StringComparison.OrdinalIgnoreCase));
    }

    private HtmlDocument LoadDocument(string filePath)
    {
        var doc = new HtmlDocument();

        if (!string.IsNullOrEmpty(_options.Url))
        {
            using var client = new HttpClient();

            var url = $"{GetWebUrl(filePath)}{GetRelativePath(filePath).Replace('\\', '/')}";

            doc.LoadHtml(client.GetStringAsync(url).GetAwaiter().GetResult());
        }
        else
        {
            doc.Load(filePath);
        }

        return doc;
    }

    private string GetRelativePath(string filePath)
    {
        var directory = _options.InputDirectories
            .First(x => filePath.StartsWith(
                x.TrimEnd(Path.DirectorySeparatorChar) + Path.DirectorySeparatorChar,
                StringComparison.OrdinalIgnoreCase));

        return Path.GetRelativePath(directory, filePath);
    }

    private string GetWebUrl(string filePath)
    {
        var directory = _options.InputDirectories
            .First(x => filePath.StartsWith(
                x.TrimEnd(Path.DirectorySeparatorChar) + Path.DirectorySeparatorChar,
                StringComparison.OrdinalIgnoreCase));

        if (directory.EndsWith(
                Path.Combine("wwwroot", "view"),
                StringComparison.OrdinalIgnoreCase))
        {
            return $"{_options.Url.TrimEnd('/')}/../view/";
        }

        return _options.Url.TrimEnd('/') + "/";
    }

    private string GetRoute(string filePath)
    {
        var route = GetRelativePath(filePath).Replace('\\', '/');
        var extension = Path.GetExtension(route);

        if (!string.IsNullOrEmpty(extension))
            route = route[..^extension.Length];

        return "/" + route.TrimStart('/');
    }

    private static string ReplaceRoute(string route, string replacements)
    {
        foreach (var replacement in replacements.Split('|', StringSplitOptions.RemoveEmptyEntries))
        {
            var separator = replacement.IndexOf(':');
            if (separator <= 0)
                continue;

            var oldValue = replacement[..separator];
            var newValue = replacement[(separator + 1)..];
            route = route.Replace(oldValue, newValue, StringComparison.OrdinalIgnoreCase);
        }

        return route;
    }

    private static void RemoveNodes(HtmlDocument doc)
    {
        var nodes = doc.DocumentNode.Descendants()
            .Where(n => n.Name.Equals("head", StringComparison.OrdinalIgnoreCase) ||
                        n.Name.Equals("script", StringComparison.OrdinalIgnoreCase) ||
                        n.Name.Equals("style", StringComparison.OrdinalIgnoreCase) ||
                        n.Name.Equals("pre", StringComparison.OrdinalIgnoreCase))
            .ToList();

        foreach (var node in nodes)
            node.Remove();
    }

    private static string GetTitle(HtmlDocument doc)
    {
        var title = doc.DocumentNode.Descendants("title").FirstOrDefault()?.InnerText;

        if (string.IsNullOrWhiteSpace(title))
            title = doc.DocumentNode.Descendants("h1").FirstOrDefault()?.InnerText;

        return string.IsNullOrWhiteSpace(title)
            ? string.Empty
            : Regex.Replace(title, @"\s+", " ").Trim();
    }
}