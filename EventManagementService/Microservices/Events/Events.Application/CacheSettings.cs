namespace Events.Application;

public sealed class CacheSettings
{
    public int EventTtlMinutes { get; set; } = 10;
    public int TopEventsTtlMinutes { get; set; } = 5;
}
