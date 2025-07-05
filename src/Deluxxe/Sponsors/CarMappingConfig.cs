
using System.Text.Json.Serialization;

namespace Deluxxe.Sponsors;

public class CarMappingConfig
{
    [JsonPropertyName("spreadsheet_id")]
    public string SpreadsheetId { get; set; } = string.Empty;

    [JsonPropertyName("range_name")]
    public string RangeName { get; set; } = string.Empty;

    [JsonPropertyName("column_mapping")]
    public Dictionary<string, string> ColumnMapping { get; set; } = [];

    [JsonPropertyName("output_columns")]
    public List<string> OutputColumns { get; set; } = [];

    [JsonPropertyName("google_credentials_file")]
    public string GoogleCredentialsFile { get; set; } = string.Empty;
}
