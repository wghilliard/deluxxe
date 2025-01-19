using System.Runtime.InteropServices;
using Deluxxe.Raffles;

namespace Deluxxe.Resources;

public class ResourceIdBuilder(IList<string>? parts = null, ResourcePartsTracker tracker = ResourcePartsTracker.None)
{
    private readonly IList<string> _parts = parts ?? new List<string>();
    private ResourcePartsTracker _tracker = tracker;
    private const char Delimiter = '/';

    public ResourceIdBuilder WithSeason(int season)
    {
        if (!_tracker.HasFlag(ResourcePartsTracker.None))
        {
            throw new ArgumentException("unknown initialization state");
        }

        if (_tracker.HasFlag(ResourcePartsTracker.Season))
        {
            throw new ArgumentException("cannot add season twice");
        }

        _tracker |= ResourcePartsTracker.Season & ~ResourcePartsTracker.None;
        _parts.Add("season");
        _parts.Add(Sanitize(season.ToString()));

        return this;
    }

    public ResourceIdBuilder WithEvent(string eventName, string eventId)
    {
        if (_tracker.HasFlag(ResourcePartsTracker.Event))
        {
            throw new ArgumentException("cannot add event twice");
        }

        if (!_tracker.HasFlag(ResourcePartsTracker.Season))
        {
            throw new ArgumentException("season must be set");
        }

        _tracker |= ResourcePartsTracker.Event;
        _parts.Add("event");
        _parts.Add(NormalizeEventName(eventName));
        _parts.Add(Sanitize(eventId));

        return this;
    }

    public ResourceIdBuilder WithEventDrawing(string drawingId)
    {
        return WithDrawing(string.Empty, drawingId, DrawingType.Event);
    }

    public ResourceIdBuilder WithRaceDrawing(string sessionName, string drawingId)
    {
        return WithDrawing(sessionName, drawingId, DrawingType.Race);
    }

    private ResourceIdBuilder WithDrawing(string sessionName, string drawingId, DrawingType drawingType)
    {
        if (_tracker.HasFlag(ResourcePartsTracker.Drawing))
        {
            throw new ArgumentException("cannot add drawing twice");
        }

        if (!_tracker.HasFlag(ResourcePartsTracker.Season))
        {
            throw new ArgumentException("season must be set");
        }

        if (!_tracker.HasFlag(ResourcePartsTracker.Event))
        {
            throw new ArgumentException("event must be set");
        }

        _tracker |= ResourcePartsTracker.Drawing;
        _parts.Add("drawing");
        _parts.Add(Sanitize(drawingType.ToString()));
        if (!string.IsNullOrWhiteSpace(sessionName))
        {
            _parts.Add(Sanitize(sessionName));
        }

        _parts.Add(Sanitize(drawingId));

        return this;
    }

    public ResourceIdBuilder WithPrize(string sponsorName, string prizeSku)
    {
        if (_tracker.HasFlag(ResourcePartsTracker.Prize))
        {
            throw new ArgumentException("cannot add season twice");
        }

        _tracker |= ResourcePartsTracker.Prize;
        _parts.Add("prize");
        _parts.Add(Sanitize(sponsorName));
        _parts.Add(Sanitize(prizeSku));

        return this;
    }

    public ResourceIdBuilder Copy()
    {
        var newParts = new List<string>();
        newParts.AddRange(_parts);

        return new ResourceIdBuilder(newParts, _tracker);
    }

    public string Build()
    {
        return string.Join(Delimiter, _parts);
    }

    private static string Sanitize(string input)
    {
        return input
            .Replace(' ', '-')
            .Replace('/', '-')
            .Replace('\\', '-')
            .ToLower()
            .Trim();
    }

    public static string NormalizeEventName(string eventName)
    {
        string[] maybeParts = eventName.Split(' ');
        string?[] realParts = new string[maybeParts.Length];
        for (var index = 0; index < maybeParts.Length; index++)
        {
            if (!string.IsNullOrWhiteSpace(maybeParts[index])
                && maybeParts[index] != "-")
            {
                realParts[index] = maybeParts[index];
            }
        }

        return Sanitize(string.Join(Delimiter, realParts.Where(part => part is not null)));
    }
}

[Flags]
public enum ResourcePartsTracker
{
    None = 0,
    Season = 1,
    Event = 1 << 2,
    Drawing = 1 << 3,
    Prize = 1 << 4,
}