#!/usr/bin/env python3
"""
Script to compute odds and analyze raffle winners against race results.

This script processes raffle winner data to calculate odds and analyze
winner distribution patterns.
"""

import os
import json
from argparse import ArgumentParser
from pathlib import Path
from typing import List, Optional, Set


def main(winners_path: Path, race_result_paths: Optional[List[Path]]) -> None:
    """
    Analyze raffle winners and compute odds.
    
    Args:
        winners_path: Path to CSV file containing raffle winners
        race_result_paths: Optional list of paths to race result JSON files
    """
    winners: Set[str] = set()
    
    try:
        with winners_path.open() as handle:
            handle.readline()  # skip header
            for line in handle.readlines():
                driver_name = line.strip().split(',')[2]
                winners.add(driver_name)
    except (FileNotFoundError, IndexError) as e:
        print(f"Error reading winners file: {e}")
        return

    print(f"Found {len(winners)} unique winners")
    print(f"Winners: {winners}")
    
    # TODO: Implement analysis with race results
    # eligible_winners = {}
    # for race in race_result_paths or []:
    #     contents = json.loads(race.read_text())


if __name__ == "__main__":
    parser = ArgumentParser(description="Compute odds and analyze raffle winners")
    parser.add_argument(
        "-w", "--winners", 
        required=True, 
        help="Path to raffle winner CSV file"
    )
    parser.add_argument(
        "-r", "--race-results", 
        nargs="*", 
        help="Paths to race result JSON files"
    )

    args = parser.parse_args()
    
    race_paths = [Path(p) for p in args.race_results] if args.race_results else None
    main(Path(args.winners), race_paths)