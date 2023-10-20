namespace ScooterRental.Exceptions
{
    public class NoAvailableScooterException : Exception
    {
        public NoAvailableScooterException() : base("There are no available scooters to rent") { }
    }
}
