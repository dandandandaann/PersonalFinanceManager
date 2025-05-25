namespace SharedLibrary.Enum;

public enum ErrorCodeEnum
{
    // General
    None = 0, // Default for success
    UnknownError = 1,
    ServiceUnavailable = 2, // For when a downstream dependency fails
    InternalError = 3,

    // Input Validation
    ValidationFailed = 100,
    InvalidInput = 101,
    RequestBodyMissing = 107,

    // Resource Specific
    ResourceNotFound = 200,
    UserAlreadyExists = 201,
    // EmailAlreadyTaken = 202, // If you have unique email constraint
    // UsernameAlreadyTaken = 203, // If you have unique username constraint

    // Operation Specific
    ConfigurationUpdateFailed = 300,
    OperationNotPermitted = 301,

    // External Dependencies
    DatabaseError = 400, // Can be generic or more specific
    ThirdPartyServiceError = 401
}