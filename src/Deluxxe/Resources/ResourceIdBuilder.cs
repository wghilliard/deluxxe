using Deluxxe.Extensions;
using Deluxxe.Raffles;

namespace Deluxxe.Resources;

public class ResourceIdBuilder(IList<string>? parts = null, ResourcePartsTracker tracker = ResourcePartsTracker.None)
{
    private readonly IList<string> _parts = parts ?? new List<string>();
    private ResourcePartsTracker _tracker = tracker;
    private const char Delimiter = '/';

    private const string SeasonSegmentName = "season";
    private const string EventSegmentName = "event";
    private const string DrawingSegmentName = "drawing";
    private const string RoundSegmentName = "round";
    private const string PrizeSegmentName = "prize";

    public ResourceIdBuilder WithSeason(string season)
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
        _parts.Add(SeasonSegmentName);
        _parts.Add(season.Sanitize());

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
        _parts.Add(EventSegmentName);
        _parts.Add(NormalizeEventName(eventName));
        _parts.Add(eventId.Sanitize());

        return this;
    }

    public ResourceIdBuilder WithEventDrawingRound(string roundId)
    {
        return WithDrawingRound(string.Empty, string.Empty, roundId, DrawingType.Event);
    }

    public ResourceIdBuilder WithRaceDrawingRound(string sessionName, string sessionId, string roundId)
    {
        return WithDrawingRound(sessionName, sessionId, roundId, DrawingType.Race);
    }

    private ResourceIdBuilder WithDrawingRound(string sessionName, string sessionId, string roundId, DrawingType drawingType)
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
        _parts.Add(DrawingSegmentName);
        _parts.Add(drawingType.ToString().Sanitize());
        if (!string.IsNullOrWhiteSpace(sessionName))
        {
            _parts.Add(sessionName.Sanitize());
        }

        if (!string.IsNullOrWhiteSpace(sessionId))
        {
            _parts.Add(sessionId.Sanitize());
        }

        _parts.Add(RoundSegmentName);
        _parts.Add(roundId.Sanitize());

        return this;
    }

    public ResourceIdBuilder WithPrize(string sponsorName, string prizeSku)
    {
        if (_tracker.HasFlag(ResourcePartsTracker.Prize))
        {
            throw new ArgumentException("cannot add season twice");
        }

        _tracker |= ResourcePartsTracker.Prize;
        _parts.Add(PrizeSegmentName);
        _parts.Add(sponsorName.Sanitize());
        _parts.Add(prizeSku.Sanitize());

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

        return string.Join(Delimiter, realParts.Where(part => part is not null)).Sanitize();
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