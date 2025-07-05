using Deluxxe.IO;

namespace DeluxxeCli;

public class FileSystemDirectoryManager(RuntimeContext runtimeContext) : IDirectoryManager
{
    private const string ConfigurationDirectoryName = "conf";
    private const string CollateralDirectoryName = "collateral";
    private const string DeluxxeDirectoryName = "deluxxe";
    private const string PreviousResultsDirectoryName = "previous-results";

    public DirectoryInfo outputDir { get; } = runtimeContext.outputDir;

    public DirectoryInfo eventDir
    {
        get
        {
            if (runtimeContext.uniqueEventName is null)
            {
                throw new InvalidOperationException("Event name is not set in the runtime context.");
            }

            return new DirectoryInfo(Path.Combine(outputDir.FullName, runtimeContext.uniqueEventName));
        }
    }

    public DirectoryInfo configDir => new(Path.Combine(outputDir.FullName, ConfigurationDirectoryName));

    public DirectoryInfo deluxxeDir
    {
        get
        {
            if (runtimeContext.uniqueEventName is null)
            {
                throw new InvalidOperationException("Event name is not set in the runtime context.");
            }

            return new DirectoryInfo(Path.Combine(eventDir.FullName, DeluxxeDirectoryName));
        }
    }

    public string deluxxeDirRelative => $"{outputDir.Name}/{runtimeContext.uniqueEventName}/{DeluxxeDirectoryName}";

    public FileInfo deluxxeConfigFile
    {
        get
        {
            if (runtimeContext.uniqueEventName is null)
            {
                throw new InvalidOperationException("Event name is not set in the runtime context.");
            }

            return new FileInfo(Path.Combine(deluxxeDir.FullName, "deluxxe.json"));
        }
    }

    public DirectoryInfo collateralDir
    {
        get
        {
            if (runtimeContext.uniqueEventName is null)
            {
                throw new InvalidOperationException("Event name is not set in the runtime context.");
            }

            return new DirectoryInfo(Path.Combine(eventDir.FullName, CollateralDirectoryName));
        }
    }

    public DirectoryInfo previousResultsDir
    {
        get
        {
            if (runtimeContext.uniqueEventName is null)
            {
                throw new InvalidOperationException("Event name is not set in the runtime context.");
            }

            return new DirectoryInfo(Path.Combine(deluxxeDir.FullName, PreviousResultsDirectoryName));
        }
    }

    public FileInfo raffleResultsJsonFile
    {
        get
        {
            if (runtimeContext.uniqueEventName is null)
            {
                throw new InvalidOperationException("Event name is not set in the runtime context.");
            }


            return new FileInfo(Path.Combine(deluxxeDir.FullName, $"{runtimeContext.season}-{runtimeContext.eventName}-results.json"));
        }
    }
}