namespace ScooterRental;

public interface IRentalRecordsService
{
    void StartRent(string id, DateTime rentStart);
    RentedScooter EndRent(string id, DateTime rentEnd);
    List<RentedScooter> GetRentedScooterList();
}