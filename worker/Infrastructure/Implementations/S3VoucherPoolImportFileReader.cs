using System.Globalization;
using System.Runtime.CompilerServices;
using System.Text;
using Amazon.S3;
using Amazon.S3.Model;
using Core.Abstractions;
using Core.Entities;
using Core.Entities.Constants;
using Core.Exceptions;
using CsvHelper;
using CsvHelper.Configuration;
using Infrastructure.Options;

namespace Infrastructure.Implementations;

public sealed class S3VoucherPoolImportFileReader : IVoucherPoolImportFileReader
{
    private const string ExpectedHeader = "voucher_code";
    private readonly IAmazonS3 _s3Client;
    private readonly VoucherPoolImportOptions _options;

    public S3VoucherPoolImportFileReader(
        IAmazonS3 s3Client,
        VoucherPoolImportOptions options)
    {
        _s3Client = s3Client;
        _options = options;
    }

    public async IAsyncEnumerable<VoucherPoolImportRawRow> ReadAsync(
        string objectKey,
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        if (!IsAllowedKey(objectKey))
        {
            throw new VoucherPoolImportException(
                VoucherPoolGenerationErrorCodes.ImportStateInvalid);
        }

        GetObjectResponse response;
        try
        {
            response = await _s3Client.GetObjectAsync(
                _options.Bucket,
                objectKey,
                cancellationToken);
        }
        catch (AmazonS3Exception exception)
            when (exception.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            throw new VoucherPoolImportException(
                VoucherPoolGenerationErrorCodes.ImportFileNotFound);
        }
        catch (AmazonS3Exception)
        {
            throw new VoucherPoolImportException(
                VoucherPoolGenerationErrorCodes.ImportS3Error,
                retriable: true);
        }

        using (response)
        {
            if (response.ContentLength <= 0 ||
                response.ContentLength > _options.MaximumFileSizeBytes)
            {
                throw new VoucherPoolImportException(
                    VoucherPoolGenerationErrorCodes.ImportFileTooLarge);
            }

            using var streamReader = new StreamReader(
                response.ResponseStream,
                new UTF8Encoding(false, true),
                detectEncodingFromByteOrderMarks: true,
                leaveOpen: false);
            using var csv = new CsvReader(streamReader, new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                HasHeaderRecord = true,
                IgnoreBlankLines = false,
                TrimOptions = TrimOptions.None,
                BadDataFound = _ => throw new VoucherPoolImportException(
                    VoucherPoolGenerationErrorCodes.ImportCsvInvalid),
                MissingFieldFound = null,
                HeaderValidated = null
            });

            await ValidateHeaderAsync(csv);

            var rowNumber = 0;
            while (true)
            {
                cancellationToken.ThrowIfCancellationRequested();
                var (hasRow, code) = await ReadRowAsync(csv);
                if (!hasRow)
                {
                    break;
                }

                rowNumber++;
                yield return new VoucherPoolImportRawRow(rowNumber, code);
            }
        }
    }

    private static async Task ValidateHeaderAsync(CsvReader csv)
    {
        try
        {
            if (!await csv.ReadAsync() || !csv.ReadHeader())
            {
                throw new VoucherPoolImportException(
                    VoucherPoolGenerationErrorCodes.ImportHeaderInvalid);
            }

            var headers = csv.HeaderRecord;
            if (headers is null ||
                headers.Length != 1 ||
                !string.Equals(headers[0]?.Trim(), ExpectedHeader, StringComparison.Ordinal))
            {
                throw new VoucherPoolImportException(
                    VoucherPoolGenerationErrorCodes.ImportHeaderInvalid);
            }
        }
        catch (VoucherPoolImportException)
        {
            throw;
        }
        catch (Exception exception)
            when (exception is DecoderFallbackException or CsvHelperException)
        {
            throw new VoucherPoolImportException(
                VoucherPoolGenerationErrorCodes.ImportCsvInvalid);
        }
    }

    private static async Task<(bool HasRow, string? Code)> ReadRowAsync(CsvReader csv)
    {
        try
        {
            if (!await csv.ReadAsync())
            {
                return (false, null);
            }

            if (csv.Parser.Count != 1)
            {
                throw new VoucherPoolImportException(
                    VoucherPoolGenerationErrorCodes.ImportCsvInvalid);
            }

            return (true, csv.GetField(0));
        }
        catch (Exception exception)
            when (exception is DecoderFallbackException or CsvHelperException)
        {
            throw new VoucherPoolImportException(
                VoucherPoolGenerationErrorCodes.ImportCsvInvalid);
        }
    }

    private static bool IsAllowedKey(string objectKey)
    {
        return objectKey.StartsWith("voucher_defs/", StringComparison.Ordinal) &&
               objectKey.Contains("/imports/", StringComparison.Ordinal) &&
               objectKey.EndsWith(".csv", StringComparison.OrdinalIgnoreCase);
    }
}
