namespace Core.Exceptions;

public sealed class EventPublicationConfigurationException : Exception
{
    public EventPublicationConfigurationException(string eventType, int eventVersion)
        : base($"Published event definition is unavailable for {eventType} v{eventVersion}.")
    {
        EventType = eventType;
        EventVersion = eventVersion;
    }

    public string EventType { get; }

    public int EventVersion { get; }
}
