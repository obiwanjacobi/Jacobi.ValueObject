namespace Jacobi.ValueObject;

/// <summary>
/// Thrown when there is problem with your ValueObject.
/// </summary>
[Serializable]
public class ValueObjectException : Exception
{
    /// <summary>
    /// Default constructor.
    /// </summary>
    public ValueObjectException() { }
    /// <summary>
    /// Initializes a new instance of the <see cref="ValueObjectException"/> class with a specified error message.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    public ValueObjectException(string message) : base(message) { }
    /// <summary>
    /// Initializes a new instance of the <see cref="ValueObjectException"/> class with a specified error message and a
    /// reference to the inner exception that is the cause of this exception.
    /// </summary>
    /// <param name="message">The error message that explains the reason for the exception.</param>
    /// <param name="inner">The exception that is the cause of the current exception, or <see langword="null"/> if no inner exception is
    /// specified.</param>
    public ValueObjectException(string message, Exception inner)
        : base(message, inner) { }
}

