# Componyx Search Indexer

A command-line tool that builds the keyword index used by the AppSkin search in [Componyx.UI](https://github.com/Componyx/Componyx.UI).

## Why

The AppSkin search middleware doesn't crawl your site at runtime. It reads a prebuilt keyword index, `KeywordIndex.json.gz`, which maps keywords to the pages they appear on. This tool generates that file from your pages.

Run it whenever your content changes, for example as part of your build or deployment.

## What it does

* Scans one or more input directories for `.html`, `.aspx`, `.cshtml` and `.vbhtml` files.
* Reads each page from disk, or downloads the rendered page from your running site if you set a URL. Use the URL option for server-rendered pages, so the index contains the final HTML.
* Uses the page's `<title>`, or its first `<h1>`, as the title shown in search results.
* Ignores the contents of `<head>`, `<script>`, `<style>` and `<pre>` elements.
* Derives each page's route from its path relative to the input directory, without extension. For example, `docs/intro.html` becomes `/docs/intro`.
* Writes an index with two parts: `routes` (route to title) and `keywords` (keyword to routes).

## Requirements

* .NET 10 SDK

## Usage

From the project folder:

```bash
dotnet run -- --input ./wwwroot --compress
```

This creates `KeywordIndex.json.gz` in the current directory. Note the `--` after `dotnet run`, which passes the remaining arguments to the tool.

### Options

| Option | Description |
| --- | --- |
| `--input <directory>` | Input directory. Can be specified multiple times. |
| `--output <file>` | Output file name, without extension. The extension (`.json` or `.json.gz`) is added automatically. Default: `KeywordIndex`. |
| `--input-index <file>` | Existing index to load. |
| `--url <url>` | Base URL of your running site. Each page's relative path is appended to it, and the rendered HTML is indexed instead of the file on disk. |
| `--replacements <value>` | Route replacements as `old:new` pairs, separated by `\|`. For example `/home:/start\|/old:/new`. |
| `--files <files>` | Only index these files, separated by `\|`. Relative paths are resolved against the first input directory. |
| `--exclude` | Treat `--files` as an exclusion list instead. |
| `--compress` | Compress the output as gzip. Required for the AppSkin search middleware. |
| `--help`, `-h` | Show help. |

### appsettings.json

Instead of command-line arguments, you can configure the tool in `appsettings.json` next to the executable. Command-line arguments override these values.

```json
{
  "InputDirectories": [
    "C:\\Sites\\MySite\\wwwroot"
  ],
  "WebURL": "",
  "Replacements": "",
  "JSON": {
    "FileInput": "",
    "FileOutput": "KeywordIndex",
    "Compress": true
  }
}
```

## Using the index with AppSkin

By default, the AppSkin search middleware looks for `KeywordIndex.json.gz` in the same folder as the Componyx.UI assembly, which is usually your application's `bin` folder. To use a different location, set `KeywordFilePath` in the middleware's search options.

The middleware reads the index compressed, so always generate it with `--compress` (or `"Compress": true`).

## License

MIT. See [LICENSE](LICENSE).