using Consumer.Core.Abstractions;
using Consumer.Core.Entities.Campaigns;
using Consumer.Core.Entities.Constants;
using Consumer.Core.Exceptions;
using Campaign.Contracts.Constants;
using Messaging.Contracts.Events;

namespace Consumer.Core.Services;

public sealed class CustomerAccountRegisteredEventRuntimeDefinition
    : ICampaignEventRuntimeDefinition
{
    private readonly ICustomerAccountRegisteredEventValidator _validator;

    public CustomerAccountRegisteredEventRuntimeDefinition(
        ICustomerAccountRegisteredEventValidator validator)
    {
        _validator = validator ?? throw new ArgumentNullException(nameof(validator));
    }

    public string EventType => EventTypeCodes.CustomerAccountRegistered;

    public IValidatedCampaignEvent ValidateDelivery(CampaignEventDelivery delivery)
    {
        ArgumentNullException.ThrowIfNull(delivery);

        return _validator.Validate(
            delivery.Body,
            delivery.MessageId,
            delivery.MessageType,
            delivery.RoutingKey);
    }

    public CampaignTargetResolution ResolveTarget(
        IValidatedCampaignEvent campaignEvent,
        string selector)
    {
        if (campaignEvent is not ValidatedCustomerAccountRegisteredEvent registrationEvent)
        {
            return new CampaignTargetResolution(
                CampaignTargetResolutionStatuses.InvalidEvent,
                CampaignTargetKinds.Customer,
                null,
                CampaignProcessingErrorCodes.EventPayloadInvalid);
        }

        if (selector == CustomerRegistrationTargetSelectors.EventCustomer)
        {
            return new CampaignTargetResolution(
                CampaignTargetResolutionStatuses.Resolved,
                CampaignTargetKinds.Customer,
                registrationEvent.CustomerId,
                null);
        }

        if (selector == CustomerRegistrationTargetSelectors.Referrer)
        {
            if (registrationEvent.Source == CustomerRegistrationSources.Normal)
            {
                return new CampaignTargetResolution(
                    CampaignTargetResolutionStatuses.NotApplicable,
                    CampaignTargetKinds.Customer,
                    null,
                    null);
            }

            if (!registrationEvent.ReferrerCustomerId.HasValue ||
                registrationEvent.ReferrerCustomerId.Value == Guid.Empty)
            {
                return new CampaignTargetResolution(
                    CampaignTargetResolutionStatuses.InvalidEvent,
                    CampaignTargetKinds.Customer,
                    null,
                    CampaignProcessingErrorCodes.EventReferrerInvalid);
            }

            return new CampaignTargetResolution(
                CampaignTargetResolutionStatuses.Resolved,
                CampaignTargetKinds.Customer,
                registrationEvent.ReferrerCustomerId.Value,
                null);
        }

        return new CampaignTargetResolution(
            CampaignTargetResolutionStatuses.Unsupported,
            string.Empty,
            null,
            CampaignProcessingErrorCodes.CampaignActionConfigurationInvalid);
    }

    public CampaignFactValue GetFact(
        IValidatedCampaignEvent campaignEvent,
        string fieldCode)
    {
        if (campaignEvent is not ValidatedCustomerAccountRegisteredEvent registrationEvent)
        {
            throw new CampaignEventValidationException(
                CampaignProcessingErrorCodes.EventPayloadInvalid);
        }

        if (!string.Equals(fieldCode, "source", StringComparison.Ordinal))
        {
            throw new CampaignConfigurationException(
                CampaignProcessingErrorCodes.CampaignConfigurationInvalid);
        }

        return new CampaignFactValue("source", registrationEvent.Source);
    }
}
