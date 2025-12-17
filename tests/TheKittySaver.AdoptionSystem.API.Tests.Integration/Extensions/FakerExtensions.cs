using Bogus;

namespace TheKittySaver.AdoptionSystem.API.Tests.Integration.Extensions;

internal static class FakerInternetExtensions
{
    extension(Person person)
    {
#pragma warning disable CA1822
        // ReSharper disable once MemberCanBeMadeStatic.Global
        public string PolishPhoneNumber()
        {
            int prefix = Random.Shared.Next(5, 8); // 5,6,7
            return $"+48{prefix}{Random.Shared.Next(100_000_000, 999_999_999)}"[..12];
        }
#pragma warning restore CA1822
    }

}

