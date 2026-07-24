using Core.Entities.Constants;
using Core.Exceptions;

namespace Core.Entities;

public class VoucherDefinition
{
    private const int MaxCodeLength = 200;
    private const int MaxNameLength = 200;

    private VoucherDefinition(
        Guid voucherDefinitionId,
        string? code,
        string name,
        string? description,
        string? bannerImageUrl,
        string rewardType,
        decimal? rewardValue,
        string validityType,
        DateTime? validFrom,
        DateTime? validTo,
        int? durationDay,
        string generationType,
        string publishType,
        int totalStock,
        int remainingStock,
        DateTime createdAt,
        DateTime? deletedAt)
    {
        VoucherDefinitionId = voucherDefinitionId;
        Code = code;
        Name = name;
        Description = description;
        BannerImageUrl = bannerImageUrl;
        RewardType = rewardType;
        RewardValue = rewardValue;
        ValidityType = validityType;
        ValidFrom = validFrom;
        ValidTo = validTo;
        DurationDay = durationDay;
        GenerationType = generationType;
        PublishType = publishType;
        TotalStock = totalStock;
        RemainingStock = remainingStock;
        CreatedAt = createdAt;
        DeletedAt = deletedAt;
    }

    public Guid VoucherDefinitionId { get; private set; }

    public string? Code { get; private set; }

    public string Name { get; private set; }

    public string? Description { get; private set; }

    public string? BannerImageUrl { get; private set; }

    public string RewardType { get; private set; }

    public decimal? RewardValue { get; private set; }

    public string ValidityType { get; private set; }

    public DateTime? ValidFrom { get; private set; }

    public DateTime? ValidTo { get; private set; }

    public int? DurationDay { get; private set; }

    public string GenerationType { get; private set; }

    public string PublishType { get; private set; }

    public int TotalStock { get; private set; }

    public int RemainingStock { get; private set; }

    public DateTime CreatedAt { get; private set; }

    public DateTime? DeletedAt { get; private set; }

    public static VoucherDefinition Create(
        string? code,
        string name,
        string? description,
        string? bannerImageUrl,
        string rewardType,
        decimal? rewardValue,
        string validityType,
        DateTime? validFrom,
        DateTime? validTo,
        int? durationDay,
        string generationType,
        string publishType,
        int totalStock,
        DateTime now)
    {
        Validate(
            code,
            name,
            bannerImageUrl,
            rewardType,
            rewardValue,
            validityType,
            validFrom,
            validTo,
            durationDay,
            generationType,
            publishType,
            totalStock,
            now);

        var normalizedPublishType = VoucherPublishTypes.Normalize(publishType);
        var remainingStock = normalizedPublishType == VoucherPublishTypes.Public
            ? totalStock
            : 0;

        return new VoucherDefinition(
            Guid.NewGuid(),
            NormalizeOptional(code),
            name.Trim(),
            NormalizeOptional(description),
            NormalizeOptional(bannerImageUrl),
            VoucherRewardTypes.Normalize(rewardType),
            rewardValue,
            VoucherValidityTypes.Normalize(validityType),
            validFrom,
            validTo,
            durationDay,
            VoucherGenerationTypes.Normalize(generationType),
            normalizedPublishType,
            totalStock,
            remainingStock,
            now,
            null);
    }

    public static VoucherDefinition Restore(
        Guid voucherDefinitionId,
        string? code,
        string name,
        string? description,
        string? bannerImageUrl,
        string rewardType,
        decimal? rewardValue,
        string validityType,
        DateTime? validFrom,
        DateTime? validTo,
        int? durationDay,
        string generationType,
        string publishType,
        int totalStock,
        int remainingStock,
        DateTime createdAt,
        DateTime? deletedAt)
    {
        if (voucherDefinitionId == Guid.Empty)
        {
            throw ValidationError("VOUCHER_DEFINITION_ID_REQUIRED");
        }

        Validate(
            code,
            name,
            bannerImageUrl,
            rewardType,
            rewardValue,
            validityType,
            validFrom,
            validTo,
            durationDay,
            generationType,
            publishType,
            totalStock,
            null);

        ValidateRemainingStock(totalStock, remainingStock);

        return new VoucherDefinition(
            voucherDefinitionId,
            NormalizeOptional(code),
            name.Trim(),
            NormalizeOptional(description),
            NormalizeOptional(bannerImageUrl),
            VoucherRewardTypes.Normalize(rewardType),
            rewardValue,
            VoucherValidityTypes.Normalize(validityType),
            validFrom,
            validTo,
            durationDay,
            VoucherGenerationTypes.Normalize(generationType),
            VoucherPublishTypes.Normalize(publishType),
            totalStock,
            remainingStock,
            createdAt,
            deletedAt);
    }

