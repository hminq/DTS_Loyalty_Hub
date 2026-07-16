using Core.Abstractions;
using Core.UseCases.VoucherDefinitions.Results;
using MediatR;

namespace Core.UseCases.VoucherDefinitions.Commands;

public sealed record CreateVoucherDefinitionCommand(
    string? Code,
    string Name,
    string? Description,
    string? BannerImageUrl,
    string RewardType,
    decimal? RewardValue,
    string ValidityType,
    DateTime? ValidFrom,
    DateTime? ValidTo,
    int? DurationDay,
    string GenerationType,
    string PublishType,
    int TotalStock,
    Guid? ActorUserId) : IRequest<VoucherDefinitionResult>, ITransactionalRequest;
