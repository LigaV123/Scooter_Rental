using ScooterRental.Exceptions;

namespace ScooterRental
{
    public class RentalCompany : IRentalCompany
    {
        private readonly IScooterService _scooterService;
        private readonly IRentalRecordsService _rentalRecordsService;

        public RentalCompany(string name, IScooterService scooterService, IRentalRecordsService rentalRecordsService)
        {
            Name = name;
            _scooterService = scooterService;
            _rentalRecordsService = rentalRecordsService;
        }

        public string Name { get; }

        public void StartRent(string id)
        {
            var scooter = _scooterService.GetScooterById(id);
            if (scooter.IsRented)
            {
                throw new ScooterRentedException();
            }

            scooter.IsRented = true;
            _rentalRecordsService.StartRent(scooter.Id, DateTime.Now);
        }

        public decimal EndRent(string id)
        {
            var scooter = _scooterService.GetScooterById(id);
            scooter.IsRented = false;
            var rentalRecord = _rentalRecordsService.EndRent(scooter.Id, DateTime.Now);

            rentalRecord.TotalRentPrice = CalculateTotalRentPrice(rentalRecord, scooter);

            return rentalRecord.TotalRentPrice;
        }

        private decimal CalculateTotalRentPrice(RentedScooter rentalRecord, Scooter scooter)
        {
            var endRent = rentalRecord.EndRent ?? DateTime.Now;
            var rentalDays = (endRent - rentalRecord.StartRent).Days;
            var rentalHours = (endRent - rentalRecord.StartRent).Hours;
            var rentalMinutes = (endRent - rentalRecord.StartRent).Minutes;
            var totalPriceInMinutes = (rentalHours * 60 + rentalMinutes) * scooter.PricePerMinute;

            if (totalPriceInMinutes > 20)
            {
                totalPriceInMinutes = 20;
            }

            var totalRentPrice = totalPriceInMinutes + rentalDays * 20;
            return totalRentPrice;
        }

        public decimal CalculateIncome(int? year, bool includeNotCompletedRentals)
        {
            decimal totalIncome = 0;

            if (year.HasValue)
            {
                totalIncome += _rentalRecordsService
                    .GetRentedScooterList()
                    .Where(s => s.EndRent.HasValue && s.EndRent.Value.Year == year)
                    .Select(s => s.TotalRentPrice)
                    .Sum();
            }

            if (year == null)
            {
                totalIncome += _rentalRecordsService
                    .GetRentedScooterList()
                    .Where(s => s.EndRent.HasValue)
                    .Select(s => s.TotalRentPrice)
                    .Sum();
            }

            if (includeNotCompletedRentals)
            {
                totalIncome += _rentalRecordsService
                    .GetRentedScooterList()
                    .Where(s => !s.EndRent.HasValue)
                    .Select(s => CalculateTotalRentPrice(s, _scooterService.GetScooterById(s.Id)))
                    .Sum();
            }

            return totalIncome;
        }
    }
}
