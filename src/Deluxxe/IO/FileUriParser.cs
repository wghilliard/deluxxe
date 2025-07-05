using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Deluxxe.IO;

public static class FileUriParser
{
    private const string FileScheme = "file";

    private static readonly JsonSerializerOptions DeserializingOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        Converters =
        {
            new JsonStringEnumConverter(JsonNamingPolicy.CamelCase)
        }
    };

    public static IEnumerable<FileInfo> Parse(Uri uri, IDirectoryManager directoryManager, IList<string>? extensions = null)
    {
        if (uri.Scheme != FileScheme)
        {
            throw new ArgumentException($"Uri scheme '{uri.Scheme}' is not supported.");
        }

        FileInfo fileHandle;
        if (uri.Host == "local")
        {
            var segments = uri.Segments.Select(segment => segment.Trim('/'));
            segments = segments.Prepend(directoryManager.outputDir.FullName);
            fileHandle = new FileInfo(Path.Combine(segments.ToArray()));
        }
        else
        {
            fileHandle = new FileInfo(uri.LocalPath);
        }

        var handles = new List<FileInfo>();
        var attributes = File.GetAttributes(fileHandle.FullName);
        if (attributes.HasFlag(FileAttributes.Directory))
        {
            handles.AddRange(Directory.GetFiles(fileHandle.FullName, "*.*", SearchOption.AllDirectories)
                .Select(subFileHandle => new FileInfo(subFileHandle)));
        }
        else
        {
            if (!fileHandle.Exists)
            {
                throw new FileNotFoundException("File not found", fileHandle.FullName);
            }

            handles.Add(fileHandle);
        }

        if (extensions != null)
        {
            handles = handles.Where(handle => extensions.Contains(handle.Extension[1..])).ToList();
        }

        return handles;
    }

    public static async Task<T?> ParseAndDeserializeSingleAsync<T>(Uri uri, IDirectoryManager directoryManager, IList<string>? extensions = null, CancellationToken cancellationToken = default)
    {
        await using var stream = new FileStream(Parse(uri, directoryManager, extensions).First().FullName, FileMode.Open);
        using var streamReader = new StreamReader(stream, Encoding.UTF8);
        return JsonSerializer.Deserialize<T>(await streamReader.ReadToEndAsync(cancellationToken), DeserializingOptions);
    }

    public static Uri Generate(string outputDirectory, string fileName)
    {
        return new Uri($"{FileScheme}://local/{outputDirectory}/{fileName}");
    }
}