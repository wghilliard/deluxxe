### Agent Implementation Specification

#### 1. Goal

Convert the Python script `scripts/create-event.py` into a new .NET CLI worker, `CreateEventCliWorker`, within the `DeluxxeCli` project. This worker will automate the creation of event configurations, mirroring the functionality of the existing Python script.

#### 2. New CLI Command

A new command `create-event` will be added to `DeluxxeCli`.

**Command:** `create-event`

**Arguments:**

*   `--event-name` (Required): The name for the new event (e.g., "2025-07-19-summer-classic").
*   `--output-dir` (Required): The base directory where event folders are stored (typically `output/`).
*   `--mylaps-account-id` (Optional): The MYLAPS account ID for querying the SpeedHive API.
*   `--event-id` (Optional): A specific SpeedHive event ID to use, bypassing the search for the latest event.

#### 3. `CreateEventCliWorker` Implementation

A new file `src/DeluxxeCli/CreateEventCliWorker.cs` will be created.

**Class:** `CreateEventCliWorker`

*   Inherits from `Microsoft.Extensions.Hosting.BackgroundService`.
*   Will be registered for dependency injection.

**Dependencies (injected via constructor):**

*   `ILogger<CreateEventCliWorker>`: For logging.
*   `CreateEventOptions`: A new class to hold the parsed command-line arguments.
*   `SpeedHiveService`: A new service to handle interactions with the SpeedHive API.
*   `CompletionToken`: To signal when the worker is finished.

**Core Logic (`ExecuteAsync` method):**

1.  **Directory Setup**:
    *   Create the main event directory structure inside the specified `output-dir`:
        *   `{event-name}/`
        *   `{event-name}/deluxxe/`
        *   `{event-name}/deluxxe/previous-results/`
        *   `{event-name}/collateral/`

2.  **SpeedHive Integration**:
    *   If `mylaps-account-id` is provided, use the `SpeedHiveService` to:
        *   Fetch the list of events.
        *   Find the latest event associated with the organizations "Cascade Sports Car Club" or "IRDC".
        *   Fetch the detailed event information, including race sessions for "Group 1".
    *   If `event-id` is provided, it will be used directly.

3.  **File Management**:
    *   Identify the most recent `car-to-sticker-mapping-*.csv` and `prize-descriptions-*.json` files in the `output-dir`.
    *   Create symbolic links to these two files inside the `{event-name}/deluxxe/` directory.

4.  **Configuration Generation**:
    *   Read the `output/event-template.json` file.
    *   Deserialize the JSON template into a C# object (e.g., a `JsonNode` or a custom class).
    *   Populate the configuration object with data obtained from SpeedHive (event ID, session IDs, race result URIs) and the names of the linked mapping/prize files.
    *   Serialize the populated configuration object back to a JSON string.
    *   Write the resulting JSON to `{event-name}/deluxxe/deluxxe.json`.

5.  **Link Previous Results**:
    *   Scan the `output-dir` for other completed event directories.
    *   For each found event, locate the `*-results.json` file.
    *   Create a symbolic link to each results file inside the new event's `{event-name}/deluxxe/previous-results/` directory.

6.  **Completion**:
    *   Log the successful creation of the event.
    *   Call `completionToken.Complete()` to terminate the CLI application.

#### 4. `SpeedHiveService` Implementation

A new service, `SpeedHiveService`, will be created to encapsulate all communication with the SpeedHive API.

**Class:** `SpeedHiveService`

**Methods:**

*   `Task<SpeedHiveEvent[]> GetEventsAsync(string mylapsAccountId)`: Fetches all events for a given account.
*   `Task<SpeedHiveEventDetails> GetEventDetailsAsync(int eventId)`: Fetches details for a specific event, including sessions.

This service will use `HttpClient` (configured via `IHttpClientFactory`) and will include the necessary `Origin` and `Referer` headers in its requests to the SpeedHive API.

#### 5. Project File Updates

*   **`src/DeluxxeCli/DeluxxeCli.csproj`**: Add a reference to the new `CreateEventCliWorker.cs` file.
*   **`src/DeluxxeCli/Program.cs`**:
    *   Register the `CreateEventCliWorker`, `SpeedHiveService`, and `CreateEventOptions`.
    *   Define the new `create-event` command, its arguments, and wire it to execute the `CreateEventCliWorker`.
