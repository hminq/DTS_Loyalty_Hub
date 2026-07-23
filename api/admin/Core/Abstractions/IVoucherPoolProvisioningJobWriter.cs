using Core.Entities;

namespace Core.Abstractions;

public interface IVoucherPoolProvisioningJobWriter
{
    VoucherPoolProvisioningJob Add(VoucherPoolProvisioningJob job);
}
