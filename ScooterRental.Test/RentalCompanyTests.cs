using FluentAssertions;
using FluentAssertions.Extensions;
using ScooterRental.Exceptions;

namespace ScooterRental.Test
{
    [TestClass]
    public class RentalCompanyTests
    {
        private IScooterService _scooterService;
        private List<Scooter> _scooterList;
        private const string SCOOTER_ID = "1";
        private const decimal PRICE_PER_MINUTE = 1m;

        private IRentalCompany _rentalCompany;
        private IRentalRecordsService _rentedScooters;
        private List<RentedScooter> _rentedScooterList;

        [TestInitialize]
        public void Setup()
        {
            _scooterList = new List<Scooter>();
            _rentedScooterList = new List<RentedScooter>();

            _scooterService = new ScooterService(_scooterList);
            _rentedScooters = new RentalRecordsService(_rentedScooterList);
            _rentalCompany = new RentalCompany("RenT-Scoot-ER", _scooterService, _rentedScooters);
        }

        [TestMethod]
        public void StartRent_ScooterWithId1_StartsRentForScooter1()
        {
            _scooterService.AddScooter(SCOOTER_ID, PRICE_PER_MINUTE);
            _rentalCompany.StartRent(SCOOTER_ID);

            _scooterList.First().IsRented.Should().BeTrue();
            _rentedScooters.GetRentedScooterList().Count.Should().Be(1);
            _rentedScooters.GetRentedScooterList().First().Id.Should().Be("1");
            _rentedScooters.GetRentedScooterList().First().StartRent.Should().BeCloseTo(DateTime.Now, 1.Seconds());
        }

        [TestMethod]
        public void StartRent_2TimesFor1Scooter_ThrowsScooterRentedException()
        {
            _scooterService.AddScooter(SCOOTER_ID, PRICE_PER_MINUTE);
            _scooterService.AddScooter("fg32", PRICE_PER_MINUTE);
            _rentalCompany.StartRent(SCOOTER_ID);
            Action action = () => _rentalCompany.StartRent(SCOOTER_ID);

            _scooterList.First().IsRented.Should().BeTrue();
            _rentedScooters.GetRentedScooterList().Count.Should().Be(1);
            action.Should().Throw<ScooterRentedException>();
        }

        [TestMethod]
        public void StartRent_EmptyAvailableScooterList_ThrowsNoAvailableScooterException()
        {
            _scooterService.AddScooter(SCOOTER_ID, PRICE_PER_MINUTE);
            _rentalCompany.StartRent(SCOOTER_ID);
            Action action = () => _scooterService.GetScooters();

            _rentedScooters.GetRentedScooterList().Count.Should().Be(1);
            action.Should().Throw<NoAvailableScooterException>();
        }

        [TestMethod]
        public void EndRent_ScooterWithId1RentPriceIsLessThan20_EndsRentForScooter1AndReturnsPrice()
        {
            _scooterList.Add(new Scooter(SCOOTER_ID, PRICE_PER_MINUTE));
            _rentedScooters.GetRentedScooterList().Add(new RentedScooter(SCOOTER_ID, DateTime.Now.AddMinutes(-10)));
            var price = _rentalCompany.EndRent(SCOOTER_ID);

            price.Should().Be(10);
            _scooterList.First().IsRented.Should().BeFalse();
        }

        [TestMethod]
        public void EndRent_ReachesMaxRentalPricePerDay_ReturnsMaxPrice()
        {
            _scooterList.Add(new Scooter(SCOOTER_ID, PRICE_PER_MINUTE));
            _rentedScooters.GetRentedScooterList().Add(new RentedScooter(SCOOTER_ID, DateTime.Now.AddMinutes(-30)));
            var price = _rentalCompany.EndRent(SCOOTER_ID);

            price.Should().Be(20);
            _scooterList.First().IsRented.Should().BeFalse();
        }

        [TestMethod]
        public void EndRent_RentFor2Days_ReturnsPriceFor2DayRental()
        {
            _scooterList.Add(new Scooter(SCOOTER_ID, PRICE_PER_MINUTE));
            _rentedScooters.GetRentedScooterList().Add(new RentedScooter(SCOOTER_ID, DateTime.Now.AddDays(-2)));
            var price = _rentalCompany.EndRent(SCOOTER_ID);

            price.Should().Be(40);
            _scooterList.First().IsRented.Should().BeFalse();
        }

        [TestMethod]
        public void EndRent_RentFor2Days10Minutes_ReturnsPriceFor2DayAnd10MinuteRental()
        {
            _scooterList.Add(new Scooter(SCOOTER_ID, PRICE_PER_MINUTE));
            _rentedScooters.GetRentedScooterList().Add(new RentedScooter(SCOOTER_ID, DateTime.Now.AddDays(-2).AddMinutes(-10)));
            var price = _rentalCompany.EndRent(SCOOTER_ID);

            price.Should().Be(50);
            _scooterList.First().IsRented.Should().BeFalse();
        }

