namespace TheKittySaver.AdoptionSystem.API.Exceptions;

/// <summary>
/// Base class for all domain-specific exceptions.
/// </summary>
public abstract class DomainException : Exception
{
    public string ErrorCode { get; }

    protected DomainException(string message, string errorCode) : base(message)
    {
        ErrorCode = errorCode;
    }

    protected DomainException(string message, string errorCode, Exception innerException)
        : base(message, innerException)
    {
        ErrorCode = errorCode;
    }
}

/// <summary>
/// Exception thrown when a requested resource is not found.
/// </summary>
public sealed class NotFoundException : DomainException
{
    public string ResourceName { get; }
    public object? ResourceId { get; }

    public NotFoundException(string resourceName, object? resourceId = null)
        : base($"Resource '{resourceName}' with identifier '{resourceId}' was not found.",
            "RESOURCE_NOT_FOUND")
    {
        ResourceName = resourceName;
        ResourceId = resourceId;
    }

    public NotFoundException(string message)
        : base(message, "RESOURCE_NOT_FOUND")
    {
        ResourceName = string.Empty;
    }
}

/// <summary>
/// Exception thrown when validation fails.
/// </summary>
public sealed class ValidationException : DomainException
{
    public IReadOnlyDictionary<string, string[]> Errors { get; }

    public ValidationException(IReadOnlyDictionary<string, string[]> errors)
        : base("One or more validation errors occurred.", "VALIDATION_ERROR")
    {
        Errors = errors;
    }

    public ValidationException(string propertyName, string errorMessage)
        : base($"Validation failed for '{propertyName}': {errorMessage}", "VALIDATION_ERROR")
    {
        Errors = new Dictionary<string, string[]>
        {
            { propertyName, [errorMessage] }
        };
    }
}

/// <summary>
/// Exception thrown when a conflict occurs (e.g., duplicate resource).
/// </summary>
public sealed class ConflictException : DomainException
{
    public ConflictException(string message)
        : base(message, "CONFLICT_ERROR")
    {
    }

    public ConflictException(string resourceName, object identifier)
        : base($"Resource '{resourceName}' with identifier '{identifier}' already exists.", "CONFLICT_ERROR")
    {
    }
}

/// <summary>
/// Exception thrown when the user is not authenticated.
/// </summary>
public sealed class UnauthenticatedException : DomainException
{
    public UnauthenticatedException()
        : base("Authentication is required to access this resource.", "UNAUTHENTICATED")
    {
    }

    public UnauthenticatedException(string message)
        : base(message, "UNAUTHENTICATED")
    {
    }
}

/// <summary>
/// Exception thrown when the user is authenticated but not authorized for the requested operation.
/// </summary>
public sealed class ForbiddenAccessException : DomainException
{
    public ForbiddenAccessException()
        : base("You do not have permission to access this resource.", "FORBIDDEN")
    {
    }

    public ForbiddenAccessException(string message)
        : base(message, "FORBIDDEN")
    {
    }

    public ForbiddenAccessException(string resource, string action)
        : base($"You do not have permission to {action} on '{resource}'.", "FORBIDDEN")
    {
    }
}

/// <summary>
/// Exception thrown for bad request scenarios not covered by validation.
/// </summary>
public sealed class BadRequestException : DomainException
{
    public BadRequestException(string message)
        : base(message, "BAD_REQUEST")
    {
    }
}
