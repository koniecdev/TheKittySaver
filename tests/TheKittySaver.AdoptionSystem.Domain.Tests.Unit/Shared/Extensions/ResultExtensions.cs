using TheKittySaver.AdoptionSystem.Domain.Core.Monads.ResultMonad;

namespace TheKittySaver.AdoptionSystem.Domain.Tests.Unit.Shared.Extensions;

internal static class ResultExtensions
{
    extension<T>(Result<T> result)
    {
        public void EnsureSuccess() => EnsureTheSuccess(result);
    }

    extension(Result result)
    {
        public void EnsureSuccess() => EnsureTheSuccess(result);
    }

    private static void EnsureTheSuccess(Result result)
    {
        if (result.IsSuccess)
        {
            return;
        }

        throw new InvalidOperationException(result.Error.ToString());
    }
}
