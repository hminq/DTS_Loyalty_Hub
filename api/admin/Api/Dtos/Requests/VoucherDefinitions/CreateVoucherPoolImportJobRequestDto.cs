namespace Api.Dtos.Requests.VoucherDefinitions;

/// <summary>Submits an uploaded voucher-code CSV for provisioning.</summary>
public sealed record CreateVoucherPoolImportJobRequestDto(string ImportFileKey);
