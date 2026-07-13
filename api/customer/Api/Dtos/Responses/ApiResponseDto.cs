namespace Api.Dtos.Responses;

public sealed class ApiResponseDto<T>
{
    public T Data { get; set; } = default!;

    public ApiMetaDto? Meta { get; set; }
}

public sealed class ApiMetaDto
{
    public int Page { get; set; }

    public int PageSize { get; set; }

    public int TotalItems { get; set; }

    public int TotalPages { get; set; }
}
