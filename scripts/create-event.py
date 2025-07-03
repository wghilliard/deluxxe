#!/usr/bin/env python3
"""
Enhanced create event script with SpeedHive integration.

This script creates event directory structures and configuration files
for Deluxxe raffle management, with automatic integration to SpeedHive
for race event data retrieval.
"""

import os
import json
import re
import requests
from argparse import ArgumentParser
from datetime import datetime
from pathlib import Path
from typing import Dict, List, Optional, Tuple, Any


def query_speedhive_events(mylaps_account_id: str) -> List[Dict[str, Any]]:
    """Query SpeedHive API for user's events."""
    url = f"https://eventresults-api.speedhive.com/api/v0.2.3/eventresults/accounts/{mylaps_account_id}/events"
    params = {
        "sportCategory": "Motorized",
        "count": 100
    }
    
    headers = {
        "Origin": "https://speedhive.mylaps.com",
        "Referer": "https://speedhive.mylaps.com/",
        "User-Agent": "Mozilla/5.0 (Macintosh; Intel Mac OS X 10.15; rv:133.0)"
    }
    
    try:
        response = requests.get(url, params=params, headers=headers)
        response.raise_for_status()
        return response.json()
    except requests.RequestException as e:
        print(f"Error querying SpeedHive events: {e}")
        return []


def query_speedhive_event_details(event_id: int) -> Optional[Dict[str, Any]]:
    """Query SpeedHive API for event details including sessions."""
    url = f"https://eventresults-api.speedhive.com/api/v0.2.3/eventresults/events/{event_id}"
    params = {"sessions": "true"}
    
    headers = {
        "Origin": "https://speedhive.mylaps.com",
        "Referer": "https://speedhive.mylaps.com/",
        "User-Agent": "Mozilla/5.0 (Macintosh; Intel Mac OS X 10.15; rv:133.0)"
    }
    
    try:
        response = requests.get(url, params=params, headers=headers)
        response.raise_for_status()
        return response.json()
    except requests.RequestException as e:
        print(f"Error querying SpeedHive event details: {e}")
        return None


def find_latest_event_by_organization(
    events: List[Dict[str, Any]], 
    organizations: List[str]
) -> Optional[Dict[str, Any]]:
    """Find the latest event by specified organizations."""
    filtered_events = [
        event for event in events 
        if event.get("organization", {}).get("name") in organizations
    ]
    
    if not filtered_events:
        return None
    
    # Sort by start date descending to get the latest
    filtered_events.sort(key=lambda x: x.get("startDate", ""), reverse=True)
    return filtered_events[0]


def extract_group1_race_sessions(event_details: Dict[str, Any]) -> List[Dict[str, Any]]:
    """Extract Group 1 race sessions from event details."""
    group1_races = []
    
    sessions = event_details.get("sessions", {})
    groups = sessions.get("groups", [])
    
    for group in groups:
        if group.get("name").lower() == "group 1":
            for session in group.get("sessions", []):
                if session.get("type").lower() == "race":
                    group1_races.append(session)
    
    return group1_races


def find_latest_files(output_dir: Path) -> Tuple[Optional[str], Optional[str]]:
    """Find the latest car-to-sticker mapping and prize descriptions files."""
    car_mapping_files = []
    prize_files = []
    
    for file in output_dir.iterdir():
        if file.is_file():
            if file.name.startswith("car-to-sticker-mapping-") and file.name.endswith(".csv"):
                car_mapping_files.append(file.name)
            elif file.name.startswith("prize-descriptions-") and file.name.endswith(".json"):
                prize_files.append(file.name)
    
    # Sort by filename (which includes date) to get latest
    car_mapping_latest = sorted(car_mapping_files)[-1] if car_mapping_files else None
    prize_latest = sorted(prize_files)[-1] if prize_files else None
    
    return car_mapping_latest, prize_latest


