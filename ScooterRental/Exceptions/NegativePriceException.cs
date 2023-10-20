namespace ScooterRental.Exceptions
{
    public class NegativePriceException : Exception
    {
        public NegativePriceException() : base("Price must be positive and above zero") { }
    }
}
