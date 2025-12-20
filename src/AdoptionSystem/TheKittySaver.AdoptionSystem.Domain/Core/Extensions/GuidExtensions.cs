using TheKittySaver.AdoptionSystem.Domain.Core.Monads.OptionMonad;
using TheKittySaver.AdoptionSystem.Primitives.Common;

namespace TheKittySaver.AdoptionSystem.Domain.Core.Extensions;

public static class GuidExtensions
{
    extension(Guid? guid)
    {
        public ValueMaybe<TId> ToMaybeId<TId>() where TId : struct, IStronglyTypedId<TId>
            => guid.HasValue ? TId.Create(guid.Value) : ValueMaybe<TId>.None();
    }
}
