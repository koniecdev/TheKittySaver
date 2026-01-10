using Bogus;

namespace TheKittySaver.AdoptionSystem.API.Tests.Integration.Extensions;

internal sealed record PolishAddressData
{
    public PolishAddressData(string PostalCode, string Region, string City)
    {
        this.PostalCode = PostalCode;
        this.Region = Region;
        this.City = City;
    }

    public string PostalCode { get; init; }
    public string Region { get; init; }
    public string City { get; init; }

    public void Deconstruct(out string PostalCode, out string Region, out string City)
    {
        PostalCode = this.PostalCode;
        Region = this.Region;
        City = this.City;
    }
}

internal static class FakerInternetExtensions
{
    private static readonly PolishAddressData[] ValidPolishAddresses =
    [
        new("60-123", "Wielkopolskie", "Poznań"),
        new("61-456", "Wielkopolskie", "Poznań"),
        new("62-800", "Wielkopolskie", "Kalisz"),
        new("30-001", "Małopolskie", "Kraków"),
        new("31-234", "Małopolskie", "Kraków"),
        new("32-567", "Małopolskie", "Tarnów"),
        new("00-001", "Mazowieckie", "Warszawa"),
        new("01-234", "Mazowieckie", "Warszawa"),
        new("26-890", "Mazowieckie", "Radom"),
        new("40-001", "Śląskie", "Katowice"),
        new("41-234", "Śląskie", "Chorzów"),
        new("42-567", "Śląskie", "Częstochowa"),
        new("50-001", "Dolnośląskie", "Wrocław"),
        new("51-234", "Dolnośląskie", "Wrocław"),
        new("58-890", "Dolnośląskie", "Jelenia Góra"),
        new("80-001", "Pomorskie", "Gdańsk"),
        new("81-234", "Pomorskie", "Gdynia"),
        new("83-890", "Pomorskie", "Sopot"),
        new("90-001", "Łódzkie", "Łódź"),
        new("93-234", "Łódzkie", "Łódź"),
        new("20-001", "Lubelskie", "Lublin"),
        new("21-234", "Lubelskie", "Lublin"),
        new("85-001", "Kujawsko-Pomorskie", "Bydgoszcz"),
        new("86-234", "Kujawsko-Pomorskie", "Toruń"),
        new("35-001", "Podkarpackie", "Rzeszów"),
        new("36-234", "Podkarpackie", "Rzeszów"),
        new("15-001", "Podlaskie", "Białystok"),
        new("16-234", "Podlaskie", "Białystok"),
        new("10-001", "Warmińsko-Mazurskie", "Olsztyn"),
        new("11-234", "Warmińsko-Mazurskie", "Olsztyn"),
        new("70-001", "Zachodniopomorskie", "Szczecin"),
        new("71-234", "Zachodniopomorskie", "Szczecin"),
        new("45-001", "Opolskie", "Opole"),
        new("46-234", "Opolskie", "Opole"),
        new("65-001", "Lubuskie", "Zielona Góra"),
        new("66-234", "Lubuskie", "Gorzów Wielkopolski"),
        new("25-001", "Świętokrzyskie", "Kielce"),
        new("28-234", "Świętokrzyskie", "Kielce")
    ];

    extension(Person person)
    {
#pragma warning disable CA1822
        // ReSharper disable once MemberCanBeMadeStatic.Global
        public string PolishPhoneNumber()
#pragma warning restore CA1822
        {
            int prefix = Random.Shared.Next(5, 8); // 5,6,7
            return $"+48{prefix}{Random.Shared.Next(100_000_000, 999_999_999)}"[..12];
        }
    }

    extension(Faker faker)
    {
        public PolishAddressData PolishAddress()
        {
            return faker.PickRandom(ValidPolishAddresses);
        }
    }
}

