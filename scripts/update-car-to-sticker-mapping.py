#!/usr/bin/env python3
"""
Script to download and update car-to-sticker mapping from Google Sheets.

This script:
1. Downloads the latest version of the Google Sheet
2. Parses the CSV and selects the same columns as existing mapping files
3. Writes the data to the output directory with the correct naming pattern

Usage:
    python update-car-to-sticker-mapping.py [--config-file path/to/config.json] [--date YYYY-MM-DD]
"""

import os
import json
import csv
import requests
import re
from argparse import ArgumentParser
from datetime import datetime
from pathlib import Path
from typing import Dict, List, Optional
from urllib.parse import urlparse, parse_qs


def load_config(config_file: str) -> Dict:
    """Load configuration from JSON file."""
    try:
        with open(config_file, 'r') as f:
            return json.load(f)
    except FileNotFoundError:
        raise FileNotFoundError(f"Config file not found: {config_file}")
    except json.JSONDecodeError as e:
        raise ValueError(f"Invalid JSON in config file: {e}")


def extract_sheet_id_from_url(url: str) -> str:
    """Extract the Google Sheets ID from the URL."""
    # Handle different Google Sheets URL formats
    patterns = [
        r'/spreadsheets/d/([a-zA-Z0-9-_]+)',
        r'key=([a-zA-Z0-9-_]+)',
        r'id=([a-zA-Z0-9-_]+)'
    ]
    
    for pattern in patterns:
        match = re.search(pattern, url)
        if match:
            return match.group(1)
    
    raise ValueError(f"Could not extract sheet ID from URL: {url}")


def get_csv_export_url(sheet_url: str, gid: Optional[str] = None) -> str:
    """Convert Google Sheets URL to CSV export URL."""
    sheet_id = extract_sheet_id_from_url(sheet_url)
    
    # Extract gid from URL if present and not provided
    if not gid:
        parsed_url = urlparse(sheet_url)
        if '#gid=' in sheet_url:
            gid = sheet_url.split('#gid=')[1]
        elif 'gid=' in parsed_url.fragment:
            gid = parse_qs(parsed_url.fragment)['gid'][0]
        elif 'gid=' in parsed_url.query:
            gid = parse_qs(parsed_url.query)['gid'][0]
    
    # Construct CSV export URL
    csv_url = f"https://docs.google.com/spreadsheets/d/{sheet_id}/export?format=csv"
    if gid:
        csv_url += f"&gid={gid}"
    
    return csv_url


def download_sheet_as_csv(sheet_url: str) -> str:
    """Download Google Sheet as CSV data."""
    csv_url = get_csv_export_url(sheet_url)
    
    try:
        response = requests.get(csv_url)
        response.raise_for_status()
        return response.text
    except requests.RequestException as e:
        raise RuntimeError(f"Failed to download sheet: {e}")


def parse_csv_data(csv_content: str) -> List[Dict[str, str]]:
    """Parse CSV content into list of dictionaries."""
    lines = csv_content.strip().split('\n')
    if not lines:
        raise ValueError("Empty CSV content")
    
    reader = csv.DictReader(lines)
    return list(reader)


def get_expected_columns() -> List[str]:
    """Return the expected column names based on existing files."""
    return [
        'Number', 'Owner', 'Listed Color', 'Email 1', 'Email 2', 'Is A Rental',
        '_425', 'AAF', 'Bimmerworld', 'Griots', 'Redline', 'RoR', 'Toyo', 
        'Proformance', 'Alpinestars'
    ]


def filter_and_validate_data(data: List[Dict[str, str]]) -> List[Dict[str, str]]:
    """Filter data to include only expected columns and validate format."""
    expected_columns = get_expected_columns()
    
    if not data:
        raise ValueError("No data to process")
    
    # Check if all expected columns are present
    available_columns = set(data[0].keys())
    missing_columns = set(expected_columns) - available_columns
    
    if missing_columns:
        print(f"Warning: Missing columns in source data: {missing_columns}")
        # Add missing columns with empty values
        for row in data:
            for col in missing_columns:
                row[col] = ''
    
    # Filter to only include expected columns
    filtered_data = []
    for row in data:
        filtered_row = {col: row.get(col, '') for col in expected_columns}
        filtered_data.append(filtered_row)
    
    return filtered_data


def generate_output_filename(date_str: str) -> str:
    """Generate the output filename based on the date."""
    return f"car-to-sticker-mapping-{date_str}.csv"


def write_csv_file(data: List[Dict[str, str]], output_path: str) -> None:
    """Write data to CSV file."""
    if not data:
        raise ValueError("No data to write")
    
    expected_columns = get_expected_columns()
    
    # Ensure output directory exists
    os.makedirs(os.path.dirname(output_path), exist_ok=True)
    
    with open(output_path, 'w', newline='', encoding='utf-8') as f:
        writer = csv.DictWriter(f, fieldnames=expected_columns)
        writer.writeheader()
        writer.writerows(data)
    
    print(f"Successfully wrote {len(data)} rows to {output_path}")


def main():
    parser = ArgumentParser(description="Update car-to-sticker mapping from Google Sheets")
    parser.add_argument(
        '--config-file', 
        default='bin/car-mapping-config.json',
        help='Path to configuration file (default: bin/car-mapping-config.json)'
    )
    parser.add_argument(
        '--date',
        default=datetime.now().strftime('%Y-%m-%d'),
        help='Date for output filename (default: today)'
    )
    parser.add_argument(
        '--output-dir',
        default='output',
        help='Output directory (default: output)'
    )
    
    args = parser.parse_args()
    
    try:
        # Load configuration
        config = load_config(args.config_file)
        sheet_url = config.get('google_sheet_url')
        
        if not sheet_url:
            raise ValueError("google_sheet_url not found in config file")
        
        print(f"Downloading data from Google Sheet...")
        csv_content = download_sheet_as_csv(sheet_url)
        
        print("Parsing CSV data...")
        raw_data = parse_csv_data(csv_content)
        
        print(f"Processing {len(raw_data)} rows...")
        filtered_data = filter_and_validate_data(raw_data)
        
        # Generate output filename and path
        output_filename = generate_output_filename(args.date)
        output_path = os.path.join(args.output_dir, output_filename)
        
        print(f"Writing to {output_path}...")
        write_csv_file(filtered_data, output_path)
        
        print("Car-to-sticker mapping update completed successfully!")
        
    except Exception as e:
        print(f"Error: {e}")
        return 1
    
    return 0


if __name__ == "__main__":
    exit(main())
