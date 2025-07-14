namespace Deluxxe.IO;

public interface IDirectoryManager
{
    DirectoryInfo outputDir { get; }
    DirectoryInfo staticContentDir { get; }
    DirectoryInfo eventDir { get; }
    DirectoryInfo configDir { get; }
    FileInfo operatorConfigFile { get; }
    DirectoryInfo deluxxeDir { get; }

    string deluxxeDirRelative { get; }

    FileInfo deluxxeConfigFile { get; }
    DirectoryInfo collateralDir { get; }

    DirectoryInfo previousResultsDir { get; }

    FileInfo raffleResultsJsonFile { get; }
}