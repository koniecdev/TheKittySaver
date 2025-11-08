using TheKittySaver.AdoptionSystem.Domain.Core.BuildingBlocks;
using TheKittySaver.AdoptionSystem.Domain.Core.Enums;

namespace TheKittySaver.AdoptionSystem.Domain.Core.Errors;

public static partial class DomainErrors
{
    private static Error HasNotBeenFound(string entity, Guid id) 
        => new($"{entity}.NotFound",
            $"The {entity.ToLower()} with id: {id} has not been found.",
            TypeOfError.NotFound);
    
    private static Error Required(string entity, string property) 
        => new($"{entity}.{property}.NullOrEmpty",
            $"The {property.ToLower()} is required.",
            TypeOfError.Validation);
    
    private static Error AlreadyHasBeenTaken(string entity, string property, object alreadyTakenValue) 
        => new($"{entity}.{property}.AlreadyTaken",
            $"The {property.ToLower()} value '{alreadyTakenValue}' is already taken.",
            TypeOfError.Conflict);

    private static Error TooManyCharacters(string entity, string property, int maxLength) 
        => new($"{entity}.{property}.LongerThanAllowed", 
            $"The {property.ToLower()} exceeds maximum length of {maxLength}.",
            TypeOfError.Validation);

    private static Error BadFormat(string entity, string property) 
        => new($"{entity}.{property}.InvalidFormat", 
            $"The {property.ToLower()} format is invalid.",
            TypeOfError.Validation);
    
    private static Error ValueInvalid(string entity, string property) 
        => new($"{entity}.{property}.InvalidValue",
            $"The {property.ToLower()} value is invalid.",
            TypeOfError.Validation);
    
    private static Error MustBeEmpty(string entity, string property, string propertyThatIsEmpty) 
        => new($"{entity}.{property}.MustBeEmpty",
            $"The {property.ToLower()} must be empty, when {propertyThatIsEmpty.ToLower()} is empty",
            TypeOfError.Validation);
    
    private static Error BelowValue<T>(string entity, string property, T actualValue, T minimalValue) where T : struct
        => new($"{entity}.{property}.BelowValue", 
            $"The {property.ToLower()} has been set with value '{actualValue}', and it is below the minimum required value '{minimalValue}'.",
            TypeOfError.Validation);
    
    private static Error AboveValue<T>(string entity, string property, T actualValue, T maximumValue) where T : struct
        => new($"{entity}.{property}.AboveValue", 
            $"The {property.ToLower()} has been set with value '{actualValue}', and it is above the maximum required value '{maximumValue}'.",
            TypeOfError.Validation);
    
    private static Error CustomMessage(string entity, string property, string message, TypeOfError type = TypeOfError.Validation) 
        => new($"{entity}.{property}.InvalidValue", message, type);

    public static Error DeletionCorruption(string entity)
        => new($"{entity}.DeletionCorruption", 
            $"{entity} was not found in the list for deletion even though it was supposed to be there.");
}