def create_deluxxe_config(
    template_path: Path,
    output_path: Path,
    event_name: str,
    event_id: int,
    race_sessions: List[Dict[str, Any]],
    car_mapping_file: Optional[str],
    prize_file: Optional[str]
) -> None:
    """Create deluxxe.json config from template with substitutions."""
    with open(template_path, 'r') as f:
        template_content = f.read()
    
    # Extract year from event name or use current year
    year_match = re.search(r'(\d{4})', event_name)
    season = year_match.group(1) if year_match else str(datetime.now().year)
    
    # Prepare the normalized event name
    normalized_event_name = event_name.lower().replace(' ', '-')
    
    # If the event name already starts with the season, extract just the event part
    # for the eventName field, but keep the full name for the directory
    event_name_only = normalized_event_name
    if normalized_event_name.startswith(f"{season}-"):
        event_name_only = normalized_event_name[len(f"{season}-"):]
    
    # Basic substitutions
    replacements = {
        "{season}": season,
        "{event-name}": normalized_event_name,  # This keeps the full name for the top-level name field
        "{speedhive-event-id}": str(event_id)
    }
    
    # Update file references
    if car_mapping_file:
        replacements['"file://local/car-to-sticker-mapping-{latest}.csv"'] = f'"file://local/{car_mapping_file}"'
    if prize_file:
        replacements['"file://local/prize-descriptions-{latest}.json"'] = f'"file://local/{prize_file}"'
    
    # Apply basic replacements
    for placeholder, value in replacements.items():
        template_content = template_content.replace(placeholder, value)
    
    # Handle race session replacements if we have session data
    if len(race_sessions) >= 2:
        # Replace Saturday session placeholders
        template_content = template_content.replace(
            '"{sat-session-id}"', f'"{race_sessions[0]["id"]}"'
        )
        template_content = template_content.replace(
            '"{sat-race-result-uri}"', 
            f'"https://eventresults-api.speedhive.com/api/v0.2.3/eventresults/sessions/{race_sessions[0]["id"]}/classification"'
        )
        
        # Replace Sunday session placeholders
        template_content = template_content.replace(
            '"{sun-session-id}"', f'"{race_sessions[1]["id"]}"'
        )
        template_content = template_content.replace(
            '"{sun-race-result-uri}"',
            f'"https://eventresults-api.speedhive.com/api/v0.2.3/eventresults/sessions/{race_sessions[1]["id"]}/classification"'
        )
    elif len(race_sessions) == 1:
        # For single race events, convert to single race format
        template_content = re.sub(
            r'"raceResults": \[.*?\]',
            f'"raceResults": [{{"sessionName": "race", "sessionId": "{race_sessions[0]["id"]}", "raceResultUri": "https://eventresults-api.speedhive.com/api/v0.2.3/eventresults/sessions/{race_sessions[0]["id"]}/classification"}}]',
            template_content,
            flags=re.DOTALL
        )
    else:
        # No race sessions available, leave placeholders for manual replacement
        print("Warning: No race sessions found, leaving session placeholders for manual replacement")
    
    # Update eventName field to contain only the event name without season prefix
    template_content = re.sub(
        r'"eventName": ".*?"',
        f'"eventName": "{event_name_only}"',
        template_content
    )
    
    # Write the config file
    with open(output_path, 'w') as f:
        f.write(template_content)


