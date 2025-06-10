# Compute Odds Script

The `compute-odds.py` script analyzes raffle winner data to compute odds and analyze winner distribution patterns.

## Purpose

This script processes raffle winner CSV files to:
- Extract unique winners from raffle results
- Count winner occurrences
- TODO: Analyze against race results to compute odds

## Usage

```bash
python compute-odds.py -w path/to/winners.csv [-r path/to/race1.json path/to/race2.json ...]
```

### Arguments

- `-w, --winners` (required): Path to raffle winner CSV file
- `-r, --race-results` (optional): Paths to race result JSON files for odds computation

### Examples

```bash
# Basic winner analysis
python compute-odds.py -w ../output/2025-06-09-winners.csv

# With race results (when implemented)
python compute-odds.py -w ../output/2025-06-09-winners.csv -r ../output/race1.json ../output/race2.json
```

## Input Format

### Winner CSV Format
The winner CSV file should have the following structure:
```csv
Event,Prize,Winner,Details
2025-06-09,Prize 1,John Doe,Car #123
2025-06-09,Prize 2,Jane Smith,Car #456
```

The script expects the winner name to be in the third column (index 2).

### Race Results Format (Future)
Race result JSON files will contain race classification data for odds computation.

## Output

The script currently outputs:
- Number of unique winners
- List of all unique winner names

Future enhancements will include:
- Winner frequency analysis
- Odds calculations based on race participation
- Statistical analysis of winner distribution

## Error Handling

The script handles:
- Missing winner files
- Malformed CSV data
- File read errors

## Dependencies

- Python 3.7+
- Standard library modules only (no external dependencies)

## Notes

This script is currently in development. The race results analysis functionality is planned but not yet implemented.
