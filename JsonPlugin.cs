using System.IO.Compression;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Componyx.Search.Indexer;

public sealed class KeywordIndexData
{
    public Dictionary<string, string> Routes { get; set; } = new();

    public SortedDictionary<string, List<string>> Keywords { get; set; } = new(StringComparer.OrdinalIgnoreCase);
}

public sealed class JsonPlugin
{
    private readonly string _input;
    private readonly string _output;
    private readonly bool _compress;

    private readonly SortedDictionary<string, List<string>> _keywords = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, string> _routes = new(StringComparer.OrdinalIgnoreCase);

    public string Data { get; private set; } = string.Empty;

    public JsonPlugin(string input, string output, bool compress)
    {
        _input = input;
        _output = output;
        _compress = compress;
    }

    public void Init()
    {
        Data = string.Empty;
        _keywords.Clear();
        _routes.Clear();

        LoadExistingIndex();
    }

    private void LoadExistingIndex()
    {
        if (string.IsNullOrWhiteSpace(_input) || !File.Exists(_input))
            return;

        string data;

        if (_input.EndsWith(".gz", StringComparison.OrdinalIgnoreCase))
        {
            using var fileStream = File.OpenRead(_input);
            using var gzipStream = new GZipStream(fileStream, CompressionMode.Decompress);
            using var reader = new StreamReader(gzipStream, Encoding.UTF8);

            data = reader.ReadToEnd();
        }
        else
        {
            data = File.ReadAllText(_input);
        }

        var indexData = JsonConvert.DeserializeObject<KeywordIndexData>(data);

        if (indexData == null)
            return;

        foreach (var route in indexData.Routes)
            _routes[route.Key] = route.Value;

        foreach (var keyword in indexData.Keywords)
            _keywords[keyword.Key] = keyword.Value;
    }

    public void Add(int fileIndex, int fileCount, string filePath, string route, string title, string[] keywords)
    {
        if (!_routes.ContainsKey(route))
            _routes[route] = title;

        foreach (var keyword in keywords)
        {
            if (!_keywords.TryGetValue(keyword, out var routes))
            {
                routes = new List<string>();
                _keywords[keyword] = routes;
            }

            if (!routes.Contains(route, StringComparer.OrdinalIgnoreCase))
                routes.Add(route);
        }

        if (fileIndex != fileCount - 1)
            return;

        var indexData = new KeywordIndexData
        {
            Routes = _routes,
            Keywords = _keywords
        };

        Data = JsonConvert.SerializeObject(
            indexData,
            new JsonSerializerSettings
            {
                ContractResolver = new CamelCasePropertyNamesContractResolver(),
                NullValueHandling = NullValueHandling.Ignore,
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore
            });

        WriteOutput();
    }

    private void WriteOutput()
    {
        if (string.IsNullOrWhiteSpace(_output))
            return;

        var directory = Path.GetDirectoryName(_output);

        if (!string.IsNullOrEmpty(directory))
            Directory.CreateDirectory(directory);

        if (!_compress)
        {
            File.WriteAllText(_output, Data, new UTF8Encoding(false));
            return;
        }

        using var fileStream = File.Create(_output);
        using var gzipStream = new GZipStream(fileStream, CompressionLevel.Optimal);
        using var writer = new StreamWriter(gzipStream, new UTF8Encoding(false));
        writer.Write(Data);
    }
}
