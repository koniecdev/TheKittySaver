namespace TheKittySaver.AdoptionSystem.API.Authorization;

/// <summary>
/// Marker interface for requests that require the user to be authenticated.
/// </summary>
public interface IAuthorizedRequest
{
}

/// <summary>
/// Marker interface for requests that require admin role.
/// </summary>
public interface IAdminOnlyRequest : IAuthorizedRequest
{
}

/// <summary>
/// Marker interface for requests that can be performed by either a job (background service) or an admin.
/// </summary>
public interface IJobOrAdminOnlyRequest : IAuthorizedRequest
{
}

/// <summary>
/// Interface for requests that operate on a specific cat resource.
/// Enables ownership-based authorization.
/// </summary>
public interface ICatRequest : IAuthorizedRequest
{
    Guid CatId { get; }
}

/// <summary>
/// Interface for requests that operate on a specific person resource.
/// Enables self-authorization (users can only modify their own profile).
/// </summary>
public interface IPersonRequest : IAuthorizedRequest
{
    Guid PersonId { get; }
}

/// <summary>
/// Interface for requests that operate on a specific adoption announcement.
/// </summary>
public interface IAdoptionAnnouncementRequest : IAuthorizedRequest
{
    Guid AdoptionAnnouncementId { get; }
}

/// <summary>
/// Interface for requests that operate on a specific address.
/// </summary>
public interface IAddressRequest : IAuthorizedRequest
{
    Guid AddressId { get; }
}
