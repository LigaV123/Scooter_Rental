namespace ScooterRental.Exceptions
{
    public class ScooterRentedException : Exception
    {
        public ScooterRentedException() : base("Scooter is still in rent") { }
    }
}
