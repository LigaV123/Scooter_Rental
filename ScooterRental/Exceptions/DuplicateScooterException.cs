namespace ScooterRental.Exceptions
{
    public class DuplicateScooterException : Exception
    {
        public DuplicateScooterException() : base("Scooter already exists") { }
    }
}
