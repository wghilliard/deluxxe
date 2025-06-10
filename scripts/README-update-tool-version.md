# Update Tool Version Script

The `update-tool-version.py` script automatically updates version numbers in .NET project files (`.csproj`) by incrementing the patch version number.

## Purpose

This script helps automate version management for .NET projects by:
- Reading version information from `.csproj` files
- Incrementing the patch version (e.g., 1.2.3 → 1.2.4)
- Supporting both `<Version>` and `<VersionPrefix>` tags
- Writing the updated version back to the file

## Usage

```bash
python update-tool-version.py <path-to-csproj-file>
```

### Examples

```bash
# Update the DeluxxeCli project version
python update-tool-version.py ../src/DeluxxeCli/DeluxxeCli.csproj

# Update any .csproj file
python update-tool-version.py /path/to/project/MyProject.csproj
```

## Supported Version Formats

The script supports two common .NET project version patterns:

### Version Tag
```xml
<Version>1.2.3</Version>
```

### VersionPrefix Tag
```xml
<VersionPrefix>1.2.3</VersionPrefix>
```

## Behavior

1. **Reads the .csproj file** and searches for version tags
2. **Parses the version** in the format `major.minor.patch`
3. **Increments the patch number** by 1
4. **Updates the file** with the new version
5. **Reports the change** to the console

### Example Output
```
Found <Version> tag in ../src/DeluxxeCli/DeluxxeCli.csproj
Updating version from 1.2.3 to 1.2.4
Successfully updated version in ../src/DeluxxeCli/DeluxxeCli.csproj to 1.2.4
```

## Error Handling

The script provides comprehensive error handling for:

### File Issues
- **File not found**: Clear error message if the specified file doesn't exist
- **Permission errors**: Handles read/write permission issues
- **Invalid file format**: Graceful handling of malformed XML

### Version Tag Issues
- **Missing version tags**: Checks for both `<Version>` and `<VersionPrefix>` tags
- **Invalid version format**: Validates semantic version format (major.minor.patch)
- **Multiple version tags**: Handles files with multiple version specifications

### Example Error Messages
```bash
# File not found
Error: File not found at /path/to/missing/file.csproj

# No version tags found
Error: Could not find <Version> or <VersionPrefix> tag in project.csproj

# Invalid version format
Error: Invalid version format in project file
```

## Integration with Development Workflow

This script is commonly used in development automation:

### Manual Version Updates
```bash
# Update version before publishing
python update-tool-version.py ../src/DeluxxeCli/DeluxxeCli.csproj
```

### Automated Builds
The script can be integrated into build scripts or CI/CD pipelines:
```bash
# Build script example
python update-tool-version.py src/MyProject/MyProject.csproj
dotnet pack src/MyProject/MyProject.csproj
dotnet tool update --local MyProject
```

### Git Workflow Integration
```bash
# Update version and commit
python update-tool-version.py src/Project/Project.csproj
git add src/Project/Project.csproj
git commit -m "Bump version to $(grep '<Version>' src/Project/Project.csproj | sed 's/.*<Version>\(.*\)<\/Version>.*/\1/')"
```

## Limitations

- **Semantic Versioning Only**: Supports `major.minor.patch` format only
- **Single Version Tag**: Processes the first version tag found
- **XML Format**: Requires well-formed XML in the .csproj file
- **Patch Increment Only**: Only increments the patch version, not major or minor

## Dependencies

- **Python 3.7+**: Standard library modules only
- **No external packages required**

## File Safety

The script includes several safety measures:
- **File existence check** before attempting to read
- **Backup creation** (content is read fully before writing)
- **Atomic write** (full content replacement, not partial updates)
- **Error rollback** (original file remains unchanged if errors occur)

## Related Scripts

This script is often used in conjunction with:
- [`update-tool.sh`](./README-update-tool.md) - Builds and updates the .NET tool
- Build automation scripts
- Release preparation workflows

## Advanced Usage

### Custom Version Patterns
While the script currently only supports patch increment, it can be extended for:
- Minor version increments
- Major version increments
- Pre-release version handling
- Build metadata updates

### Batch Processing
For projects with multiple .csproj files:
```bash
# Update all projects in a solution
find . -name "*.csproj" -exec python update-tool-version.py {} \;
```
