using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;
using TheKittySaver.AdoptionSystem.Domain.Core.BuildingBlocks;
using TheKittySaver.AdoptionSystem.Domain.Core.Enums;
using TheKittySaver.AdoptionSystem.Domain.Core.Errors;
using TheKittySaver.AdoptionSystem.Domain.SharedValueObjects.AddressCompounds.Specifications;
using TheKittySaver.AdoptionSystem.Primitives.Enums;
using TheKittySaver.AdoptionSystem.Primitives.Guards;

namespace TheKittySaver.AdoptionSystem.Infrastructure.Specifications;

public sealed partial class PolandAddressConsistencySpecification : IAddressConsistencySpecification
{
    private static readonly Regex PolishPostalCodeRegex = PolishPostalCode();

    private static readonly Dictionary<string, PolandVoivodeship> PostalCodePrefixToVoivodeship = new()
    {
        // Wielkopolskie (60-64)
        ["60"] = PolandVoivodeship.Wielkopolskie,
        ["61"] = PolandVoivodeship.Wielkopolskie,
        ["62"] = PolandVoivodeship.Wielkopolskie,
        ["63"] = PolandVoivodeship.Wielkopolskie,
        ["64"] = PolandVoivodeship.Wielkopolskie,

        // Kujawsko-Pomorskie (85-89)
        ["85"] = PolandVoivodeship.KujawskoXPomorskie,
        ["86"] = PolandVoivodeship.KujawskoXPomorskie,
        ["87"] = PolandVoivodeship.KujawskoXPomorskie,
        ["88"] = PolandVoivodeship.KujawskoXPomorskie,
        ["89"] = PolandVoivodeship.KujawskoXPomorskie,

        // Małopolskie (30-34)
        ["30"] = PolandVoivodeship.Małopolskie,
        ["31"] = PolandVoivodeship.Małopolskie,
        ["32"] = PolandVoivodeship.Małopolskie,
        ["33"] = PolandVoivodeship.Małopolskie,
        ["34"] = PolandVoivodeship.Małopolskie,

        // Łódzkie (90-99)
        ["90"] = PolandVoivodeship.Łódzkie,
        ["91"] = PolandVoivodeship.Łódzkie,
        ["92"] = PolandVoivodeship.Łódzkie,
        ["93"] = PolandVoivodeship.Łódzkie,
        ["94"] = PolandVoivodeship.Łódzkie,
        ["95"] = PolandVoivodeship.Łódzkie,
        ["96"] = PolandVoivodeship.Łódzkie,
        ["97"] = PolandVoivodeship.Łódzkie,
        ["98"] = PolandVoivodeship.Łódzkie,
        ["99"] = PolandVoivodeship.Łódzkie,

        // Dolnośląskie (50-59)
        ["50"] = PolandVoivodeship.Dolnośląskie,
        ["51"] = PolandVoivodeship.Dolnośląskie,
        ["52"] = PolandVoivodeship.Dolnośląskie,
        ["53"] = PolandVoivodeship.Dolnośląskie,
        ["54"] = PolandVoivodeship.Dolnośląskie,
        ["55"] = PolandVoivodeship.Dolnośląskie,
        ["56"] = PolandVoivodeship.Dolnośląskie,
        ["57"] = PolandVoivodeship.Dolnośląskie,
        ["58"] = PolandVoivodeship.Dolnośląskie,
        ["59"] = PolandVoivodeship.Dolnośląskie,

        // Lubelskie (20-24)
        ["20"] = PolandVoivodeship.Lubelskie,
        ["21"] = PolandVoivodeship.Lubelskie,
        ["22"] = PolandVoivodeship.Lubelskie,
        ["23"] = PolandVoivodeship.Lubelskie,
        ["24"] = PolandVoivodeship.Lubelskie,

        // Lubuskie (65-69)
        ["65"] = PolandVoivodeship.Lubuskie,
        ["66"] = PolandVoivodeship.Lubuskie,
        ["67"] = PolandVoivodeship.Lubuskie,
        ["68"] = PolandVoivodeship.Lubuskie,
        ["69"] = PolandVoivodeship.Lubuskie,

        // Mazowieckie (00-09, 26-27)
        ["00"] = PolandVoivodeship.Mazowieckie,
        ["01"] = PolandVoivodeship.Mazowieckie,
        ["02"] = PolandVoivodeship.Mazowieckie,
        ["03"] = PolandVoivodeship.Mazowieckie,
        ["04"] = PolandVoivodeship.Mazowieckie,
        ["05"] = PolandVoivodeship.Mazowieckie,
        ["06"] = PolandVoivodeship.Mazowieckie,
        ["07"] = PolandVoivodeship.Mazowieckie,
        ["08"] = PolandVoivodeship.Mazowieckie,
        ["09"] = PolandVoivodeship.Mazowieckie,
        ["26"] = PolandVoivodeship.Mazowieckie,
        ["27"] = PolandVoivodeship.Mazowieckie,

        // Opolskie (45-49)
        ["45"] = PolandVoivodeship.Opolskie,
        ["46"] = PolandVoivodeship.Opolskie,
        ["47"] = PolandVoivodeship.Opolskie,
        ["48"] = PolandVoivodeship.Opolskie,
        ["49"] = PolandVoivodeship.Opolskie,

        // Podlaskie (15-19)
        ["15"] = PolandVoivodeship.Podlaskie,
        ["16"] = PolandVoivodeship.Podlaskie,
        ["17"] = PolandVoivodeship.Podlaskie,
        ["18"] = PolandVoivodeship.Podlaskie,
        ["19"] = PolandVoivodeship.Podlaskie,

        // Pomorskie (80-84)
        ["80"] = PolandVoivodeship.Pomorskie,
        ["81"] = PolandVoivodeship.Pomorskie,
        ["82"] = PolandVoivodeship.Pomorskie,
        ["83"] = PolandVoivodeship.Pomorskie,
        ["84"] = PolandVoivodeship.Pomorskie,

        // Śląskie (40-44)
        ["40"] = PolandVoivodeship.Śląskie,
        ["41"] = PolandVoivodeship.Śląskie,
        ["42"] = PolandVoivodeship.Śląskie,
        ["43"] = PolandVoivodeship.Śląskie,
        ["44"] = PolandVoivodeship.Śląskie,

        // Podkarpackie (35-39)
        ["35"] = PolandVoivodeship.Podkarpackie,
        ["36"] = PolandVoivodeship.Podkarpackie,
        ["37"] = PolandVoivodeship.Podkarpackie,
        ["38"] = PolandVoivodeship.Podkarpackie,
        ["39"] = PolandVoivodeship.Podkarpackie,

        // Świętokrzyskie (25, 28, 29)
        ["25"] = PolandVoivodeship.Świętokrzyskie,
        ["28"] = PolandVoivodeship.Świętokrzyskie,
        ["29"] = PolandVoivodeship.Świętokrzyskie,

        // Warmińsko-Mazurskie (10-14)
        ["10"] = PolandVoivodeship.WarmińskoXMazurskie,
        ["11"] = PolandVoivodeship.WarmińskoXMazurskie,
        ["12"] = PolandVoivodeship.WarmińskoXMazurskie,
        ["13"] = PolandVoivodeship.WarmińskoXMazurskie,
        ["14"] = PolandVoivodeship.WarmińskoXMazurskie,

        // Zachodniopomorskie (70-78)
        ["70"] = PolandVoivodeship.Zachodniopomorskie,
        ["71"] = PolandVoivodeship.Zachodniopomorskie,
        ["72"] = PolandVoivodeship.Zachodniopomorskie,
        ["73"] = PolandVoivodeship.Zachodniopomorskie,
        ["74"] = PolandVoivodeship.Zachodniopomorskie,
        ["75"] = PolandVoivodeship.Zachodniopomorskie,
        ["76"] = PolandVoivodeship.Zachodniopomorskie,
        ["77"] = PolandVoivodeship.Zachodniopomorskie,
        ["78"] = PolandVoivodeship.Zachodniopomorskie,
    };

