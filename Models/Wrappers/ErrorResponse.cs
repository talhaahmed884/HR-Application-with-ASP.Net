using System.Net;
using HR_Application.Models.Enums;

namespace HR_Application.Models.Wrappers;

public class ErrorResponse
{
    public bool Success { get; set; } = false;
    public int StatusCode { get; set; }
    public int ErrorCode { get; set; }
    public string Message { get; set; } = string.Empty;
    public string? Details { get; set; }
    public Dictionary<string, string[]>? ValidationErrors { get; set; }
    public DateTime Timestamp { get; set; }

    public static ErrorResponse FromErrorCode(Enum errorCode, string? details = null)
    {
        var httpStatusCode = ErrorCodes.GetHttpStatusCode(errorCode);
        var message = ErrorCodes.GetErrorMessage(errorCode);

        return new ErrorResponse
        {
            Success = false,
            StatusCode = (int)httpStatusCode,
            ErrorCode = Convert.ToInt32(errorCode),
            Message = message,
            Details = details,
            Timestamp = DateTime.UtcNow
        };
    }

    public static ErrorResponse FromMessage(HttpStatusCode statusCode, string message, int errorCode = 0, string? details = null)
    {
        return new ErrorResponse
        {
            Success = false,
            StatusCode = (int)statusCode,
            ErrorCode = errorCode,
            Message = message,
            Details = details,
            Timestamp = DateTime.UtcNow
        };
    }

    public static ErrorResponse ValidationError(Dictionary<string, string[]> validationErrors)
    {
        return new ErrorResponse
        {
            Success = false,
            StatusCode = (int)HttpStatusCode.BadRequest,
            ErrorCode = (int)ErrorCodes.CommonErrors.ValidationError,
            Message = "One or more validation errors occurred.",
            ValidationErrors = validationErrors,
            Timestamp = DateTime.UtcNow
        };
    }
}
