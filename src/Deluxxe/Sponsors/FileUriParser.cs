namespace Deluxxe.Sponsors;

public static class FileUriParser
{
    public static FileInfo? Parse(Uri uri)
    {
        
        FileInfo fileHandle;
        if (uri.Host == "local")
        {
            var segments = uri.Segments.Select(segment => segment.Trim('/'));
            segments = segments.Prepend(Directory.GetCurrentDirectory());
            fileHandle = new FileInfo(Path.Combine(segments.ToArray()));
        }
        else
        {
            fileHandle = new FileInfo(string.Concat(uri.Segments));
        }
        
        if (!fileHandle.Exists)
        {
            throw new FileNotFoundException("File not found", fileHandle.FullName);
        }
        
        return fileHandle;
    }
}