    private static readonly Dictionary<string, PolandVoivodeship> VoivodeshipNameMapping =
        new(StringComparer.OrdinalIgnoreCase)
        {
            ["wielkopolskie"] = PolandVoivodeship.Wielkopolskie,
            ["kujawsko-pomorskie"] = PolandVoivodeship.KujawskoXPomorskie,
            ["kujawskopomorskie"] = PolandVoivodeship.KujawskoXPomorskie,
            ["małopolskie"] = PolandVoivodeship.Małopolskie,
            ["malopolskie"] = PolandVoivodeship.Małopolskie,
            ["łódzkie"] = PolandVoivodeship.Łódzkie,
            ["lodzkie"] = PolandVoivodeship.Łódzkie,
            ["dolnośląskie"] = PolandVoivodeship.Dolnośląskie,
            ["dolnoslaskie"] = PolandVoivodeship.Dolnośląskie,
            ["lubelskie"] = PolandVoivodeship.Lubelskie,
            ["lubuskie"] = PolandVoivodeship.Lubuskie,
            ["mazowieckie"] = PolandVoivodeship.Mazowieckie,
            ["opolskie"] = PolandVoivodeship.Opolskie,
            ["podlaskie"] = PolandVoivodeship.Podlaskie,
            ["pomorskie"] = PolandVoivodeship.Pomorskie,
            ["śląskie"] = PolandVoivodeship.Śląskie,
            ["slaskie"] = PolandVoivodeship.Śląskie,
            ["podkarpackie"] = PolandVoivodeship.Podkarpackie,
            ["świętokrzyskie"] = PolandVoivodeship.Świętokrzyskie,
            ["swietokrzyskie"] = PolandVoivodeship.Świętokrzyskie,
            ["warmińsko-mazurskie"] = PolandVoivodeship.WarmińskoXMazurskie,
            ["warminskomazurskie"] = PolandVoivodeship.WarmińskoXMazurskie,
            ["zachodniopomorskie"] = PolandVoivodeship.Zachodniopomorskie,
        };