def main(
    event_name: str, 
    output_dir: Path, 
    mylaps_account_id: Optional[str] = None, 
    event_id: Optional[int] = None
) -> None:
    """
    Main function to create event structure and configuration.
    
    Args:
        event_name: Name of the event to create
        output_dir: Base output directory
        mylaps_account_id: Optional MYLAPS account ID for SpeedHive integration
        event_id: Optional specific event ID to use
    """
    event_path = output_dir.joinpath(event_name)
    collateral_path = event_path.joinpath("collateral")
    deluxxe_path = event_path.joinpath("deluxxe")
    previous_results_path = deluxxe_path.joinpath("previous-results")

    paths = [event_path, collateral_path, deluxxe_path, previous_results_path]

    for path in paths:
        os.makedirs(path, exist_ok=True)

    # Query SpeedHive for event data if account ID provided
    race_sessions = []
    actual_event_id = event_id
    
    if mylaps_account_id:
        print(f"Querying SpeedHive for events...")
        events = query_speedhive_events(mylaps_account_id)
        
        if events:
            # Find latest event by Cascade Sports Car Club or IRDC
            target_orgs = ["Cascade Sports Car Club", "IRDC"]
            latest_event = find_latest_event_by_organization(events, target_orgs)
            
            if latest_event:
                print(f"Found latest event: {latest_event['name']} (ID: {latest_event['id']})")
                actual_event_id = latest_event["id"]
                
                # Get detailed event information
                event_details = query_speedhive_event_details(actual_event_id)
                if event_details:
                    race_sessions = extract_group1_race_sessions(event_details)
                    print(f"Found {len(race_sessions)} Group 1 race sessions")
            else:
                print("No events found from target organizations")
        else:
            print("No events returned from SpeedHive")
    
    # Find latest support files
    car_mapping_file, prize_file = find_latest_files(output_dir)
    print(f"Latest car mapping file: {car_mapping_file}")
    print(f"Latest prize descriptions file: {prize_file}")
    
    # Create soft links to latest files
    if car_mapping_file:
        src_path = output_dir.joinpath(car_mapping_file)
        link_path = deluxxe_path.joinpath(car_mapping_file)
        if not link_path.exists():
            os.symlink(src_path, link_path)
            print(f"Linked {car_mapping_file}")
    
    if prize_file:
        src_path = output_dir.joinpath(prize_file)
        link_path = deluxxe_path.joinpath(prize_file)
        if not link_path.exists():
            os.symlink(src_path, link_path)
            print(f"Linked {prize_file}")
    
    # Copy and customize event template
    template_path = output_dir.joinpath("event-template.json")
    config_path = deluxxe_path.joinpath("deluxxe.json")
    
    if template_path.exists() and actual_event_id:
        create_deluxxe_config(
            template_path, 
            config_path, 
            event_name, 
            actual_event_id, 
            race_sessions,
            car_mapping_file,
            prize_file
        )
        print(f"Created deluxxe.json config")
    elif not template_path.exists():
        print(f"Warning: Template file {template_path} not found")
    elif not actual_event_id:
        print("Warning: No event ID available for config creation")

    print(f"searching for events in {output_dir}")
    for maybe_event in os.listdir(output_dir):
        if output_dir.joinpath(maybe_event).is_dir():
            maybe_event_deluxxe_path = output_dir.joinpath(maybe_event).joinpath("deluxxe")
            if maybe_event_deluxxe_path.exists() and "test" not in maybe_event:
                print(f"found results dir in {maybe_event_deluxxe_path}")
                for maybe_results_file in os.listdir(maybe_event_deluxxe_path):
                    if maybe_results_file.endswith("-results.json") and "=" not in maybe_results_file:
                        src_link_path = maybe_event_deluxxe_path.joinpath(maybe_results_file)
                        result_link_path = previous_results_path.joinpath(maybe_results_file)
                        print(f"linking {src_link_path} to {result_link_path}")
                        if not result_link_path.is_symlink():
                            os.symlink(src_link_path, result_link_path)
                        else:
                            print(f"{result_link_path} already exists")
        else:
            print(f"skipping {maybe_event}")


if __name__ == "__main__":
    parser = ArgumentParser()
    parser.add_argument("-o", "--output-dir", dest="output_dir", required=True,
                       help="Output directory path")
    parser.add_argument("-n", "--event-name", dest="event_name", required=True,
                       help="Name of the event")
    parser.add_argument("-a", "--mylaps-account", dest="mylaps_account_id",
                       help="MYLAPS account ID for SpeedHive API queries")
    parser.add_argument("-e", "--event-id", dest="event_id", type=int,
                       help="Specific SpeedHive event ID to use")

    args = parser.parse_args()

    main(args.event_name, Path(os.path.abspath(args.output_dir)), 
         args.mylaps_account_id, args.event_id)
