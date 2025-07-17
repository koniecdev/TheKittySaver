namespace TheKittySaver.AdoptionSystem.Domain.Tests.Unit.Shared;

public class NullOrEmptyData : TheoryData<string?>
{
    public NullOrEmptyData()
    {
        Add(null);
        Add("");
        Add(" ");
    }
}