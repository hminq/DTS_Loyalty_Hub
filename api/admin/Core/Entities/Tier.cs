using Core.Exceptions;

namespace Core.Entities;

public class Tier
{
    private const int MinNameLength = 3;
    private const int MaxNameLength = 49;
    private const int MinCycleMonth = 1;
    private const int MaxCycleMonth = 12;

    private Tier(
        Guid tierConfigId,
        string name,
        decimal pointsRequired,
        int cycleMonth,
        int priority,
        DateTime createdAt)
    {
        TierConfigId = tierConfigId;
        Name = name;
        PointsRequired = pointsRequired;
        CycleMonth = cycleMonth;
        Priority = priority;
        CreatedAt = createdAt;
    }

    public Guid TierConfigId { get; private set; }

    public string Name { get; private set; }

    public decimal PointsRequired { get; private set; }

    public int CycleMonth { get; private set; }

    public int Priority { get; private set; }

    public DateTime CreatedAt { get; private set; }

    public static Tier Create(
        string name,
        decimal pointsRequired,
        int cycleMonth,
        int priority)
    {
        ValidateName(name);
        ValidateCycleMonth(cycleMonth);
        ValidatePointsRequired(pointsRequired);
        ValidatePriority(priority);

        return new Tier(
            Guid.NewGuid(),
            name.Trim(),
            pointsRequired,
            cycleMonth,
            priority,
            DateTime.UtcNow);
    }

    public static Tier Restore(
        Guid tierConfigId,
        string name,
        decimal pointsRequired,
        int cycleMonth,
        int priority,
        DateTime createdAt)
    {
        if (tierConfigId == Guid.Empty)
        {
            throw ValidationError("TIER_ID_REQUIRED");
        }

        ValidateName(name);
        ValidateCycleMonth(cycleMonth);
        ValidatePointsRequired(pointsRequired);
        ValidatePriority(priority);

        return new Tier(
            tierConfigId,
            name.Trim(),
            pointsRequired,
            cycleMonth,
            priority,
            createdAt);
    }

    public void Update(
        string name,
        decimal pointsRequired,
        int cycleMonth,
        int priority)
    {
        ValidateName(name);
        ValidateCycleMonth(cycleMonth);
        ValidatePointsRequired(pointsRequired);
        ValidatePriority(priority);

        Name = name.Trim();
        PointsRequired = pointsRequired;
        CycleMonth = cycleMonth;
        Priority = priority;
    }

    private static void ValidateName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw ValidationError("TIER_NAME_REQUIRED");
        }

        var length = name.Trim().Length;

        if (length < MinNameLength || length > MaxNameLength)
        {
            throw ValidationError("TIER_NAME_LENGTH_INVALID");
        }
    }

    private static void ValidateCycleMonth(int cycleMonth)
    {
        if (cycleMonth < MinCycleMonth || cycleMonth > MaxCycleMonth)
        {
            throw ValidationError("TIER_CYCLE_MONTH_INVALID");
        }
    }

    private static void ValidatePointsRequired(decimal pointsRequired)
    {
        if (pointsRequired < 0)
        {
            throw ValidationError("TIER_POINTS_REQUIRED_INVALID");
        }
    }

    private static void ValidatePriority(int priority)
    {
        if (priority <= 0)
        {
            throw ValidationError("TIER_PRIORITY_INVALID");
        }
    }

    private static DomainException ValidationError(string errorCode)
    {
        return new DomainException(errorCode, DomainErrorType.Validation);
    }
}