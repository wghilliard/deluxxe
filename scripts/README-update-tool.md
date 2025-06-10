# Update Tool Script

The `update-tool.sh` script automates the build and update process for the DeluxxeCli .NET tool, handling packaging, local tool updates, and dependency restoration.

## Purpose

This script streamlines the development workflow for the DeluxxeCli tool by:
- Building and packaging the .NET project into a NuGet package
- Updating the locally installed tool from the package source
- Restoring all tool dependencies
- Providing proper error handling and feedback

## Usage

```bash
./update-tool.sh
```

### Prerequisites

1. **Make the script executable** (if not already):
   ```bash
   chmod +x update-tool.sh
   ```

2. **Ensure .NET SDK is installed**:
   ```bash
   dotnet --version
   ```

3. **Run from project root**: The script should be executed from the Deluxxe project root directory

## What the Script Does

### 1. Package the Project
```bash
dotnet pack
```
- Compiles the DeluxxeCli project
- Creates a NuGet package (`.nupkg` file)
- Places the package in `src/DeluxxeCli/nupkg/`

### 2. Update Local Tool
```bash
dotnet tool update --add-source src/DeluxxeCli/nupkg DeluxxeCli
```
- Updates the locally installed DeluxxeCli tool
- Uses the freshly built package as the source
- Installs the latest version from the local package source

### 3. Restore Dependencies
```bash
dotnet tool restore
```
- Restores all tool dependencies
- Ensures the tool environment is properly configured
- Downloads any missing tool dependencies

## Output

Successful execution example:
```bash
$ ./update-tool.sh
Building and packing DeluxxeCli...
Microsoft (R) Build Engine version 17.0.0+...
...
Successfully created package 'DeluxxeCli.1.2.4.nupkg'

Updating DeluxxeCli tool from local source...
Tool 'deluxxecli' was successfully updated from version '1.2.3' to version '1.2.4'

Restoring tool dependencies...
Tool 'deluxxecli' (version '1.2.4') was restored.

Tool update completed successfully
```

## Error Handling

The script includes comprehensive error handling:

### Build Failures
If the `dotnet pack` command fails:
- Build errors are displayed
- Script exits immediately (due to `set -e`)
- No attempt to update with broken package

### Tool Update Failures
If the tool update fails:
- Package source issues are reported
- Version conflicts are identified
- Script stops before dependency restoration

### Common Error Scenarios

**Build compilation errors:**
```bash
Building and packing DeluxxeCli...
error CS1002: ; expected
Build FAILED.
```

**Tool already up-to-date:**
```bash
Updating DeluxxeCli tool from local source...
Tool 'deluxxecli' is already installed with the latest version.
```

**Missing package source:**
```bash
Updating DeluxxeCli tool from local source...
error NU1101: Unable to find package DeluxxeCli in source(s): src/DeluxxeCli/nupkg
```

## Development Workflow Integration

### Typical Development Cycle

1. **Make code changes** to DeluxxeCli project
2. **Run the update script**:
   ```bash
   ./scripts/update-tool.sh
   ```
3. **Test the updated tool**:
   ```bash
   dotnet tool run DeluxxeCli --help
   ```

### Version Management Integration

Combine with version updates:
```bash
# Update version first
python scripts/update-tool-version.py src/DeluxxeCli/DeluxxeCli.csproj

# Build and update tool
./scripts/update-tool.sh

# Verify new version
dotnet tool run DeluxxeCli --version
```

### Git Workflow Integration

```bash
# Complete development cycle
git checkout -b feature/new-functionality
# ... make changes ...
git add .
git commit -m "Add new functionality"

# Update version and tool
python scripts/update-tool-version.py src/DeluxxeCli/DeluxxeCli.csproj
./scripts/update-tool.sh

# Test and finalize
git add src/DeluxxeCli/DeluxxeCli.csproj
git commit -m "Bump version and update tool"
```

## Package Management

### Local Package Source

The script uses a local package source at `src/DeluxxeCli/nupkg/`:
- Contains locally built NuGet packages
- Allows testing changes before publishing
- Enables offline development and testing

### Package Versioning

Each build creates a new package version:
- Version number comes from the `.csproj` file
- Incremental updates are supported
- Multiple versions can coexist in the package source

### Cleanup

Periodically clean up old packages:
```bash
# Remove old package versions (optional)
rm src/DeluxxeCli/nupkg/*.nupkg

# Rebuild from scratch
./scripts/update-tool.sh
```

## Tool Installation Modes

The script works with different .NET tool installation modes:

### Local Tools (Recommended)
```bash
# Install as local tool (in .config/dotnet-tools.json)
dotnet tool install --local DeluxxeCli --add-source src/DeluxxeCli/nupkg
```

### Global Tools
```bash
# Install as global tool
dotnet tool install --global DeluxxeCli --add-source src/DeluxxeCli/nupkg
```

## Troubleshooting

### Permission Issues
```bash
# Make script executable
chmod +x update-tool.sh

# Fix directory permissions if needed
chmod -R 755 src/DeluxxeCli/nupkg/
```

### .NET SDK Issues
```bash
# Verify .NET SDK installation
dotnet --info

# Check for required SDK version
# (as specified in global.json or project file)
```

### Package Source Issues
```bash
# Verify package source directory exists
ls -la src/DeluxxeCli/nupkg/

# Manually create if missing
mkdir -p src/DeluxxeCli/nupkg/
```

### Tool Installation Issues
```bash
# List installed tools
dotnet tool list

# Force reinstall if needed
dotnet tool uninstall DeluxxeCli
./scripts/update-tool.sh
```

## Advanced Usage

### Custom Package Sources

To use additional package sources:
```bash
# Add multiple sources
dotnet tool update DeluxxeCli \
  --add-source src/DeluxxeCli/nupkg \
  --add-source https://api.nuget.org/v3/index.json
```

### Specific Version Updates

```bash
# Update to specific version
dotnet tool update DeluxxeCli --version 1.2.3 --add-source src/DeluxxeCli/nupkg
```

### Configuration File Support

The script can be extended to support configuration:
```bash
# Example: Add configuration file support
if [[ -f .tool-config ]]; then
    source .tool-config
fi
```

## Dependencies

- **.NET SDK**: Required for building and tool management
- **bash**: Standard bash shell (macOS/Linux)
- **Project structure**: Assumes standard .NET project layout

## Integration with Other Scripts

This script works well with:
- [`update-tool-version.py`](./README-update-tool-version.md) - Version management
- [`backup-output.sh`](./README-backup-output.md) - Data backup workflows
- CI/CD pipelines - Automated build and deployment
- Development environment setup scripts

## Performance Considerations

- **Incremental builds**: .NET only rebuilds changed components
- **Package caching**: NuGet packages are cached locally
- **Tool restoration**: Only missing dependencies are downloaded
- **Parallel execution**: Multiple build tasks can run concurrently
