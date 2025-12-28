using TheKittySaver.AdoptionSystem.Domain.Core.Monads.ResultMonad;
using TheKittySaver.AdoptionSystem.Domain.SharedValueObjects.Timestamps;

namespace TheKittySaver.AdoptionSystem.Domain.Core.Abstractions;

public interface IClaimable
{
    public ClaimedAt? ClaimedAt { get; }

    public Result Claim(ClaimedAt claimedAt);
}
