namespace Deluxxe.IO;

public interface IDirectoryManager
{
    DirectoryInfo outputDir { get; }
    DirectoryInfo eventDir { get; }
    DirectoryInfo configDir { get; }
    DirectoryInfo deluxxeDir { get; }

    string deluxxeDirRelative { get; }

    FileInfo deluxxeConfigFile { get; }
    DirectoryInfo collateralDir { get; }

    DirectoryInfo previousResultsDir { get; }

    FileInfo raffleResultsJsonFile { get; }
}