namespace Chinook.Exceptions
{
    public class ChinookException : Exception
    {
        public ChinookException()
        {
        }

        public ChinookException(string? message) : base(message)
        {
        }

        public ChinookException(string? message, Exception? innerException) : base(message, innerException)
        {
        }
    }
}
