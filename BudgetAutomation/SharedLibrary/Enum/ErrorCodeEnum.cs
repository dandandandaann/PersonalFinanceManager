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

    // Operation Specific
    ConfigurationUpdateFailed = 300,
    OperationNotPermitted = 301,

    // Generic Resource
    UnauthorizedAccess = 403,
    ResourceNotFound = 404,

    // Resource Specific
    UserAlreadyExists = 501,
    TransactionsSheetNotFound = 511,
    // EmailAlreadyTaken = 502, // If you have unique email constraint
    // UsernameAlreadyTaken = 503, // If you have unique username constraint

    // External Dependencies
    DatabaseError = 600, // Can be generic or more specific
    ThirdPartyServiceError = 601
}