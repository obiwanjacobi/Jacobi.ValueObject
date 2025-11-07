namespace Jacobi.ValueObject;

[Serializable]
public class ValueObjectException : Exception
{
    public ValueObjectException() { }
    public ValueObjectException(string message) : base(message) { }
    public ValueObjectException(string message, Exception inner) : base(message, inner) { }
}

