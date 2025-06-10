# Enhanced Create Event Script

The `create-event.py` script has been enhanced to automate event creation with SpeedHive integration.

## Features

1. **SpeedHive API Integration**: Automatically queries SpeedHive for the latest events from target organizations (Cascade Sports Car Club, IRDC)
2. **Automatic File Linking**: Creates soft links to the latest prize descriptions and car-to-sticker mapping files
3. **Config Generation**: Automatically generates `deluxxe.json` from the event template with proper substitutions
4. **Group 1 Race Detection**: Extracts Group 1 race sessions and their URIs for use in the configuration

## Usage

### Basic Usage (Manual Event ID)
```bash
python create-event.py -n "my-event-name" -o /path/to/output -e 3032241
```

### SpeedHive Integration
```bash
python create-event.py -n "my-event-name" -o /path/to/output -a "MYLAPS-GA-fee46dbc16df4da7ad0a8f8c973a563d"
```

## Arguments

- `-n, --event-name`: Name of the event (required)
- `-o, --output-dir`: Output directory path (required)
- `-a, --mylaps-account`: MYLAPS account ID for SpeedHive API queries (optional)
- `-e, --event-id`: Specific SpeedHive event ID to use (optional)

## Dependencies

Install required dependencies:
```bash
pip install -r requirements.txt
```

## What the Script Does

1. Creates the event directory structure (event/collateral, event/deluxxe, event/deluxxe/previous-results)
2. If MYLAPS account ID is provided:
   - Queries SpeedHive API for user's events
   - Finds the latest event from target organizations
   - Extracts Group 1 race sessions and their details
3. Finds and links the latest support files (car mappings, prize descriptions)
4. Creates `deluxxe.json` configuration file with proper substitutions:
   - Replaces `{season}`, `{event-name}`, `{speedhive-event-id}`
   - Updates race result URIs with actual session URLs
   - References the latest support files
5. Links previous event results for historical context
