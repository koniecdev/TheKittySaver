using TheKittySaver.AdoptionSystem.Domain.Core.BuildingBlocks;

namespace TheKittySaver.AdoptionSystem.Domain.Core.Monads.ResultMonad;

/// <summary>
/// Represents a result of some operation, with status information and possibly an error.
/// </summary>
public class Result
{
    /// <summary>
    /// Initializes a new instance of the <see cref="Result"/> class with the specified parameters.
    /// </summary>
    /// <param name="isSuccess">The flag indicating if the result is successful.</param>
    /// <param name="error">The error.</param>
    protected Result(bool isSuccess, Error error)
    {
        if (isSuccess && error != Error.None || !isSuccess && error == Error.None)
        {
            throw new InvalidOperationException();
        }

        IsSuccess = isSuccess;
        Error = error;
    }

    /// <summary>
    /// Gets a value indicating whether the result is a success result.
    /// </summary>
    public bool IsSuccess { get; }

    /// <summary>
    /// Gets a value indicating whether the result is a failure result.
    /// </summary>
    public bool IsFailure => !IsSuccess;

    /// <summary>
    /// Gets the error.
    /// </summary>
    public Error Error { get; }

    /// <summary>
    /// Returns a success <see cref="Result"/>.
    /// </summary>
    /// <returns>A new instance of <see cref="Result"/> with the success flag set.</returns>
    public static Result Success() => new(true, Error.None);

    /// <summary>
    /// Returns a success <see cref="Result{TValue}"/> with the specified value.
    /// </summary>
    /// <typeparam name="TValue">The result type.</typeparam>
    /// <param name="value">The result value.</param>
    /// <returns>A new instance of <see cref="Result{TValue}"/> with the success flag set.</returns>
    public static Result<TValue> Success<TValue>(TValue value) => new(value, true, Error.None);

    /// <summary>
    /// Returns a failure <see cref="Result"/> with the specified error.
    /// </summary>
    /// <param name="error">The error.</param>
    /// <returns>A new instance of <see cref="Result"/> with the specified error and failure flag set.</returns>
    public static Result Failure(Error error) => new(false, error);

    /// <summary>
    /// Returns a failure <see cref="Result{TValue}"/> with the specified error.
    /// </summary>
    /// <typeparam name="TValue">The result type.</typeparam>
    /// <param name="error">The error.</param>
    /// <returns>A new instance of <see cref="Result{TValue}"/> with the specified error and failure flag set.</returns>
    /// <remarks>
    /// We're purposefully ignoring the nullable assignment here because the API will never allow it to be accessed.
    /// The value is accessed through a method that will throw an exception if the result is a failure result.
    /// </remarks>
    public static Result<TValue> Failure<TValue>(Error error) => new(default!, false, error);
}

/// <summary>
/// Represents the result of some operation, with status information and possibly a value and an error.
/// </summary>
/// <typeparam name="TValue">The result value type.</typeparam>
public class Result<TValue> : Result
{
    /// <summary>
    /// Initializes a new instance of the <see cref="Result{TValueType}"/> class with the specified parameters.
    /// </summary>
    /// <param name="value">The result value.</param>
    /// <param name="isSuccess">The flag indicating if the result is successful.</param>
    /// <param name="error">The error.</param>
    protected internal Result(TValue value, bool isSuccess, Error error)
        : base(isSuccess, error)
    {
        Value = value;
    }

    public static implicit operator Result<TValue>(TValue value) => Success(value);

    /// <summary>
    /// Gets the result value if the result is successful, otherwise throws an exception.
    /// </summary>
    /// <returns>The result value if the result is successful.</returns>
    /// <exception cref="InvalidOperationException"> when <see cref="Result.IsFailure"/> is true.</exception>
    public TValue Value => IsSuccess
        ? field
        : throw new InvalidOperationException("The value of a failure result can not be accessed.");
}
