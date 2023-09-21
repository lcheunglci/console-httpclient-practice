namespace Movies.Client
{
    internal class UnauthorizedApiAccessException : Exception
    {
        public UnauthorizedApiAccessException()
        {
            
        }

        public UnauthorizedApiAccessException(string? message) : base(message)
        {

        }

        public UnauthorizedApiAccessException(string? message, Exception? innerException) : base(message, innerException)
        {

        }
    }
}