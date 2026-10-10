namespace Componyx.Search.Indexer;

public sealed class IndexerOptions
{
    public string[] InputDirectories { get; set; } = [];

    public string Url { get; set; } = string.Empty;

    public string Replacements { get; set; } = string.Empty;

    public string Files { get; set; } = string.Empty;

    public bool Exclude { get; set; }

    public string FileInput { get; set; } = string.Empty;

    public string FileOutput { get; set; } = string.Empty;

    public bool Compress { get; set; } = true;
}