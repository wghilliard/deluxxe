
# Implementation Plan: Render Race Results as PDF

## 1. Overview

This document outlines the plan to implement a new feature that renders a race results webpage from `speedhive.mylaps.com` as a PDF and saves it to disk. This will be accomplished using a headless browser to ensure that all dynamic content is properly rendered.

## 2. Dependencies

To achieve this, we will introduce a new dependency:

*   **Microsoft.Playwright**: This package will be used to control a headless browser, which will handle the rendering of the webpage.

This package will need to be added to the `Deluxxe.csproj` file.

## 3. Implementation Details

The implementation will be split between `SpeedHiveClient` and `RaceResultsService`.

### 3.1. `SpeedHiveClient` Modifications

A new method will be added to `SpeedHiveClient` to handle the interaction with the headless browser.

**New Method:** `public async Task<byte[]> GetResultsAsPdfAsync(Uri uiUrl, CancellationToken token = default)`

**Logic:**

1.  **Initialize Playwright**: A new `Playwright` instance will be created.
2.  **Launch Browser**: A headless browser instance will be launched.
3.  **Create Page**: A new page will be created within the browser.
4.  **Navigate**: The page will navigate to the provided `uiUrl`.
5.  **Wait for Content**: The method will wait for the page to be fully loaded to ensure all JavaScript has executed.
6.  **Generate PDF**: Playwright's built-in `Page.PdfAsync()` function will be called to generate the PDF as a byte array.
7.  **Cleanup**: The browser and Playwright instances will be disposed of to free up resources.

### 3.2. `RaceResultsService` Modifications

A new method will be added to `RaceResultsService` to orchestrate the process and handle file I/O, following existing patterns in the class.

**New Method:** `public async Task<FileInfo> SaveResultsAsPdfAsync(Uri raceResultUiUrl, CancellationToken cancellationToken)`

**Logic:**

1.  **Generate Filename**: A unique filename will be generated for the PDF. To maintain consistency with the existing caching mechanism, the filename will be derived from a SHA256 hash of the `raceResultUiUrl`, e.g., `{hash}-race-results.pdf`.
2.  **Check for Existing File**: The method will check if a PDF with the generated filename already exists in the `serializerOptions.outputDirectory`.
    *   If it exists, a `FileInfo` object for the existing file will be returned.
3.  **Fetch and Save PDF**:
    *   If the file does not exist, the new `speedHiveClient.GetResultsAsPdfAsync()` method will be called to get the PDF byte array.
    *   The byte array will be written to a new file at the designated path.
4.  **Return FileInfo**: A `FileInfo` object for the newly created file will be returned.

## 4. Project File Update

The following package reference will be added to `src/Deluxxe/Deluxxe.csproj`:

```xml
<ItemGroup>
  ...
  <PackageReference Include="Microsoft.Playwright" Version="1.43.0" />
  ...
</ItemGroup>
```
