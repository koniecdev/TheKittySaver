using TheKittySaver.AdoptionSystem.Domain.Aggregates.PersonAggregate.ValueObjects;
using TheKittySaver.AdoptionSystem.Domain.Core.Primitives.Results;

namespace TheKittySaver.AdoptionSystem.Domain.Aggregates.PersonAggregate.Factories;

public interface IPhoneNumberFactory
{
    Result<PhoneNumber> Create(string value);
}