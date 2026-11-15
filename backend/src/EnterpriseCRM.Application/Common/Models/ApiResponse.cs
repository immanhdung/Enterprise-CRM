namespace EnterpriseCRM.Application.Common.Models;

public class ApiResponse<T>
{
    public bool Success { get; set; }
    public T? Data { get; set; }
    public string? Message { get; set; }
    public List<string>? Errors { get; set; }
    public int StatusCode { get; set; } = 200;
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    public static ApiResponse<T> Succeeded(T data, string? message = null, int statusCode = 200)
    {
        return new ApiResponse<T>
        {
            Success = true,
            Data = data,
            Message = message,
            StatusCode = statusCode,
            Errors = null,
            Timestamp = DateTime.UtcNow
        };
    }

    public static ApiResponse<T> SuccessResponse(T data, string? message = null, int statusCode = 200)
    {
        return Succeeded(data, message, statusCode);
    }

    public static ApiResponse<T> Failed(List<string> errors, string? message = null, int statusCode = 400)
    {
        return new ApiResponse<T>
        {
            Success = false,
            Data = default,
            Message = message,
            StatusCode = statusCode,
            Errors = errors,
            Timestamp = DateTime.UtcNow
        };
    }

    public static ApiResponse<T> Failed(string error, string? message = null, int statusCode = 400)
    {
        return Failed(new List<string> { error }, message, statusCode);
    }

    public static ApiResponse<T> FailureResponse(string error, int statusCode = 400, string? message = null)
    {
        return Failed(error, message ?? error, statusCode);
    }

    public static ApiResponse<T> FailureResponse(List<string> errors, int statusCode = 400, string? message = null)
    {
        return Failed(errors, message, statusCode);
    }
}

public class PaginatedList<T>
{
    public IReadOnlyCollection<T> Items { get; }
    public int PageIndex { get; }
    public int TotalPages { get; }
    public int TotalCount { get; }
    public int PageSize { get; }

    public PaginatedList(IReadOnlyCollection<T> items, int count, int pageIndex, int pageSize)
    {
        PageIndex = pageIndex;
        PageSize = pageSize;
        TotalPages = (int)Math.Ceiling(count / (double)pageSize);
        TotalCount = count;
        Items = items;
    }

    public bool HasPreviousPage => PageIndex > 1;
    public bool HasNextPage => PageIndex < TotalPages;
}
