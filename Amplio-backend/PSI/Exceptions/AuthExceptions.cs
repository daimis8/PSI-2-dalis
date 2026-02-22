namespace PSI.Exceptions
{
    public class InvalidCredentialsException : Exception
    {
        public InvalidCredentialsException() : base("Invalid username or password") { }
    }

    public class UsernameAlreadyExistsException : Exception
    {
        public UsernameAlreadyExistsException() : base("Username is already taken") { }
    }

    public class InvalidPasswordException : Exception
    {
        public InvalidPasswordException(string message) : base(message) { }
    }
}
