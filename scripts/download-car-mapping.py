#!/usr/bin/env python3
"""
Download car-to-sticker mapping from Google Sheets.

This script authenticates with Google Sheets API, downloads car mapping data,
processes it according to configuration, and saves it in the expected CSV format.
"""

import os
import sys
import json
import csv
from datetime import datetime
from typing import Dict, List, Any, Optional
from google.auth.transport.requests import Request
from google.oauth2.credentials import Credentials
from google_auth_oauthlib.flow import InstalledAppFlow
from googleapiclient.discovery import build
from googleapiclient.discovery import Resource

# If modifying these scopes, delete the file token.json.
SCOPES = ['https://www.googleapis.com/auth/spreadsheets.readonly']

def load_config() -> Dict[str, Any]:
    """Load configuration from car-mapping-config.json"""
    config_path = os.path.join(os.path.dirname(__file__), 'car-mapping-config.json')
    if not os.path.exists(config_path):
        print(f"Error: Config file not found at {config_path}")
        print("Please copy car-mapping-config.json.example to car-mapping-config.json and configure it.")
        sys.exit(1)
    
    with open(config_path, 'r') as f:
        return json.load(f)

def authenticate_google_sheets(credentials_file: str) -> Resource:
    """
    Authenticate with Google Sheets API.
    
    Args:
        credentials_file: Path to Google API credentials file
        
    Returns:
        Google Sheets service resource
        
    Raises:
        SystemExit: If authentication fails or credentials file is missing
    """
    creds = None
    token_file = os.path.join(os.path.dirname(__file__), 'token.json')
    
    # The file token.json stores the user's access and refresh tokens.
    if os.path.exists(token_file):
        creds = Credentials.from_authorized_user_file(token_file, SCOPES)
    
    # If there are no (valid) credentials available, let the user log in.
    if not creds or not creds.valid:
        if creds and creds.expired and creds.refresh_token:
            creds.refresh(Request())
        else:
            if not os.path.exists(credentials_file):
                print(f"Error: Google credentials file not found at {credentials_file}")
                print("Please download the credentials.json file from Google Cloud Console.")
                print("See README-car-mapping.md for setup instructions.")
                sys.exit(1)
            
            flow = InstalledAppFlow.from_client_secrets_file(credentials_file, SCOPES)
            creds = flow.run_local_server(port=0)
        
        # Save the credentials for the next run
        with open(token_file, 'w') as token:
            token.write(creds.to_json())
    
    return build('sheets', 'v4', credentials=creds)

def get_sheet_names(service: Resource, spreadsheet_id: str) -> List[str]:
    """
    Get all sheet names in the spreadsheet.
    
    Args:
        service: Google Sheets service resource
        spreadsheet_id: Google Sheets spreadsheet ID
        
    Returns:
        List of sheet names
    """
    try:
        sheet_metadata = service.spreadsheets().get(spreadsheetId=spreadsheet_id).execute()
        sheets = sheet_metadata.get('sheets', [])
        sheet_names = [sheet.get('properties', {}).get('title', 'Unknown') for sheet in sheets]
        return sheet_names
    except Exception as e:
        print(f"Error getting sheet names: {e}")
        return []

def download_sheet_data(service: Resource, spreadsheet_id: str, range_name: str) -> List[List[str]]:
    """
    Download data from Google Sheets.
    
    Args:
        service: Google Sheets service resource
        spreadsheet_id: Google Sheets spreadsheet ID
        range_name: Sheet range to download (e.g., 'Sheet1!A:Z')
        
    Returns:
        List of rows, where each row is a list of cell values
        
    Raises:
        SystemExit: If download fails
    """
    try:
        sheet = service.spreadsheets()
        result = sheet.values().get(spreadsheetId=spreadsheet_id, range=range_name).execute()
        values = result.get('values', [])
        
        if not values:
            print("No data found in the sheet.")
            return []
        
        print(f"Downloaded {len(values)} rows from Google Sheets")
        return values
    
    except Exception as e:
        print(f"Error downloading sheet data: {e}")
        sys.exit(1)

def process_data(raw_data: List[List[str]], column_mapping: Dict[str, str]) -> List[Dict[str, str]]:
    """
    Process raw sheet data into the expected format.
    
    Args:
        raw_data: Raw data from Google Sheets (list of rows)
        column_mapping: Mapping from output columns to sheet columns
        
    Returns:
        List of processed data rows as dictionaries
    """
    if not raw_data:
        return []
    
    # First row should be headers
    headers = raw_data[0]
    print(f"Sheet headers: {headers}")
    
    # Create a mapping from sheet column names to our expected column names
    processed_data = []
    
    for row_index, row in enumerate(raw_data[1:], start=2):  # Skip header row
        # Pad row with empty strings if it's shorter than headers
        while len(row) < len(headers):
            row.append("")
        
        processed_row = {}
        
        # Map each column according to the configuration
        for our_column, sheet_column in column_mapping.items():
            if sheet_column in headers:
                column_index = headers.index(sheet_column)
                if column_index < len(row):
                    processed_row[our_column] = row[column_index].strip()
                else:
                    processed_row[our_column] = ""
            else:
                print(f"Warning: Column '{sheet_column}' not found in sheet headers")
                processed_row[our_column] = ""
        
        processed_data.append(processed_row)
    
    print(f"Processed {len(processed_data)} data rows")
    return processed_data

def save_csv(data: List[Dict[str, str]], output_file: str, column_order: List[str]) -> None:
    """
    Save processed data to CSV file.
    
    Args:
        data: Processed data rows
        output_file: Output file path
        column_order: Order of columns in output file
    """
    os.makedirs(os.path.dirname(output_file), exist_ok=True)
    
    with open(output_file, 'w', newline='', encoding='utf-8') as csvfile:
        writer = csv.DictWriter(csvfile, fieldnames=column_order)
        writer.writeheader()
        writer.writerows(data)
    
    print(f"Saved {len(data)} rows to {output_file}")

def main() -> None:
    """
    Main function to download and process car mapping data.
    
    Loads configuration, authenticates with Google Sheets API,
    downloads data, processes it, and saves to CSV file.
    """
    # Load configuration
    config = load_config()
    
    # Authenticate with Google Sheets
    service = authenticate_google_sheets(config['google_credentials_file'])
    
    # First, let's see what sheets are available
    sheet_names = get_sheet_names(service, config['spreadsheet_id'])
    print(f"Available sheets in the spreadsheet: {sheet_names}")
    
    # Download sheet data
    try:
        raw_data = download_sheet_data(
            service, 
            config['spreadsheet_id'], 
            config['range_name']
        )
    except Exception as e:
        print(f"Error with range '{config['range_name']}': {e}")
        if sheet_names:
            suggested_range = f"{sheet_names[0]}!A:Z"
            print(f"Try updating your config file with range_name: \"{suggested_range}\"")
        return
    
    if not raw_data:
        print("No data to process")
        return
    
    # Process the data
    processed_data = process_data(raw_data, config['column_mapping'])
    
    # Generate output filename
    today = datetime.now().strftime('%Y-%m-%d')
    output_dir = os.path.join(os.path.dirname(__file__), '..', 'output')
    output_file = os.path.join(output_dir, f'car-to-sticker-mapping-{today}.csv')
    
    # Save to CSV
    save_csv(processed_data, output_file, config['output_columns'])
    
    print(f"Car-to-sticker mapping successfully downloaded and saved to {output_file}")

if __name__ == '__main__':
    main()
