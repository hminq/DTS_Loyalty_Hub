using Core.Abstractions;
using Core.UseCases.VoucherDefinitions.Queries;
using Core.UseCases.VoucherDefinitions.Results;
using MediatR;

namespace Core.UseCases.VoucherDefinitions.Handlers;

public sealed class GetVoucherImportTemplateQueryHandler
    : IRequestHandler<GetVoucherImportTemplateQuery, VoucherImportTemplateResult>
{
    private readonly IVoucherImportTemplateUrlProvider _urlProvider;

    public GetVoucherImportTemplateQueryHandler(
        IVoucherImportTemplateUrlProvider urlProvider)
    {
        _urlProvider = urlProvider;
    }

    public Task<VoucherImportTemplateResult> Handle(
        GetVoucherImportTemplateQuery request,
        CancellationToken cancellationToken)
    {
        var download = _urlProvider.CreateDownload();

        return Task.FromResult(new VoucherImportTemplateResult(
            download.DownloadUrl,
            download.FileName,
            download.ExpiresAt));
    }
}
