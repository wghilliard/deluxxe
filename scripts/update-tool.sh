#!/bin/bash
#
# update-tool.sh
#
# Script to build and update the DeluxxeCli .NET tool.
# 
# This script:
# 1. Packs the DeluxxeCli project into a NuGet package
# 2. Updates the tool from the local package source
# 3. Restores tool dependencies
#
# Usage: ./update-tool.sh
#
# Prerequisites:
# - .NET SDK installed
# - DeluxxeCli project properly configured
#

set -e  # Exit on any error

echo "Building and packing DeluxxeCli..."
dotnet pack

echo "Updating DeluxxeCli tool from local source..."
dotnet tool update --add-source src/DeluxxeCli/nupkg DeluxxeCli

echo "Restoring tool dependencies..."
dotnet tool restore

echo "Tool update completed successfully"