#!/bin/bash
#
# backup-output.sh
# 
# Simple backup script to copy the entire output directory to a backup location.
# This preserves file timestamps and permissions (-p flag).
#
# Usage: ./backup-output.sh
#
# Note: Update the destination path to match your backup location.
#

set -e  # Exit on any error

SOURCE_DIR="./output/"
BACKUP_DIR="/Volumes/data/m365/pro3/deluxxe-outputs"

if [[ ! -d "$SOURCE_DIR" ]]; then
    echo "Error: Source directory $SOURCE_DIR does not exist"
    exit 1
fi

if [[ ! -d "$BACKUP_DIR" ]]; then
    echo "Error: Backup directory $BACKUP_DIR does not exist or is not accessible"
    exit 1
fi

echo "Backing up $SOURCE_DIR to $BACKUP_DIR..."
cp -Rp "$SOURCE_DIR" "$BACKUP_DIR"
echo "Backup completed successfully"