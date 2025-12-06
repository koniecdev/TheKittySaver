using FluentValidation;

namespace TheKittySaver.AdoptionSystem.API.Settings.ConnectionStringsOption;

internal class ConnectionStringsValidator : AbstractValidator<ConnectionStrings>
{
    public ConnectionStringsValidator()
    {
        RuleFor(x => x.Database).NotEmpty();
    }
}
