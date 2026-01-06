using System.Net;

namespace HR_Application.Model.Wrappers;

public class ApiResponse<T>
{
    public bool Success { get; set; }
    public int StatusCode { get; set; }
    public string? Message { get; set; }
    public T? Data { get; set; }
    public DateTime Timestamp { get; set; }

    public static ApiResponse<T> SuccessResponse(T data, string? message = null, HttpStatusCode statusCode = HttpStatusCode.OK)
    {
        return new ApiResponse<T>
        {
            Success = true,
            StatusCode = (int)statusCode,
            Message = message ?? "Request completed successfully",
            Data = data,
            Timestamp = DateTime.UtcNow
        };
    }

    public static ApiResponse<T> SuccessResponse(string message, HttpStatusCode statusCode = HttpStatusCode.OK)
    {
        return new ApiResponse<T>
        {
            Success = true,
            StatusCode = (int)statusCode,
            Message = message,
            Data = default,
            Timestamp = DateTime.UtcNow
        };
    }
}
