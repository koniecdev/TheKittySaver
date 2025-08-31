using TheKittySaver.AdoptionSystem.Domain.Core.Errors;
using TheKittySaver.AdoptionSystem.Domain.Core.Primitives.BuildingBlocks;
using TheKittySaver.AdoptionSystem.Domain.Core.Primitives.ResultMonad;

namespace TheKittySaver.AdoptionSystem.Domain.Aggregates.CatAggregate.ValueObjects;

public sealed class CatColor : ValueObject
{
    public enum ColorType
    {
        Unset,
        Black,
        White,
        Orange,
        Gray,
        Tabby,
        Calico,
        Tortoiseshell,
        BlackAndWhite,
        Other
    }
    
    public ColorType Value { get; }
    
    public static CatColor Black() => new(ColorType.Black);
    public static CatColor White() => new(ColorType.White);
    public static CatColor Orange() => new(ColorType.Orange);
    public static CatColor Gray() => new(ColorType.Gray);
    public static CatColor Tabby() => new(ColorType.Tabby);
    public static CatColor Calico() => new(ColorType.Calico);
    public static CatColor Tortoiseshell() => new(ColorType.Tortoiseshell);
    public static CatColor BlackAndWhite() => new(ColorType.BlackAndWhite);
    public static CatColor Other() => new(ColorType.Other);
    
    private CatColor(ColorType value)
    {
        Value = value;
    }

    protected override IEnumerable<object> GetAtomicValues()
    {
        yield return Value;
    }
}