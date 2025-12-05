namespace TheKittySaver.AdoptionSystem.Domain.Core.Monads.OptionMonad;

public readonly struct ValueMaybe<T> where T : struct
{
    private readonly T? _value;
    private ValueMaybe(T? value)
    {
        _value = value;
    }
    public static ValueMaybe<T> From(T? value) => new(value);
    public static ValueMaybe<T> None() => new(null);
    public bool HasValue => !HasNoValue;
    public bool HasNoValue => _value is null;
    public T Value => HasValue
        ? _value!.Value
        : throw new InvalidOperationException("The value can not be accessed because it does not exist.");
    
    public TOut Match<TOut>(Func<T, TOut> from, Func<TOut> none)
        => _value.HasValue ? from(_value.Value) : none();

    public T ValueOr(Func<T> valueProvider) => _value ?? valueProvider();
    
    public static implicit operator ValueMaybe<T>(T value) => From(value);
}