        [TestMethod]
        public void EndRent_RentFor2Days1HourReachesMaxRentalPrice_ReturnsPriceFor3RentalDays()
        {
            _scooterList.Add(new Scooter(SCOOTER_ID, PRICE_PER_MINUTE));
            _rentedScooters.GetRentedScooterList().Add(new RentedScooter(SCOOTER_ID, DateTime.Now.AddDays(-2).AddHours(-1)));
            var price = _rentalCompany.EndRent(SCOOTER_ID);

            price.Should().Be(60);
            _scooterList.First().IsRented.Should().BeFalse();
        }

        [TestMethod]
        public void EndRent_RentFor24HoursAnd5MinutesInMinutes_ReturnsPriceOf25()
        {
            _scooterList.Add(new Scooter(SCOOTER_ID, PRICE_PER_MINUTE));
            _rentedScooters.GetRentedScooterList().Add(new RentedScooter(SCOOTER_ID, DateTime.Now.AddMinutes(-1445)));
            var price = _rentalCompany.EndRent(SCOOTER_ID);

            price.Should().Be(25);
            _scooterList.First().IsRented.Should().BeFalse();
        }

        [TestMethod]
        public void CalculateIncome_SpecificYearWithAllRentalsEnded_TotalIncomeOfThatYearWithAllRentalsEnded()
        {
            _scooterList.Add(new Scooter(SCOOTER_ID, PRICE_PER_MINUTE));
            _scooterList.Add(new Scooter("2", 0.1m));

            _rentedScooters.GetRentedScooterList().Add(new RentedScooter(SCOOTER_ID, DateTime.Now.AddMinutes(-10)));
            _rentalCompany.EndRent(SCOOTER_ID);

            _rentedScooters.GetRentedScooterList().Add(new RentedScooter("2", DateTime.Now.AddMinutes(-5)));
            _rentalCompany.EndRent("2");

            var totalIncome = _rentalCompany.CalculateIncome(DateTime.Now.Year, false);

            totalIncome.Should().Be(10.5m);
        }

        [TestMethod]
        public void CalculateIncome_SpecificYearWith1IncompleteRental_TotalIncomeOfThatYearWith1IncompleteRental()
        {
            _scooterList.Add(new Scooter(SCOOTER_ID, PRICE_PER_MINUTE));
            _scooterList.Add(new Scooter("2", 0.1m));

            _rentedScooters.GetRentedScooterList().Add(new RentedScooter(SCOOTER_ID, DateTime.Now.AddMinutes(-10)));
            _rentalCompany.EndRent(SCOOTER_ID);

            _rentedScooters.GetRentedScooterList().Add(new RentedScooter("2", DateTime.Now.AddMinutes(-5)));

            var totalIncome = _rentalCompany.CalculateIncome(DateTime.Now.Year, true);

            totalIncome.Should().Be(10.5m);
        }

        [TestMethod]
        public void CalculateIncome_NoYearWithCompleteRentals_TotalIncomeWithCompleteRentals()
        {
            _scooterList.Add(new Scooter(SCOOTER_ID, PRICE_PER_MINUTE));
            _scooterList.Add(new Scooter("2", 0.1m));

            _rentedScooters.GetRentedScooterList().Add(new RentedScooter(SCOOTER_ID, DateTime.Now.AddMinutes(-10)));
            _rentalCompany.EndRent(SCOOTER_ID);

            _rentedScooters.GetRentedScooterList().Add(new RentedScooter("2", DateTime.Now.AddMinutes(-5)));
            _rentalCompany.EndRent("2");

            var totalIncome = _rentalCompany.CalculateIncome(null, false);

            totalIncome.Should().Be(10.5m);
        }

        [TestMethod]
        public void CalculateIncome_NoYearWith1IncompleteRental_TotalIncomeWithIncompleteRentals()
        {
            _scooterList.Add(new Scooter(SCOOTER_ID, PRICE_PER_MINUTE));
            _scooterList.Add(new Scooter("2", 0.1m));

            _rentedScooters.GetRentedScooterList().Add(new RentedScooter(SCOOTER_ID, DateTime.Now.AddMinutes(-10)));
            _rentalCompany.EndRent(SCOOTER_ID);

            _rentedScooters.GetRentedScooterList().Add(new RentedScooter("2", DateTime.Now.AddMinutes(-5)));

            var totalIncome = _rentalCompany.CalculateIncome(null, true);

            totalIncome.Should().Be(10.5m);
        }
    }
}
