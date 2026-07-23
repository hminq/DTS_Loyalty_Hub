namespace Core.Abstractions;

public interface IBannerReadUrlProvider
{
    string CreateReadUrl(string objectKey);
}
