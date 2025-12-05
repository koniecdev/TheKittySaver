using System.Collections;

namespace TheKittySaver.AdoptionSystem.Domain.Tests.Unit.Tests.Aggregates.PersonAggregate;

internal sealed class ValidPolishAddressTestData : IEnumerable<object[]>
{
    public IEnumerator<object[]> GetEnumerator()
    {
        // Wielkopolskie
        yield return ["60-123", "Wielkopolskie", "Poznań"];
        yield return ["61-456", "Wielkopolskie", "Poznań"];
        yield return ["62-800", "Wielkopolskie", "Kalisz"];
        yield return ["63-012", "Wielkopolskie", "Konin"];
        yield return ["64-345", "Wielkopolskie", "Piła"];

        // Małopolskie
        yield return ["30-001", "Małopolskie", "Kraków"];
        yield return ["31-234", "Małopolskie", "Kraków"];
        yield return ["32-567", "Małopolskie", "Tarnów"];
        yield return ["33-890", "Małopolskie", "Nowy Sącz"];
        yield return ["34-123", "Małopolskie", "Zakopane"];

        // Mazowieckie
        yield return ["00-001", "Mazowieckie", "Warszawa"];
        yield return ["01-234", "Mazowieckie", "Warszawa"];
        yield return ["02-567", "Mazowieckie", "Warszawa"];
        yield return ["26-890", "Mazowieckie", "Radom"];
        yield return ["27-123", "Mazowieckie", "Ostrołęka"];

        // Śląskie
        yield return ["40-001", "Śląskie", "Katowice"];
        yield return ["41-234", "Śląskie", "Chorzów"];
        yield return ["42-567", "Śląskie", "Częstochowa"];
        yield return ["43-890", "Śląskie", "Bielsko-Biała"];
        yield return ["44-123", "Śląskie", "Gliwice"];

        // Dolnośląskie
        yield return ["50-001", "Dolnośląskie", "Wrocław"];
        yield return ["51-234", "Dolnośląskie", "Wrocław"];
        yield return ["52-567", "Dolnośląskie", "Wrocław"];
        yield return ["58-890", "Dolnośląskie", "Jelenia Góra"];
        yield return ["59-123", "Dolnośląskie", "Wałbrzych"];

        // Pomorskie
        yield return ["80-001", "Pomorskie", "Gdańsk"];
        yield return ["81-234", "Pomorskie", "Gdynia"];
        yield return ["82-567", "Pomorskie", "Gdańsk"];
        yield return ["83-890", "Pomorskie", "Sopot"];
        yield return ["84-123", "Pomorskie", "Słupsk"];

        // Łódzkie
        yield return ["90-001", "Łódzkie", "Łódź"];
        yield return ["93-234", "Łódzkie", "Łódź"];
        yield return ["95-567", "Łódzkie", "Zgierz"];
        yield return ["97-890", "Łódzkie", "Piotrków Trybunalski"];
        yield return ["99-123", "Łódzkie", "Łęczyca"];

        // Lubelskie
        yield return ["20-001", "Lubelskie", "Lublin"];
        yield return ["21-234", "Lubelskie", "Lublin"];
        yield return ["22-567", "Lubelskie", "Chełm"];
        yield return ["23-890", "Lubelskie", "Zamość"];
        yield return ["24-123", "Lubelskie", "Biała Podlaska"];

        // Kujawsko-Pomorskie
        yield return ["85-001", "Kujawsko-Pomorskie", "Bydgoszcz"];
        yield return ["86-234", "Kujawsko-Pomorskie", "Toruń"];
        yield return ["87-567", "Kujawsko-Pomorskie", "Toruń"];
        yield return ["88-890", "Kujawsko-Pomorskie", "Włocławek"];
        yield return ["89-123", "Kujawsko-Pomorskie", "Grudziądz"];

        // Podkarpackie
        yield return ["35-001", "Podkarpackie", "Rzeszów"];
        yield return ["36-234", "Podkarpackie", "Rzeszów"];
        yield return ["37-567", "Podkarpackie", "Stalowa Wola"];
        yield return ["38-890", "Podkarpackie", "Krosno"];
        yield return ["39-123", "Podkarpackie", "Przemyśl"];

        // Podlaskie
        yield return ["15-001", "Podlaskie", "Białystok"];
        yield return ["16-234", "Podlaskie", "Białystok"];
        yield return ["17-567", "Podlaskie", "Suwałki"];
        yield return ["18-890", "Podlaskie", "Łomża"];
        yield return ["19-123", "Podlaskie", "Augustów"];

        // Warmińsko-Mazurskie
        yield return ["10-001", "Warmińsko-Mazurskie", "Olsztyn"];
        yield return ["11-234", "Warmińsko-Mazurskie", "Olsztyn"];
        yield return ["12-567", "Warmińsko-Mazurskie", "Elbląg"];
        yield return ["13-890", "Warmińsko-Mazurskie", "Ełk"];
        yield return ["14-123", "Warmińsko-Mazurskie", "Giżycko"];

        // Zachodniopomorskie
        yield return ["70-001", "Zachodniopomorskie", "Szczecin"];
        yield return ["71-234", "Zachodniopomorskie", "Szczecin"];
        yield return ["75-567", "Zachodniopomorskie", "Koszalin"];
        yield return ["76-890", "Zachodniopomorskie", "Stargard"];
        yield return ["78-123", "Zachodniopomorskie", "Świnoujście"];

        // Opolskie
        yield return ["45-001", "Opolskie", "Opole"];
        yield return ["46-234", "Opolskie", "Opole"];
        yield return ["47-567", "Opolskie", "Kędzierzyn-Koźle"];
        yield return ["48-890", "Opolskie", "Nysa"];
        yield return ["49-123", "Opolskie", "Brzeg"];

        // Lubuskie
        yield return ["65-001", "Lubuskie", "Zielona Góra"];
        yield return ["66-234", "Lubuskie", "Gorzów Wielkopolski"];
        yield return ["67-567", "Lubuskie", "Gorzów Wielkopolski"];
        yield return ["68-890", "Lubuskie", "Żary"];
        yield return ["69-123", "Lubuskie", "Żagań"];

        // Świętokrzyskie
        yield return ["25-001", "Świętokrzyskie", "Kielce"];
        yield return ["28-234", "Świętokrzyskie", "Kielce"];
        yield return ["29-567", "Świętokrzyskie", "Starachowice"];
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
