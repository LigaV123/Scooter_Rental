namespace ScooterRental.Exceptions
{
    public class NonexistentScooterIdException : Exception
    {
        public NonexistentScooterIdException() : base("Scooter with that Id does not exist") { }
    }
}
