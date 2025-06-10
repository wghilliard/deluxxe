# Deluxxe Scripts Directory

This directory contains various scripts for managing Deluxxe raffle events, car mappings, and development tools.

## Scripts Overview

### Python Scripts

- **`compute-odds.py`** - Analyzes raffle winners and computes odds from race results
- **`create-event.py`** - Creates event directory structures with SpeedHive integration  
- **`download-car-mapping.py`** - Downloads car-to-sticker mapping from Google Sheets
- **`update-tool-version.py`** - Updates version numbers in .NET project files

### Shell Scripts

- **`backup-output.sh`** - Backs up the output directory to external storage
- **`update-tool.sh`** - Builds and updates the DeluxxeCli .NET tool

## Prerequisites

### Python Dependencies
Install required Python packages:
```bash
pip install -r requirements.txt
```

### .NET Dependencies
Ensure you have the .NET SDK installed for the .NET tool scripts.

### Google Sheets API Setup
For car mapping scripts, you'll need:
1. Google Cloud Console project with Sheets API enabled
2. OAuth2 credentials file
3. Proper configuration file

## Configuration Files

- **`car-mapping-config.json`** - Configuration for Google Sheets integration (copy from `.example`)
- **`requirements.txt`** - Python dependencies

## Quick Start

1. **Install dependencies:**
   ```bash
   pip install -r requirements.txt
   ```

2. **Set up Google Sheets API** (for car mapping):
   ```bash
   cp car-mapping-config.json.example car-mapping-config.json
   # Edit car-mapping-config.json with your settings
   ```

3. **Run scripts as needed:**
   ```bash
   # Create a new event
   python create-event.py -n "2025-06-15-test-event" -o ../output

   # Download car mapping
   python download-car-mapping.py

   # Update tool version
   python update-tool-version.py ../src/DeluxxeCli/DeluxxeCli.csproj
   ```

## Documentation

Each script has its own detailed README file:
- [`README-create-event.md`](./README-create-event.md) - Event creation script
- [`README-car-mapping.md`](./README-car-mapping.md) - Car mapping download
- [`README-compute-odds.md`](./README-compute-odds.md) - Odds computation
- [`README-update-tool-version.md`](./README-update-tool-version.md) - Version management
- [`README-backup-output.md`](./README-backup-output.md) - Backup script
- [`README-update-tool.md`](./README-update-tool.md) - Tool update script

## Development

All Python scripts follow modern Python conventions:
- Type hints throughout
- Comprehensive docstrings
- Proper error handling
- Clear argument parsing

Shell scripts include:
- Proper error handling with `set -e`
- Documentation comments
- Input validation where appropriate
