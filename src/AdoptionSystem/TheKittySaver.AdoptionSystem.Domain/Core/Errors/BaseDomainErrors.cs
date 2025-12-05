using System.Globalization;
using TheKittySaver.AdoptionSystem.Domain.Core.BuildingBlocks;
using TheKittySaver.AdoptionSystem.Domain.Core.Enums;

namespace TheKittySaver.AdoptionSystem.Domain.Core.Errors;

public static partial class DomainErrors
{
    private static Error HasNotBeenFound(string entity, Guid id) 
        => new($"{entity}.NotFound",
            $"The {entity.ToLowerInvariant()} with id: {id} has not been found.",
            TypeOfError.NotFound);
    
    private static Error Required(string entity, string property) 
        => new($"{entity}.{property}.NullOrEmpty",
            $"The {property.ToLowerInvariant()} is required.",
            TypeOfError.Validation);
    
    private static Error AlreadyHasBeenTaken(string entity, string property, object alreadyTakenValue) 
        => new($"{entity}.{property}.AlreadyTaken",
            $"The {property.ToLowerInvariant()} value '{alreadyTakenValue}' is already taken.",
            TypeOfError.Conflict);

    private static Error TooManyCharacters(string entity, string property, int maxLength) 
        => new($"{entity}.{property}.LongerThanAllowed", 
            $"The {property.ToLowerInvariant()} exceeds maximum length of {maxLength}.",
            TypeOfError.Validation);

    private static Error BadFormat(string entity, string property)
        => new($"{entity}.{property}.InvalidFormat",
            $"The {property.ToLowerInvariant()} format is invalid.",
            TypeOfError.Validation);

    private static Error BelowValue<T>(string entity, string property, T actualValue, T minimalValue) where T : struct
        => new($"{entity}.{property}.BelowValue", 
            $"The {property.ToLowerInvariant()} has been set with value '{actualValue}', and it is below the minimum required value '{minimalValue}'.",
            TypeOfError.Validation);
    
    private static Error AboveValue<T>(string entity, string property, T actualValue, T maximumValue) where T : struct
        => new($"{entity}.{property}.AboveValue", 
            $"The {property.ToLowerInvariant()} has been set with value '{actualValue}', and it is above the maximum required value '{maximumValue}'.",
            TypeOfError.Validation);
    
    private static Error DateIsInThePast(string entity, string property)
        => new($"{entity}.{property}.DateIsInThePast",
            $"The {property.ToLowerInvariant()} can't be in the past.",
            TypeOfError.Validation);

    /// <summary>
    /// Creates a conflict error when an entity is already in a particular state.
    /// Use for "Already X" scenarios (e.g., AlreadyClaimed, AlreadyPublished).
    /// </summary>
    private static Error StateConflict(string entity, string property, string message, string code)
        => new($"{entity}.{property}.{code}", message, TypeOfError.Conflict);

    /// <summary>
    /// Creates a conflict error when an operation cannot be performed due to current state.
    /// Use for "Cannot X" or "Must be X" scenarios (e.g., CannotClaimDraftCat, MustBeDraftForAssignment).
    /// </summary>
    private static Error InvalidOperation(string entity, string property, string message, string code)
        => new($"{entity}.{property}.{code}", message, TypeOfError.Conflict);

    private static Error CustomMessage(
        string entity,
        string property,
        string message,
        string code,
        TypeOfError type)
        => new($"{entity}.{property}.{code}", message, type);

    public static Error DeletionCorruption(string entity)
        => new($"{entity}.DeletionCorruption", 
            $"{entity} was not found in the list for deletion even though it was supposed to be there.");
}
