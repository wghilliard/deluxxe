"""
Script to update the version in a .csproj file.
"""

import argparse
import re
import os

def update_version_in_csproj(file_path):
    """
    Reads a .csproj file, increments the patch version of the Version attribute,
    and writes the changes back to the file.

    Args:
        file_path (str): The path to the .csproj file.
    """
    if not os.path.exists(file_path):
        print(f"Error: File not found at {file_path}")
        return

    try:
        with open(file_path, 'r', encoding='utf-8') as file:
            content = file.read()

        # Regex to find <Version>x.y.z</Version>
        # It captures the major, minor, and patch versions.
        version_pattern = re.compile(r'(<Version>)(\d+)\.(\d+)\.(\d+)(</Version>)')
        match = version_pattern.search(content)

        if not match:
            print(f"Error: Could not find <Version> tag in {file_path}")
            # Attempt to find if VersionPrefix is used
            version_prefix_pattern = re.compile(r'(<VersionPrefix>)(\d+)\.(\d+)\.(\d+)(</VersionPrefix>)')
            match = version_prefix_pattern.search(content)
            if not match:
                print(f"Error: Could not find <VersionPrefix> tag either in {file_path}")
                return
            else:
                print(f"Found <VersionPrefix> tag in {file_path}")
                tag_name = "VersionPrefix"

        else:
            tag_name = "Version"

        major = int(match.group(2))
        minor = int(match.group(3))
        patch = int(match.group(4))

        # Increment the patch version
        new_patch = patch + 1
        new_version_string = f"{major}.{minor}.{new_patch}"
        print(f"Updating version from {major}.{minor}.{patch} to {new_version_string}")

        # Replace the old version string with the new one
        if tag_name == "Version":
            new_content = version_pattern.sub(rf'\g<1>{new_version_string}\g<5>', content)
        else:
            new_content = version_prefix_pattern.sub(rf'\g<1>{new_version_string}\g<5>', content)

        with open(file_path, 'w', encoding='utf-8') as file:
            file.write(new_content)

        print(f"Successfully updated version in {file_path} to {new_version_string}")

    except Exception as e:
        print(f"An error occurred: {e}")

if __name__ == "__main__":
    parser = argparse.ArgumentParser(description="Update the patch version in a .csproj file.")
    parser.add_argument("file_path", help="The path to the .csproj file to update.")

    args = parser.parse_args()
    update_version_in_csproj(args.file_path)