    private static void Validate(
        string? code,
        string name,
        string? bannerImageUrl,
        string rewardType,
        decimal? rewardValue,
        string validityType,
        DateTime? validFrom,
        DateTime? validTo,
        int? durationDay,
        string generationType,
        string publishType,
        int totalStock,
        DateTime? now)
    {
        ValidateName(name);
        ValidateBannerImageUrl(bannerImageUrl);
        ValidateGenerationType(generationType);
        ValidatePublishType(publishType, code);
        ValidateGenerationTypeForPublishType(generationType, publishType);
        ValidateRewardType(rewardType, rewardValue);
        ValidateValidityType(validityType, validFrom, validTo, durationDay, now);
        ValidateTotalStock(totalStock, generationType);
    }

    private static void ValidateBannerImageUrl(string? bannerImageUrl)
    {
        if (string.IsNullOrWhiteSpace(bannerImageUrl))
        {
            return;
        }

        if (!bannerImageUrl.Trim().StartsWith(BannerUploadTypes.VoucherDefinitionBannerPrefix, StringComparison.Ordinal))
        {
            throw ValidationError("VOUCHER_BANNER_IMAGE_KEY_INVALID");
        }
    }

    private static void ValidateName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw ValidationError("VOUCHER_DEFINITION_NAME_REQUIRED");
        }

        if (name.Trim().Length > MaxNameLength)
        {
            throw ValidationError("VOUCHER_DEFINITION_NAME_TOO_LONG");
        }
    }

    private static void ValidateGenerationType(string generationType)
    {
        if (string.IsNullOrWhiteSpace(generationType))
        {
            throw ValidationError("VOUCHER_GENERATION_TYPE_REQUIRED");
        }

        if (!VoucherGenerationTypes.IsDefined(generationType))
        {
            throw ValidationError("VOUCHER_GENERATION_TYPE_INVALID");
        }
    }

    private static void ValidatePublishType(string publishType, string? code)
    {
        if (string.IsNullOrWhiteSpace(publishType))
        {
            throw ValidationError("VOUCHER_PUBLISH_TYPE_REQUIRED");
        }

        if (!VoucherPublishTypes.IsDefined(publishType))
        {
            throw ValidationError("VOUCHER_PUBLISH_TYPE_INVALID");
        }

        var normalizedPublishType = VoucherPublishTypes.Normalize(publishType);

        if (normalizedPublishType == VoucherPublishTypes.Public && string.IsNullOrWhiteSpace(code))
        {
            throw ValidationError("VOUCHER_CODE_REQUIRED");
        }

        if (normalizedPublishType == VoucherPublishTypes.Private && !string.IsNullOrWhiteSpace(code))
        {
            throw ValidationError("VOUCHER_CODE_MUST_BE_EMPTY");
        }

        if (!string.IsNullOrWhiteSpace(code) && code.Trim().Length > MaxCodeLength)
        {
            throw ValidationError("VOUCHER_CODE_TOO_LONG");
        }
    }

    private static void ValidateGenerationTypeForPublishType(
        string generationType,
        string publishType)
    {
        var normalizedGenerationType = VoucherGenerationTypes.Normalize(generationType);
        var normalizedPublishType = VoucherPublishTypes.Normalize(publishType);

        if (normalizedPublishType == VoucherPublishTypes.Public &&
            normalizedGenerationType != VoucherGenerationTypes.None)
        {
            throw ValidationError("VOUCHER_PUBLIC_GENERATION_TYPE_INVALID");
        }

        if (normalizedPublishType == VoucherPublishTypes.Private &&
            normalizedGenerationType == VoucherGenerationTypes.None)
        {
            throw ValidationError("VOUCHER_PRIVATE_GENERATION_TYPE_INVALID");
        }
    }

    private static void ValidateRewardType(string rewardType, decimal? rewardValue)
    {
        if (string.IsNullOrWhiteSpace(rewardType))
        {
            throw ValidationError("VOUCHER_REWARD_TYPE_REQUIRED");
        }

        if (!VoucherRewardTypes.IsDefined(rewardType))
        {
            throw ValidationError("VOUCHER_REWARD_TYPE_INVALID");
        }

        var normalizedRewardType = VoucherRewardTypes.Normalize(rewardType);

        if (normalizedRewardType == VoucherRewardTypes.Gift && rewardValue.HasValue)
        {
            throw ValidationError("VOUCHER_REWARD_VALUE_MUST_BE_EMPTY");
        }

        if (normalizedRewardType == VoucherRewardTypes.Fixed || normalizedRewardType == VoucherRewardTypes.Percent)
        {
            if (!rewardValue.HasValue)
            {
                throw ValidationError("VOUCHER_REWARD_VALUE_REQUIRED");
            }

            if (normalizedRewardType == VoucherRewardTypes.Fixed && rewardValue.Value <= 0)
            {
                throw ValidationError("VOUCHER_FIXED_REWARD_VALUE_INVALID");
            }

            if (normalizedRewardType == VoucherRewardTypes.Percent && (rewardValue.Value <= 0 || rewardValue.Value > 100))
            {
                throw ValidationError("VOUCHER_PERCENT_REWARD_VALUE_INVALID");
            }
        }
    }

    private static void ValidateValidityType(
        string validityType,
        DateTime? validFrom,
        DateTime? validTo,
        int? durationDay,
        DateTime? now)
    {
        if (string.IsNullOrWhiteSpace(validityType))
        {
            throw ValidationError("VOUCHER_VALIDITY_TYPE_REQUIRED");
        }

        if (!VoucherValidityTypes.IsDefined(validityType))
        {
            throw ValidationError("VOUCHER_VALIDITY_TYPE_INVALID");
        }

        var normalizedValidityType = VoucherValidityTypes.Normalize(validityType);

        if (now.HasValue && validFrom.HasValue && validFrom.Value <= now.Value)
        {
            throw ValidationError("VOUCHER_VALID_FROM_NOT_FUTURE");
        }

        if (normalizedValidityType == VoucherValidityTypes.Fixed)
        {
            if (!validFrom.HasValue || !validTo.HasValue)
            {
                throw ValidationError("VOUCHER_FIXED_VALIDITY_REQUIRED");
            }

            if (validTo.Value < validFrom.Value.AddMinutes(30))
            {
                throw ValidationError("VOUCHER_VALIDITY_RANGE_INVALID");
            }
        }

        if (normalizedValidityType == VoucherValidityTypes.Dynamic)
        {
            if (!validFrom.HasValue || !durationDay.HasValue)
            {
                throw ValidationError("VOUCHER_DYNAMIC_VALIDITY_REQUIRED");
            }

            if (durationDay.Value <= 0)
            {
                throw ValidationError("VOUCHER_DURATION_DAY_INVALID");
            }
        }
    }

    private static void ValidateTotalStock(int totalStock, string generationType)
    {
        if (totalStock <= 0 || totalStock > VoucherDefinitionLimits.MaxTotalStock)
        {
            throw ValidationError("VOUCHER_TOTAL_STOCK_INVALID");
        }

        if (VoucherGenerationTypes.Normalize(generationType) == VoucherGenerationTypes.Imported &&
            totalStock > VoucherDefinitionLimits.MaxImportedTotalStock)
        {
            throw ValidationError("VOUCHER_IMPORTED_TOTAL_STOCK_INVALID");
        }
    }

    private static void ValidateRemainingStock(int totalStock, int remainingStock)
    {
        if (remainingStock < 0 || remainingStock > totalStock)
        {
            throw ValidationError("VOUCHER_REMAINING_STOCK_INVALID");
        }
    }

    private static string? NormalizeOptional(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }

    private static DomainException ValidationError(string errorCode)
    {
        return new DomainException(errorCode, DomainErrorType.Validation);
    }
}
