using FluentValidation;

namespace TheKittySaver.AdoptionSystem.Persistence.Settings;

internal sealed class ConnectionStringSettingsValidator : AbstractValidator<ConnectionStringSettings>
{
    public ConnectionStringSettingsValidator()
    {
        RuleFor(x => x.Database).NotEmpty();
    }
}
