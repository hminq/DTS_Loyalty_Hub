using System.Security.Cryptography;
using Core.Abstractions;

namespace Infrastructure.Implementations;

public sealed class CryptographicVoucherCodeGenerator : IVoucherCodeGenerator
{
    private const int RandomByteCount = 16;
    private const string Prefix = "VCH-";

    public string Generate()
    {
        Span<byte> bytes = stackalloc byte[RandomByteCount];
        RandomNumberGenerator.Fill(bytes);
        return string.Concat(Prefix, Convert.ToHexString(bytes));
    }
}
