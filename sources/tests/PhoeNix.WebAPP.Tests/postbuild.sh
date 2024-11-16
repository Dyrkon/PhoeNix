#!/bin/sh

# Define the base directory
BASE_DIR="./bin/Debug"

# Dynamically find the .NET version and platform-specific directory
NET_VERSION_DIR=$(find "$BASE_DIR" -maxdepth 1 -type d -name 'net*' | head -n 1)
PLATFORM_DIR=$(find "$NET_VERSION_DIR/.playwright/node" -maxdepth 1 -type d -name 'linux-*' | head -n 1)

# Define the path to the generated file
FILE_PATH="$PLATFORM_DIR/playwright.sh"

# Replace "$SCRIPT_PATH/node" with "$(which node)"
sed -i 's|\$SCRIPT_PATH/node|$(which node)|g' "$FILE_PATH"

echo "Replacement done in file: $FILE_PATH"

