namespace Exceptions
{
    public class InvalidPersonIDException : Exception
    {
        public InvalidPersonIDException()
        {
        }

        public InvalidPersonIDException(string? message) : base(message)
        {
        }

        public InvalidPersonIDException(string? message, Exception? innerException) : base(message, innerException)
        {
        }
    }
}