    public bool IsSatisfiedBy(
        CountryCode countryCode,
        string postalCode,
        string region,
        [NotNullWhen(false)] out Error? error)
    {
        Ensure.IsValidEnum(countryCode);

        error = null;

        if (countryCode != CountryCode.PL)
        {
            return true;
        }

        if (string.IsNullOrWhiteSpace(postalCode))
        {
            error = DomainErrors.AddressConsistency.PostalCodeRequired;
            return false;
        }

        postalCode = postalCode.Trim();

        if (!PolishPostalCodeRegex.IsMatch(postalCode))
        {
            error = DomainErrors.AddressConsistency.InvalidPostalCodeFormat(postalCode);
            return false;
        }

        if (string.IsNullOrWhiteSpace(region))
        {
            return true;
        }

        string prefix = postalCode[..2];

        if (!PostalCodePrefixToVoivodeship.TryGetValue(prefix, out PolandVoivodeship expectedVoivodeship))
        {
            error = DomainErrors.AddressConsistency.UnknownPostalCodePrefix(prefix);
            return false;
        }

        string normalizedRegion = region.Trim();

        if (!VoivodeshipNameMapping.TryGetValue(normalizedRegion, out PolandVoivodeship actualVoivodeship))
        {
            error = DomainErrors.AddressConsistency.InvalidRegion(normalizedRegion);
            return false;
        }

        if (expectedVoivodeship != actualVoivodeship)
        {
            error = DomainErrors.AddressConsistency.PostalCodeRegionMismatch(postalCode, normalizedRegion);
            return false;
        }

        return true;
    }

    [GeneratedRegex(@"^\d{2}-\d{3}$", RegexOptions.Compiled)]
    private static partial Regex PolishPostalCode();
}
