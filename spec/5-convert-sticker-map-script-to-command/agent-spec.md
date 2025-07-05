# Agent Implementation Specification: `download-sticker-map` Command

This document outlines the plan for converting the Python script `scripts/download-car-mapping.py` into a new C# command-line worker within the `DeluxxeCli` project.

## 1. Overview

The new command, `download-sticker-map`, will provide the functionality of the existing Python script, enabling users to download and process car-to-sticker mapping data directly from a configured Google Sheet. It will be implemented as a new worker, following the architectural patterns of the existing `ValidateDriversCliWorker`.

## 2. New Components

### 2.1. `DownloadStickerMapOptions.cs`

A new class will be created to define the command-line interface for this operation.

- **File Path**: `src/DeluxxeCli/DownloadStickerMapOptions.cs`
- **Purpose**: To define and parse command-line arguments for the `download-sticker-map` command.
- **Attributes**:
    - `[Verb("download-sticker-map", HelpText = "Downloads the car-to-sticker mapping from Google Sheets.")]`
    - `[Option('c', "config", Required = true, HelpText = "Path to the car mapping JSON configuration file.")]`: Path to a JSON file containing Google Sheets details.
    - `[Option('o', "output-dir", Required = true, HelpText = "Directory to save the output CSV file.")]`: The directory where the output file will be saved.
    - `[Option('d', "date", HelpText = "Date for the output filename (YYYY-MM-DD). Defaults to today.")]`: Optional date for the output filename.

### 2.2. `CarMappingConfig.cs`

A new class to model the JSON configuration file.

- **File Path**: `src/Deluxxe/Sponsors/CarMappingConfig.cs`
- **Purpose**: To deserialize `car-mapping-config.json` into a strongly-typed C# object.
- **Properties**:
    - `SpreadsheetId`: `string`
    - `RangeName`: `string`
    - `ColumnMapping`: `Dictionary<string, string>`
    - `OutputColumns`: `List<string>`

### 2.3. `GoogleSheetService.cs`

A new service class responsible for all interactions with the Google Sheets API.

- **File Path**: `src/Deluxxe/Google/GoogleSheetService.cs`
- **Purpose**: To abstract the complexities of Google API authentication and data fetching.
- **Dependencies**: `ILogger<GoogleSheetService>`, `ActivitySource`.
- **Key Methods**:
    - `AuthenticateAsync(string credentialsPath, string tokenPath)`: Handles OAuth 2.0 authentication, creating and refreshing `token.json`. This will encapsulate the logic from the Python script's `authenticate_google_sheets` function.
    - `DownloadSheetDataAsync(string spreadsheetId, string rangeName)`: Fetches the raw data from the specified sheet and range.

### 2.4. `DownloadStickerMapCliWorker.cs`

The main worker class that orchestrates the download and processing.

- **File Path**: `src/DeluxxeCli/DownloadStickerMapCliWorker.cs`
- **Inherits**: `BackgroundService`
- **Dependencies**:
    - `ILogger<DownloadStickerMapCliWorker>`
    - `ActivitySource`
    - `CompletionToken`
    - `DownloadStickerMapOptions`
    - `GoogleSheetService`
- **Logic (`ExecuteAsync`)**:
    1.  Start a new `Activity` for tracing.
    2.  Deserialize the JSON config file specified in `DownloadStickerMapOptions.ConfigPath`.
    3.  Instantiate `GoogleSheetService`.
    4.  Call `GoogleSheetService.AuthenticateAsync` to get an authorized `SheetsService` client. The paths for `credentials.json` and `token.json` will be resolved relative to the config file's location.
    5.  Call `GoogleSheetService.DownloadSheetDataAsync` to get the data.
    6.  Process the downloaded data:
        -   Map columns according to `CarMappingConfig.ColumnMapping`.
        -   This logic will be ported from the `process_data` function in the Python script.
    7.  Save the processed data to a CSV file:
        -   The filename will be `car-to-sticker-mapping-YYYY-MM-DD.csv`, using the date from the options or the current date.
        -   The file will be saved in the directory specified by `DownloadStickerMapOptions.OutputDir`.
        -   The columns will be ordered according to `CarMappingConfig.OutputColumns`.
    8.  Log success or failure messages.
    9.  Call `completionToken.Complete()`.

## 3. Modifications to Existing Files

### 3.1. `Program.cs`

- **File Path**: `src/DeluxxeCli/Program.cs`
- **Changes**:
    -   Register the new services (`GoogleSheetService`) and the worker in the dependency injection container.
    -   Add a new `MapResult` entry for `DownloadStickerMapOptions` to execute the `DownloadStickerMapCliWorker`.

### 3.2. `Deluxxe.csproj` and `DeluxxeCli.csproj`

- **File Path**: `src/Deluxxe/Deluxxe.csproj`, `src/DeluxxeCli/DeluxxeCli.csproj`
- **Changes**:
    -   Add the required Google API NuGet package: `Google.Apis.Sheets.v4`.

## 4. Implementation Steps

1.  **Create `CarMappingConfig.cs`**: Define the data model for the configuration.
2.  **Add NuGet Package**: Update the `.csproj` files with the Google Sheets dependency.
3.  **Implement `GoogleSheetService.cs`**: Build the service for authentication and data fetching.
4.  **Create `DownloadStickerMapOptions.cs`**: Define the CLI verb and options.
5.  **Implement `DownloadStickerMapCliWorker.cs`**: Write the core orchestration logic.
6.  **Update `Program.cs`**: Integrate the new command and services into the application host.
7.  **Testing**: Manually run the new command, providing a valid `car-mapping-config.json` and ensuring the output CSV is generated correctly.
