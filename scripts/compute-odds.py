import os
from argparse import ArgumentParser
from pathlib import Path
import json
from typing import List

def main(winners_path: Path, race_result_paths: List[Path]) -> None:

    winners = set()
    with winners_path.open() as handle:
        handle.readline() # skip header
        for line in handle.readlines():
            driver_name = line.strip().split(',')[2]
            winners.add(driver_name)

    print(len(winners))
    print(winners)
    # eligible_winners = {}
    # for race in race_result_paths:
    #     contents = json.loads(race.read_text())



if __name__ == "__main__":
    parser = ArgumentParser()
    parser.add_argument("-w", "--winners", required=True, help="input raffle winner csv path")
    # parser.add_argument("-r", "--race-results", required=True, help="input race results paths", nargs="+")

    args = parser.parse_args()
    main(Path(args.winners), None)