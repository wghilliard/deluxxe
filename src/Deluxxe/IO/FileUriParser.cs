using System.Text;
using System.Text.Json;

namespace Deluxxe.IO;

public static class FileUriParser
{
    private const string FileScheme = "file";

    public static IEnumerable<FileInfo> Parse(Uri uri, IList<string>? extensions = null)
    {
        if (uri.Scheme != FileScheme)
        {
            throw new ArgumentException($"Uri scheme '{uri.Scheme}' is not supported.");
        }

        FileInfo fileHandle;
        if (uri.Host == "local")
        {
            var segments = uri.Segments.Select(segment => segment.Trim('/'));
            segments = segments.Prepend(Directory.GetCurrentDirectory());
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

    public static async Task<T?> ParseAndDeserializeSingleAsync<T>(Uri uri, IList<string>? extensions = null, CancellationToken cancellationToken = default)
    {
        Stream sponsorRecordStream = new FileStream(Parse(uri, extensions).First().FullName, FileMode.Open);
        using var sponsorRecordStreamReader = new StreamReader(sponsorRecordStream, Encoding.UTF8);
        return JsonSerializer.Deserialize<T>(await sponsorRecordStreamReader.ReadToEndAsync(cancellationToken));
    }

    public static Uri Generate(string outputDirectory, string fileName)
    {
        return new Uri($"{FileScheme}://local/{outputDirectory}/{fileName}");
    }
}