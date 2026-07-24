using Core.Abstractions;
using Core.Entities;
using Persistence.Models.Context;
using PersistenceVoucherPoolProvisioningJob = Persistence.Models.VoucherPoolProvisioningJob;

namespace Infrastructure.Implementations;

public sealed class VoucherPoolProvisioningJobWriter : IVoucherPoolProvisioningJobWriter
{
    private readonly LoyaltyHubDbContext _dbContext;

    public VoucherPoolProvisioningJobWriter(LoyaltyHubDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public VoucherPoolProvisioningJob Add(VoucherPoolProvisioningJob job)
    {
        _dbContext.VoucherPoolProvisioningJobs.Add(new PersistenceVoucherPoolProvisioningJob
        {
            JobId = job.JobId,
            VoucherDefId = job.VoucherDefinitionId,
            JobType = job.JobType,
            ImportFileKey = job.ImportFileKey,
            ExpectedCount = job.ExpectedCount,
            ProcessedCount = job.ProcessedCount,
            Status = job.Status,
            AttemptCount = job.AttemptCount,
            CreatedBy = job.CreatedBy,
            CreatedAt = job.CreatedAt
        });

        return job;
    }
}
