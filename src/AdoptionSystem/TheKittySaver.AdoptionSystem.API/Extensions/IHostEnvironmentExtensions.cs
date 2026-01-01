namespace TheKittySaver.AdoptionSystem.API.Extensions;

internal static class IHostEnvironmentExtensions
{
    extension(IHostEnvironment hostEnvironment)
    {
        public bool IsLocal()
        {
            ArgumentNullException.ThrowIfNull(hostEnvironment);

            return hostEnvironment.IsEnvironment(Environments.Local);
        }
        
        public bool IsTesting()
        {
            ArgumentNullException.ThrowIfNull(hostEnvironment);

            return hostEnvironment.IsEnvironment(Environments.Testing);
        }
    }
}
