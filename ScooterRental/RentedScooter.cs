namespace ScooterRental
{
    public class RentedScooter
    {
        public RentedScooter(string id, DateTime startTime)
        {
            Id = id;
            StartRent = startTime;
        }
 
        public string Id { get; }
        public DateTime StartRent { get; }
        public DateTime? EndRent { get; set; }
        public decimal TotalRentPrice { get; set; }
    }
}
