namespace Consumer.Core.Abstractions;

public interface ICampaignEventRuntimeRegistry
{
    ICampaignEventRuntimeDefinition GetRequired(string? eventType);
}
