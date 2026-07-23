namespace Api.DTOs;

public class ApiResponse<T>
{
    public T? Data { get; set; }
    public PaginationMeta? Meta { get; set; }

    public static ApiResponse<T> Success(T data) => new() { Data = data };
    
    public static ApiResponse<T> Success(T data, int page, int pageSize, int totalCount) => new()
    {
        Data = data,
        Meta = new PaginationMeta
        {
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount,
            TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize)
        }
    };
}

public class PaginationMeta
{
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalCount { get; set; }
    public int TotalPages { get; set; }
}

public class ApiErrorResponse
{
    public ApiError Error { get; set; } = new();

    public static ApiErrorResponse Create(string code, string message, List<FieldError>? details = null) => new()
    {
        Error = new ApiError
        {
            Code = code,
            Message = message,
            Details = details
        }
    };
}

public class ApiError
{
    public string Code { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public List<FieldError>? Details { get; set; }
}

public class FieldError
{
    public string Field { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
}
