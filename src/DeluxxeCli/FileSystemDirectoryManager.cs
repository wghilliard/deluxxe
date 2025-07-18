using Deluxxe.IO;

namespace DeluxxeCli;

public class FileSystemDirectoryManager(RuntimeContext runtimeContext) : IDirectoryManager
{
    private const string ConfigurationDirectoryName = "conf";
    private const string CollateralDirectoryName = "collateral";
    private const string DeluxxeDirectoryName = "deluxxe";
    private const string PreviousResultsDirectoryName = "previous-results";
    private const string StaticContentDirectoryName = "static-content";

    private const string OperatorConfigFileName = "operator_config.json";
    private const string DeluxxeConfigFileName = "deluxxe.json";

    public DirectoryInfo outputDir { get; } = runtimeContext.outputDir;

    public DirectoryInfo staticContentDir => new(Path.Combine(outputDir.FullName, StaticContentDirectoryName));

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

    public FileInfo operatorConfigFile => new(Path.Combine(configDir.FullName, OperatorConfigFileName));

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

            return new FileInfo(Path.Combine(deluxxeDir.FullName, DeluxxeConfigFileName));
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