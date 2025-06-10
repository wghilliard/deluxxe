# Car-to-Sticker Mapping Download

The `download-car-mapping.py` script automatically downloads car-to-sticker mapping data from a private Google Sheet and formats it as a CSV file with proper column mapping and data validation.

## Setup

### 1. Install Dependencies

```bash
cd bin
pip install -r requirements.txt
```

### 2. Set up Google Sheets API

1. Go to the [Google Cloud Console](https://console.cloud.google.com/)
2. Create a new project or select an existing one
3. Enable the Google Sheets API:
   - Go to "APIs & Services" > "Library"
   - Search for "Google Sheets API"
   - Click on it and press "Enable"
4. Create credentials:
   - Go to "APIs & Services" > "Credentials"
   - Click "Create Credentials" > "OAuth client ID"
   - Choose "Desktop application" as the application type
   - Download the JSON file and save it as `credentials.json` in the `bin/` directory

### 3. Configure the Script

1. Copy the example configuration file:
   ```bash
   cp car-mapping-config.json.example car-mapping-config.json
   ```

2. Edit `car-mapping-config.json` with your specific settings:
   - Update `spreadsheet_id` with your Google Sheet ID (found in the URL)
   - Adjust `range_name` if your data is in a different sheet or range
   - Modify `column_mapping` if your sheet has different column names
   - Update `output_columns` to match the desired output format

### 4. Google Sheet ID

To find your Google Sheet ID:
- Open your Google Sheet in a browser
- Look at the URL: `https://docs.google.com/spreadsheets/d/SPREADSHEET_ID/edit#gid=0`
- The `SPREADSHEET_ID` part is what you need

## Usage

```bash
cd bin
python download-car-mapping.py
```

### First Run

The first time you run the script, it will:
1. Open a browser window for Google OAuth authentication
2. Ask you to sign in to your Google account
3. Request permission to read your Google Sheets
4. Save the authentication token for future runs

After the first run, the script will use the saved token and won't require manual authentication.

## Output

The script saves the downloaded data to:
`../output/car-to-sticker-mapping-YYYY-MM-DD.csv`

Where `YYYY-MM-DD` is today's date.

## Configuration Options

### Column Mapping

The `column_mapping` section maps your Google Sheet column names to the expected output column names:

```json
{
  "column_mapping": {
    "Number": "Car Number",       # Maps "Number" output to "Car Number" sheet column
    "Owner": "Driver Name",       # Maps "Owner" output to "Driver Name" sheet column
    "Listed Color": "Color",      # etc.
    ...
  }
}
```

### Range Name

- `Sheet1!A:Z` - All columns A through Z in Sheet1
- `Sheet1!A1:J100` - Specific range from A1 to J100
- `"My Data"!A:Z` - Sheet named "My Data", all columns A through Z

## Troubleshooting

### Authentication Issues

If you get authentication errors:
1. Delete `token.json` to force re-authentication
2. Make sure `credentials.json` is in the `bin/` directory
3. Ensure you have the correct permissions for the Google Sheet

### Sheet Access Issues

- Make sure the Google account you authenticate with has access to the sheet
- The sheet owner may need to share the sheet with your Google account

### Column Mapping Issues

- Check that the column names in `column_mapping` exactly match your sheet headers
- Column names are case-sensitive
- The script will warn you if mapped columns are not found

## Files

- `download-car-mapping.py` - Main script
- `car-mapping-config.json.example` - Example configuration file
- `car-mapping-config.json` - Your actual configuration (not in git)
- `credentials.json` - Google API credentials (not in git)
- `token.json` - Saved authentication token (not in git)
- `requirements.txt` - Python dependencies

## Setup

1. **Install dependencies:**
   ```bash
   pip install -r requirements.txt
   ```

2. **Create configuration file:**
   ```bash
   cp car-mapping-config.json.example car-mapping-config.json
   ```

3. **Edit the configuration file** `car-mapping-config.json` with your Google Sheets URL:
   ```json
   {
     "google_sheet_url": "https://docs.google.com/spreadsheets/d/YOUR_SHEET_ID/edit?gid=YOUR_GID#gid=YOUR_GID"
   }
   ```

## Usage

```bash
cd bin
python download-car-mapping.py
```

### First Run

The first time you run the script, it will:
1. Open a browser window for Google OAuth authentication
2. Ask you to sign in to your Google account
3. Request permission to read your Google Sheets
4. Save the authentication token for future runs

After the first run, the script will use the saved token and won't require manual authentication.

## Output

The script saves the downloaded data to:
`../output/car-to-sticker-mapping-YYYY-MM-DD.csv`

Where `YYYY-MM-DD` is today's date.
