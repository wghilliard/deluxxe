namespace DeluxxeCli;

public class CreateEventOptions
{
    public required string EventName { get; set; }

    public required string Date { get; set; }

    public required string OutputDir { get; set; }
    public string? MylapsAccountId { get; set; }
    public int? EventId { get; set; }
}