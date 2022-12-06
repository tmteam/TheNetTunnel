namespace TNT.Exceptions;

public interface ITntCallException
{
    bool IsFatal { get; }
    short? MessageId { get; }
    short? AskId { get; }
}