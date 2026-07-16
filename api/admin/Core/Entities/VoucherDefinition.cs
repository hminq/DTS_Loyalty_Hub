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
        int totalStock)
    {
        Validate(
            code,
            name,
            rewardType,
            rewardValue,
            validityType,
            validFrom,
            validTo,
            durationDay,
            generationType,
            publishType,
            totalStock);

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
            VoucherPublishTypes.Normalize(publishType),
            totalStock,
            totalStock,
            DateTime.UtcNow,
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
            throw ValidationError("VOUCHER_DEFINITION_ID_REQUIRED", "Voucher definition id is required.");
        }

        Validate(
            code,
            name,
            rewardType,
            rewardValue,
            validityType,
            validFrom,
            validTo,
            durationDay,
            generationType,
            publishType,
            totalStock);

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
        string rewardType,
        decimal? rewardValue,
        string validityType,
        DateTime? validFrom,
        DateTime? validTo,
        int? durationDay,
        string generationType,
        string publishType,
        int totalStock)
    {
        ValidateName(name);
        ValidateGenerationType(generationType);
        ValidatePublishType(publishType, code);
        ValidateGenerationTypeForPublishType(generationType, publishType);
        ValidateRewardType(rewardType, rewardValue);
        ValidateValidityType(validityType, validFrom, validTo, durationDay);
        ValidateTotalStock(totalStock);
    }

    private static void ValidateName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw ValidationError("VOUCHER_DEFINITION_NAME_REQUIRED", "Voucher definition name is required.");
        }

        if (name.Trim().Length > MaxNameLength)
        {
            throw ValidationError("VOUCHER_DEFINITION_NAME_TOO_LONG", "Voucher definition name is too long.");
        }
    }

    private static void ValidateGenerationType(string generationType)
    {
        if (string.IsNullOrWhiteSpace(generationType))
        {
            throw ValidationError("VOUCHER_GENERATION_TYPE_REQUIRED", "Voucher generation type is required.");
        }

        if (!VoucherGenerationTypes.IsDefined(generationType))
        {
            throw ValidationError("VOUCHER_GENERATION_TYPE_INVALID", "Voucher generation type is invalid.");
        }
    }

    private static void ValidatePublishType(string publishType, string? code)
    {
        if (string.IsNullOrWhiteSpace(publishType))
        {
            throw ValidationError("VOUCHER_PUBLISH_TYPE_REQUIRED", "Voucher publish type is required.");
        }

        if (!VoucherPublishTypes.IsDefined(publishType))
        {
            throw ValidationError("VOUCHER_PUBLISH_TYPE_INVALID", "Voucher publish type is invalid.");
        }

        var normalizedPublishType = VoucherPublishTypes.Normalize(publishType);

        if (normalizedPublishType == VoucherPublishTypes.Public && string.IsNullOrWhiteSpace(code))
        {
            throw ValidationError("VOUCHER_CODE_REQUIRED", "Voucher code is required for public vouchers.");
        }

        if (normalizedPublishType == VoucherPublishTypes.Private && !string.IsNullOrWhiteSpace(code))
        {
            throw ValidationError("VOUCHER_CODE_MUST_BE_EMPTY", "Voucher code must be empty for private vouchers.");
        }

        if (!string.IsNullOrWhiteSpace(code) && code.Trim().Length > MaxCodeLength)
        {
            throw ValidationError("VOUCHER_CODE_TOO_LONG", "Voucher code is too long.");
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
            throw ValidationError(
                "VOUCHER_PUBLIC_GENERATION_TYPE_INVALID",
                "Public vouchers must use NONE generation type.");
        }

        if (normalizedPublishType == VoucherPublishTypes.Private &&
            normalizedGenerationType == VoucherGenerationTypes.None)
        {
            throw ValidationError(
                "VOUCHER_PRIVATE_GENERATION_TYPE_INVALID",
                "Private vouchers cannot use NONE generation type.");
        }
    }

    private static void ValidateRewardType(string rewardType, decimal? rewardValue)
    {
        if (string.IsNullOrWhiteSpace(rewardType))
        {
            throw ValidationError("VOUCHER_REWARD_TYPE_REQUIRED", "Voucher reward type is required.");
        }

        if (!VoucherRewardTypes.IsDefined(rewardType))
        {
            throw ValidationError("VOUCHER_REWARD_TYPE_INVALID", "Voucher reward type is invalid.");
        }

        var normalizedRewardType = VoucherRewardTypes.Normalize(rewardType);

        if (normalizedRewardType == VoucherRewardTypes.Gift && rewardValue.HasValue)
        {
            throw ValidationError("VOUCHER_REWARD_VALUE_MUST_BE_EMPTY", "Reward value must be empty for gift vouchers.");
        }
    }

    private static void ValidateValidityType(
        string validityType,
        DateTime? validFrom,
        DateTime? validTo,
        int? durationDay)
    {
        if (string.IsNullOrWhiteSpace(validityType))
        {
            throw ValidationError("VOUCHER_VALIDITY_TYPE_REQUIRED", "Voucher validity type is required.");
        }

        if (!VoucherValidityTypes.IsDefined(validityType))
        {
            throw ValidationError("VOUCHER_VALIDITY_TYPE_INVALID", "Voucher validity type is invalid.");
        }

        var normalizedValidityType = VoucherValidityTypes.Normalize(validityType);

        if (normalizedValidityType == VoucherValidityTypes.Fixed)
        {
            if (!validFrom.HasValue || !validTo.HasValue)
            {
                throw ValidationError("VOUCHER_FIXED_VALIDITY_REQUIRED", "Valid from and valid to are required for fixed validity.");
            }

            if (validFrom.Value >= validTo.Value)
            {
                throw ValidationError("VOUCHER_VALIDITY_RANGE_INVALID", "Valid from must be earlier than valid to.");
            }
        }

        if (normalizedValidityType == VoucherValidityTypes.Dynamic)
        {
            if (!validFrom.HasValue || !durationDay.HasValue)
            {
                throw ValidationError("VOUCHER_DYNAMIC_VALIDITY_REQUIRED", "Valid from and duration day are required for dynamic validity.");
            }

            if (durationDay.Value <= 0)
            {
                throw ValidationError("VOUCHER_DURATION_DAY_INVALID", "Duration day must be greater than zero.");
            }
        }
    }

    private static void ValidateTotalStock(int totalStock)
    {
        if (totalStock < 0)
        {
            throw ValidationError("VOUCHER_TOTAL_STOCK_INVALID", "Total stock cannot be negative.");
        }
    }

    private static void ValidateRemainingStock(int totalStock, int remainingStock)
    {
        if (remainingStock < 0 || remainingStock > totalStock)
        {
            throw ValidationError(
                "VOUCHER_REMAINING_STOCK_INVALID",
                "Remaining stock must be between zero and total stock.");
        }
    }

    private static string? NormalizeOptional(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }

    private static DomainException ValidationError(string errorCode, string message)
    {
        return new DomainException(errorCode, message, DomainErrorType.Validation);
    }
}
