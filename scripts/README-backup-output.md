# Backup Output Script

The `backup-output.sh` script provides a simple and reliable way to backup the entire output directory to an external storage location, preserving file timestamps and permissions.

## Purpose

This script automates the backup process for Deluxxe event data by:
- Copying all output files to a backup location
- Preserving file timestamps and permissions
- Providing error checking and user feedback
- Ensuring data safety and disaster recovery

## Usage

```bash
./backup-output.sh
```

### Prerequisites

1. **Make the script executable** (if not already):
   ```bash
   chmod +x backup-output.sh
   ```

2. **Ensure backup destination exists**:
   - Default: `/Volumes/data/m365/pro3/deluxxe-outputs`
   - Update the script if your backup location is different

3. **Mount external storage** (if applicable):
   - Ensure external drives are mounted and accessible
   - Verify sufficient space for the backup

## Configuration

### Backup Destination

The script is currently configured to backup to:
```bash
/Volumes/data/m365/pro3/deluxxe-outputs
```

To change the backup location, edit the script and update the `BACKUP_DIR` variable:
```bash
BACKUP_DIR="/your/custom/backup/path"
```

### Source Directory

The script backs up from:
```bash
./output/
```

This assumes you're running the script from the project root directory.

## What Gets Backed Up

The script copies the entire `output/` directory, including:
- **Event directories** (`2025-06-09-event-name/`)
- **Car mapping files** (`car-to-sticker-mapping-*.csv`)
- **Prize description files** (`prize-descriptions-*.json`)
- **Event templates** (`event-template.json`)
- **Generated reports and results**
- **All subdirectories and files**

## Backup Method

The script uses `cp -Rp` which provides:
- **`-R`**: Recursive copying (includes all subdirectories)
- **`-p`**: Preserve file attributes (timestamps, permissions, ownership)

This ensures that:
- File modification times are preserved
- Directory structure is maintained
- File permissions remain intact
- Symbolic links are preserved

## Error Handling

The script includes comprehensive error checking:

### Pre-flight Checks
```bash
# Verify source directory exists
if [[ ! -d "$SOURCE_DIR" ]]; then
    echo "Error: Source directory $SOURCE_DIR does not exist"
    exit 1
fi

# Verify backup destination is accessible
if [[ ! -d "$BACKUP_DIR" ]]; then
    echo "Error: Backup directory $BACKUP_DIR does not exist or is not accessible"
    exit 1
fi
```

### Exit on Error
The script uses `set -e` to immediately exit if any command fails, preventing partial backups.

## Example Output

Successful backup:
```bash
$ ./backup-output.sh
Backing up ./output/ to /Volumes/data/m365/pro3/deluxxe-outputs...
Backup completed successfully
```

Error scenarios:
```bash
# Source directory missing
$ ./backup-output.sh
Error: Source directory ./output/ does not exist

# Backup destination not accessible
$ ./backup-output.sh
Error: Backup directory /Volumes/data/m365/pro3/deluxxe-outputs does not exist or is not accessible
```

## Storage Requirements

Backup space requirements depend on your output directory size:
- **Typical event**: 1-10 MB (configuration files, small data files)
- **Events with large datasets**: 10-100 MB
- **Complete project history**: 100 MB - 1 GB+

Plan for sufficient backup storage based on your event frequency and data size.

## Automation

### Scheduled Backups

Add to cron for automatic backups:
```bash
# Edit crontab
crontab -e

# Add entry for daily backup at 11 PM
0 23 * * * cd /path/to/deluxxe && ./scripts/backup-output.sh >> /var/log/deluxxe-backup.log 2>&1
```

### Pre-Event Backup

Run before each event for data safety:
```bash
# Run backup before creating new event
./scripts/backup-output.sh

# Create new event
./scripts/create-event.py -n "2025-06-15-new-event" -o ./output
```

### Post-Event Backup

Run after each event completion:
```bash
# Complete event processing
# ... run raffles, generate reports ...

# Backup results
./scripts/backup-output.sh
```

## Troubleshooting

### Permission Issues
```bash
# Make script executable
chmod +x backup-output.sh

# Fix file permissions if needed
chmod -R 755 ./output/
```

### Disk Space Issues
```bash
# Check available space
df -h /Volumes/data/m365/pro3/deluxxe-outputs

# Clean up old backups if needed
# (manually review and remove old files)
```

### Network Drive Issues
```bash
# Remount network drive
# (specific steps depend on your network storage setup)

# Test write access
touch /Volumes/data/m365/pro3/deluxxe-outputs/test-file
rm /Volumes/data/m365/pro3/deluxxe-outputs/test-file
```

## Backup Strategy

### Recommendations

1. **Regular Backups**: Run daily or after each significant change
2. **Multiple Locations**: Consider backing up to multiple destinations
3. **Version Control**: Use git for code and configuration files
4. **Cloud Backup**: Consider cloud storage for critical data
5. **Verification**: Periodically verify backup integrity

### Backup Rotation

For automated setups, consider implementing backup rotation:
```bash
# Example: Keep backups with timestamps
TIMESTAMP=$(date +%Y%m%d_%H%M%S)
BACKUP_DIR="/backup/deluxxe-$TIMESTAMP"
```

## Security Considerations

- **Access Control**: Ensure backup location has appropriate access restrictions
- **Encryption**: Consider encrypting sensitive data in backups
- **Network Security**: Use secure protocols for remote backups
- **Data Retention**: Implement appropriate data retention policies

## Dependencies

- **bash**: Standard bash shell (available on macOS/Linux)
- **cp command**: Standard UNIX copy command
- **Mounted storage**: Backup destination must be accessible

## Integration

This script works well with:
- [`create-event.py`](./README-create-event.md) - Backup before/after events
- [`update-tool.sh`](./README-update-tool.md) - Include in build workflows
- CI/CD pipelines - Automated backup integration
- Monitoring systems - Track backup success/failure
