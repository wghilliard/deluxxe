# Create Event Script

The `create-event.py` script automates the creation of event directory structures and configuration files for Deluxxe raffle management, with automatic integration to SpeedHive for race event data retrieval.

## Features

1. **SpeedHive API Integration**: Automatically queries SpeedHive for the latest events from target organizations (Cascade Sports Car Club, IRDC)
2. **Automatic File Linking**: Creates soft links to the latest prize descriptions and car-to-sticker mapping files
3. **Config Generation**: Automatically generates `deluxxe.json` from the event template with proper substitutions
4. **Group 1 Race Detection**: Extracts Group 1 race sessions and their URIs for use in the configuration
5. **Previous Results Linking**: Automatically links to previous event results for historical context

## Usage

### Basic Usage (Manual Event ID)
```bash
python create-event.py -n "2025-06-15-test-event" -o /path/to/output -e 3032241
```

### SpeedHive Integration (Recommended)
```bash
python create-event.py -n "2025-06-15-pacific-northwest" -o ../output -a "MYLAPS-GA-fee46dbc16df4da7ad0a8f8c973a563d"
```

### Full Example with All Options
```bash
python create-event.py \
  --event-name "2025-06-15-pacific-northwest" \
  --output-dir ../output \
  --mylaps-account "MYLAPS-GA-fee46dbc16df4da7ad0a8f8c973a563d" \
  --event-id 3032241
```

## Arguments

- `-n, --event-name` (required): Name of the event directory to create
- `-o, --output-dir` (required): Output directory path where event will be created
- `-a, --mylaps-account` (optional): MYLAPS account ID for SpeedHive API queries
- `-e, --event-id` (optional): Specific SpeedHive event ID to use (overrides auto-detection)

## Dependencies

Install required dependencies:
```bash
pip install -r requirements.txt
```

Required packages:
- `requests>=2.28.0` - For SpeedHive API calls

## What the Script Does

### 1. Directory Structure Creation
Creates the following directory structure:
```
{event-name}/
├── collateral/           # Marketing materials, sponsor info
├── deluxxe/             # Main event configuration and data
│   └── previous-results/ # Links to previous event results
```

### 2. SpeedHive Integration (if account ID provided)
- Queries SpeedHive API for user's events
- Finds the latest event from target organizations (Cascade Sports Car Club, IRDC)
- Extracts Group 1 race sessions and their details
- Retrieves session IDs and result URIs

### 3. File Management
- Finds and links the latest support files:
  - `car-to-sticker-mapping-YYYY-MM-DD.csv`
  - `prize-descriptions-YYYY-MM-DD.json`
- Creates symbolic links in the event's deluxxe directory

### 4. Configuration Generation
Creates `deluxxe.json` configuration file with:
- Event name and season information
- SpeedHive event ID
- Race result URIs for each session
- References to support files
- Proper event structure

### 5. Previous Results Linking
- Scans existing events for result files
- Creates links to previous event results in `previous-results/`
- Skips test events and temporary files

## Template Substitutions

The script replaces the following placeholders in `event-template.json`:

- `{season}` → Current year or year extracted from event name
- `{event-name}` → Normalized event name (lowercase, hyphens)
- `{speedhive-event-id}` → Actual SpeedHive event ID
- `{sat-session-id}` → Saturday race session ID
- `{sat-race-result-uri}` → Saturday race result API URL
- `{sun-session-id}` → Sunday race session ID  
- `{sun-race-result-uri}` → Sunday race result API URL
- File references → Latest actual file names

## Target Organizations

The script automatically searches for events from these organizations:
- Cascade Sports Car Club
- IRDC

## Error Handling

The script handles:
- Missing SpeedHive account credentials
- Network errors during API calls
- Missing template files
- File permission issues
- Invalid event data

## Output

Example output:
```
Querying SpeedHive for events...
Found latest event: 2025 Pacific Northwest Challenge (ID: 3032241)
Found 2 Group 1 race sessions
Latest car mapping file: car-to-sticker-mapping-2025-06-09.csv
Latest prize descriptions file: prize-descriptions-2025-06-03.json
Linked car-to-sticker-mapping-2025-06-09.csv
Linked prize-descriptions-2025-06-03.json
Created deluxxe.json config
```

## Prerequisites

1. **Event Template**: Ensure `event-template.json` exists in the output directory
2. **Support Files**: Latest car mapping and prize description files should be available
3. **SpeedHive Access**: Valid MYLAPS account ID for API access (optional but recommended)

## Troubleshooting

### SpeedHive API Issues
- Verify MYLAPS account ID is correct
- Check network connectivity
- Ensure the account has access to the target events

### Template Issues
- Ensure `event-template.json` exists in the output directory
- Verify template contains expected placeholders

### File Linking Issues
- Check file permissions in output directory
- Ensure support files exist and are readable

## Notes

- Event names are automatically normalized (lowercase, spaces to hyphens)
- The script preserves year information when present in event names
- Single-race events are automatically detected and configured appropriately
- Previous results linking helps maintain historical context across events
