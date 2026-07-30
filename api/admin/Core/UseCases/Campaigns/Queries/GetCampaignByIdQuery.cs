using Core.UseCases.Campaigns.Results;
using MediatR;

namespace Core.UseCases.Campaigns.Queries;

public sealed record GetCampaignByIdQuery(Guid CampaignId) : IRequest<CampaignDetailResult>;
