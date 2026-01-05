using System.Net;

namespace HR_Application.Models.Enums;

public static class ErrorCodes
{
    public enum EmployeeErrors
    {
        UserNotFound = 1001,

        UserAlreadyExists = 1002,

        UserInactive = 1003,

        InvalidEmployeeData = 1004,

        InvalidEmail = 1005
    }

    public enum AuthErrors
    {
        InvalidCredentials = 2001,

        Unauthorized = 2002,

        TokenMissing = 2003,

        TokenExpired = 2004,

        TokenInvalid = 2005,

        AccountLocked = 2006,

        InsufficientPermissions = 2007
    }

    public enum CommonErrors
    {
        InternalServerError = 3001,

        ValidationError = 3002,

        RequiredFieldMissing = 3003,

        DatabaseError = 3004,

        ResourceNotFound = 3005,

        BadRequest = 3006
    }

    public static HttpStatusCode GetHttpStatusCode(Enum errorCode)
    {
        return errorCode switch
        {
            // Employee Errors
            EmployeeErrors.UserNotFound => HttpStatusCode.NotFound,
            EmployeeErrors.UserAlreadyExists => HttpStatusCode.Conflict,
            EmployeeErrors.UserInactive => HttpStatusCode.Forbidden,
            EmployeeErrors.InvalidEmployeeData => HttpStatusCode.BadRequest,
            EmployeeErrors.InvalidEmail => HttpStatusCode.BadRequest,

            // Auth Errors
            AuthErrors.InvalidCredentials => HttpStatusCode.Unauthorized,
            AuthErrors.Unauthorized => HttpStatusCode.Forbidden,
            AuthErrors.TokenMissing => HttpStatusCode.Unauthorized,
            AuthErrors.TokenExpired => HttpStatusCode.Unauthorized,
            AuthErrors.TokenInvalid => HttpStatusCode.Unauthorized,
            AuthErrors.AccountLocked => HttpStatusCode.Locked,
            AuthErrors.InsufficientPermissions => HttpStatusCode.Forbidden,

            // Common Errors
            CommonErrors.InternalServerError => HttpStatusCode.InternalServerError,
            CommonErrors.ValidationError => HttpStatusCode.BadRequest,
            CommonErrors.RequiredFieldMissing => HttpStatusCode.BadRequest,
            CommonErrors.DatabaseError => HttpStatusCode.InternalServerError,
            CommonErrors.ResourceNotFound => HttpStatusCode.NotFound,
            CommonErrors.BadRequest => HttpStatusCode.BadRequest,

            // Default fallback
            _ => HttpStatusCode.InternalServerError
        };
    }

    public static string GetErrorMessage(Enum errorCode)
    {
        return errorCode switch
        {
            // Employee Errors
            EmployeeErrors.UserNotFound => "The requested employee was not found.",
            EmployeeErrors.UserAlreadyExists => "An employee with this email already exists.",
            EmployeeErrors.UserInactive => "This employee account is inactive.",
            EmployeeErrors.InvalidEmployeeData => "The employee data provided is invalid.",
            EmployeeErrors.InvalidEmail => "The email format is invalid.",

            // Auth Errors
            AuthErrors.InvalidCredentials => "Invalid email or password.",
            AuthErrors.Unauthorized => "You are not authorized to perform this action.",
            AuthErrors.TokenMissing => "Authentication token is missing.",
            AuthErrors.TokenExpired => "Your session has expired. Please login again.",
            AuthErrors.TokenInvalid => "Invalid authentication token.",
            AuthErrors.AccountLocked => "Your account has been locked due to too many failed login attempts.",
            AuthErrors.InsufficientPermissions => "You do not have permission to access this resource.",

            // Common Errors
            CommonErrors.InternalServerError => "An unexpected error occurred. Please try again later.",
            CommonErrors.ValidationError => "The request contains validation errors.",
            CommonErrors.RequiredFieldMissing => "Required field is missing from the request.",
            CommonErrors.DatabaseError => "A database error occurred. Please try again later.",
            CommonErrors.ResourceNotFound => "The requested resource was not found.",
            CommonErrors.BadRequest => "The request is invalid or malformed.",

            // Default fallback
            _ => "An unknown error occurred."
        };
    }
}
