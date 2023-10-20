namespace ScooterRental
{
    public class RentalRecordsService : IRentalRecordsService
    {
        private readonly List<RentedScooter> _rentedScooterList;

        public RentalRecordsService(List<RentedScooter> rentedScooterList)
        {
            _rentedScooterList = rentedScooterList;
        }

        public void StartRent(string id, DateTime rentStart)
        {
            _rentedScooterList.Add(new RentedScooter(id, rentStart));
        }

        public RentedScooter EndRent(string id, DateTime rentEnd)
        {
            var rentalRecord = _rentedScooterList
                .First(s => s.Id == id ); //&& !s.EndRent.HasValue);
            rentalRecord.EndRent = rentEnd;

            return rentalRecord;
        }

        public List<RentedScooter> GetRentedScooterList()
        {
            return _rentedScooterList;
        }
    }
}
