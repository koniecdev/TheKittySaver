using TheKittySaver.AdoptionSystem.Domain.Core.Monads.ResultMonad;

namespace TheKittySaver.AdoptionSystem.Domain.Tests.Unit.Shared.Extensions;

public static class ResultExtensions
{
    extension<T>(Result<T> result)
    {
        public void EnsureSuccess()
        {
            if (result.IsSuccess)
            {
                return;
            }
            
            throw new Exception(result.Error.ToString());
        }
    }
}