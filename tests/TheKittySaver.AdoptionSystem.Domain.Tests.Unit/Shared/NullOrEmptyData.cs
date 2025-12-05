namespace TheKittySaver.AdoptionSystem.Domain.Tests.Unit.Shared;

internal sealed class NullOrEmptyData : TheoryData<string?>
{
    public NullOrEmptyData()
    {
        Add(null);
        Add("");
        Add(" ");
    }
}
