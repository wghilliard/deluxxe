# Raffle Standard Operating Procedure (SOP)

## Automated Event Creation (Recommended)

The enhanced `create-event.py` script now automates the entire event setup process:

### Prerequisites
1. Activate the virtual environment:
   ```bash
   source venv/bin/activate
   ```

2. Ensure you have your MYLAPS account ID (for SpeedHive API integration)

### Usage Examples

#### With SpeedHive API Integration (Recommended)
Automatically finds the latest event from target organizations and extracts Group 1 race sessions:

```bash
python bin/create-event.py -n "2025-spring-sprints" -o output -a "MYLAPS-GA-fee46dbc16df4da7ad0a8f8c973a563d"
```

#### With Manual Event ID
If you know the specific SpeedHive event ID:

```bash
python bin/create-event.py -n "2025-spring-sprints" -o output -e 3032241
```

#### Basic Usage (No SpeedHive Integration)
Creates directory structure and links files, but requires manual config updates:

```bash
python bin/create-event.py -n "2025-spring-sprints" -o output
```

## What the Script Automates

1. ✅ **Event Directory Creation** - Creates proper directory structure (collateral, deluxxe, previous-results)
2. ✅ **SpeedHive Integration** - Queries API for latest events from Cascade Sports Car Club or IRDC
3. ✅ **Group 1 Race Detection** - Extracts Group 1 race sessions and their URIs
4. ✅ **File Linking** - Automatically links latest car-to-sticker-mapping and prize-descriptions files
5. ✅ **Config Generation** - Creates `deluxxe.json` from template with proper substitutions
6. ✅ **Previous Results** - Links historical event results for context

## Manual Steps (If Needed)

Only required if using basic usage without SpeedHive integration:

1. Update `car-to-sticker-mapping-YYYY-MM-DD.csv` in output directory
2. Update `prize-descriptions-YYYY-MM-DD.json` in output directory  
3. Manually edit `deluxxe.json` to set correct race result URIs if not auto-populated

## Command Line Options

- `-n, --event-name`: Name of the event (required)
- `-o, --output-dir`: Output directory path (required) 
- `-a, --mylaps-account`: MYLAPS account ID for SpeedHive API queries (optional)
- `-e, --event-id`: Specific SpeedHive event ID to use (optional)