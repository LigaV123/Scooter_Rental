namespace ScooterRental.Exceptions
{
    public class InvalidIDException : Exception
    {
        public InvalidIDException() : base("ID must be at least 1 character long") { }
    }
}
