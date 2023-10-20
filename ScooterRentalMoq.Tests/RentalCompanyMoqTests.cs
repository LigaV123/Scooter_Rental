using FluentAssertions;
using Moq;
using Moq.AutoMock;
using ScooterRental;
using ScooterRental.Exceptions;

namespace ScooterRentalMoq.Tests
{
    [TestClass]
    public class RentalCompanyMoqTests
    {
        private AutoMocker _mocker;
        private IRentalCompany _rentalCompany;
        private const string DEFAULT_ID = "1";
        private const decimal DEFAULT_PRICE = 1m;
        private Scooter _scooter;

        [TestInitialize]
        public void Setup()
        {
            _mocker = new AutoMocker();
            _scooter = new Scooter(DEFAULT_ID, DEFAULT_PRICE);
            var scooterServiceMock = _mocker.GetMock<IScooterService>();
            var rentalRecordsServiceMock = _mocker.GetMock<IRentalRecordsService>();
            _rentalCompany = new RentalCompany("Name Of Company", 
                scooterServiceMock.Object, 
                rentalRecordsServiceMock.Object);
            
            _mocker.GetMock<IScooterService>()
                .Setup(s => s.GetScooterById(DEFAULT_ID))
                .Returns(_scooter);
        }

        [TestMethod]
        public void RentalCompany_WhenCreated_HasName()
        {
            _rentalCompany.Name.Should().NotBeNullOrEmpty().And.NotBeNullOrWhiteSpace();
        }

        [TestMethod]
        public void StartRent_ScooterWithId1_StartsRentForScooter1()
        {
            _rentalCompany.StartRent(DEFAULT_ID);

            _scooter.IsRented.Should().BeTrue();
            _mocker.GetMock<IRentalRecordsService>().
                Verify(s => 
                    s.StartRent(DEFAULT_ID, It.IsAny<DateTime>()), Times.Once);
        }

        [TestMethod]
        public void StartRent_2TimesFor1Scooter_ThrowsScooterRentedException()
        {
            _rentalCompany.StartRent(DEFAULT_ID);
            Action action = () => _rentalCompany.StartRent(DEFAULT_ID);

            action.Should().Throw<ScooterRentedException>();
        }

        [TestMethod]
        public void StartRent_EmptyAvailableScooterList_ThrowsNoAvailableScooterException()
        {
            _scooter.IsRented = true;
            
            Action action = () => _rentalCompany.StartRent(DEFAULT_ID);

            action.Should().Throw<ScooterRentedException>();
        }

        [TestMethod]
        public void EndRent_ScooterWithId1RentPriceIsLessThan20_EndsRentForScooter1AndReturnsPrice()
        {
            var rentedScooter = new RentedScooter(DEFAULT_ID, DateTime.Now.AddMinutes(-10)) { EndRent = DateTime.Now };
            MakeRentedScooterMock(rentedScooter);

            var result = _rentalCompany.EndRent(DEFAULT_ID);

            result.Should().Be(10);
            _scooter.IsRented.Should().BeFalse();
        }

        [TestMethod]
        public void EndRent_ReachesMaxRentalPricePerDay_ReturnsMaxPrice()
        {
            var rentedScooter = new RentedScooter(DEFAULT_ID, DateTime.Now.AddMinutes(-30)) { EndRent = DateTime.Now };
            MakeRentedScooterMock(rentedScooter);

            var result = _rentalCompany.EndRent(DEFAULT_ID);

            result.Should().Be(20);
            _scooter.IsRented.Should().BeFalse();
        }

        [TestMethod]
        public void EndRent_RentFor2Days_ReturnsPriceFor2DayRental()
        {
            var rentedScooter = new RentedScooter(DEFAULT_ID, DateTime.Now.AddDays(-2)) { EndRent = DateTime.Now };
            MakeRentedScooterMock(rentedScooter);

            var result = _rentalCompany.EndRent(DEFAULT_ID);

            result.Should().Be(40);
            _scooter.IsRented.Should().BeFalse();
        }

        [TestMethod]
        public void EndRent_RentFor2Days10Minutes_ReturnsPriceFor2DayAnd10MinuteRental()
        {
            var rentedScooter = new RentedScooter(DEFAULT_ID, DateTime.Now.AddDays(-2).AddMinutes(-10)) { EndRent = DateTime.Now };
            MakeRentedScooterMock(rentedScooter);

            var result = _rentalCompany.EndRent(DEFAULT_ID);

            result.Should().Be(50);
            _scooter.IsRented.Should().BeFalse();
        }

        [TestMethod]
        public void EndRent_RentFor2Days1HourReachesMaxRentalPrice_ReturnsPriceFor3RentalDays()
        {
            var rentedScooter = new RentedScooter(DEFAULT_ID, DateTime.Now.AddDays(-2).AddHours(-1)) { EndRent = DateTime.Now };
            MakeRentedScooterMock(rentedScooter);

            var result = _rentalCompany.EndRent(DEFAULT_ID);

            result.Should().Be(60);
            _scooter.IsRented.Should().BeFalse();
        }

        [TestMethod]
        public void EndRent_RentFor24HoursAnd5MinutesInMinutes_ReturnsPriceOf25()
        {
            var rentedScooter = new RentedScooter(DEFAULT_ID, DateTime.Now.AddMinutes(-1445)) { EndRent = DateTime.Now };
            MakeRentedScooterMock(rentedScooter);

            var result = _rentalCompany.EndRent(DEFAULT_ID);

            result.Should().Be(25);
            _scooter.IsRented.Should().BeFalse();
        }

        [TestMethod]
        public void CalculateIncome_SpecificYearWithAllRentalsEnded_TotalIncomeOfThatYearWithAllRentalsEnded()
        {
            var now = DateTime.Now;
            var rentedScooters = new List<RentedScooter>
            {
                new(DEFAULT_ID, now.AddMinutes(-10)){EndRent = now, TotalRentPrice = 10},
                new("2", now.AddMinutes(-5)){EndRent = now, TotalRentPrice = 0.5m}
            };

            _mocker.GetMock<IRentalRecordsService>()
                .Setup(s => s.GetRentedScooterList())
                .Returns(rentedScooters);
            
            var result = _rentalCompany.CalculateIncome(now.Year, false);

            result.Should().Be(10.5m);
        }

        [TestMethod]
        public void CalculateIncome_SpecificYearWith1IncompleteRental_TotalIncomeOfThatYearWith1IncompleteRental()
        {
            var now = DateTime.Now;
            var secondScooter = new Scooter("2", 0.1m);
            var rentedScooters = new List<RentedScooter>
            {
                new(DEFAULT_ID, now.AddMinutes(-10)) {EndRent = now, TotalRentPrice = 10m },
                new("2", now.AddMinutes(-5)) { EndRent = null }
            };

            _mocker.GetMock<IRentalRecordsService>()
                .Setup(s => s.GetRentedScooterList())
                .Returns(rentedScooters);
            _mocker.GetMock<IScooterService>()
                .Setup(s => s.GetScooterById("2"))
                .Returns(secondScooter);

            var result = _rentalCompany.CalculateIncome(now.Year, true);

            result.Should().Be(10.5m);
        }

        [TestMethod]
        public void CalculateIncome_NoYearWithCompleteRentals_TotalIncomeWithCompleteRentals()
        {
            var now = DateTime.Now;
            var rentedScooters = new List<RentedScooter>
            {
                new(DEFAULT_ID, now.AddMinutes(-10)) {EndRent = now, TotalRentPrice = 10m },
                new("2", now.AddMinutes(-5)) { EndRent = now, TotalRentPrice = 0.5m}
            };

            _mocker.GetMock<IRentalRecordsService>()
                .Setup(s => s.GetRentedScooterList())
                .Returns(rentedScooters);

            var result = _rentalCompany.CalculateIncome(null, false);

            result.Should().Be(10.5m);
        }

        [TestMethod]
        public void CalculateIncome_NoYearWith1IncompleteRental_TotalIncomeWithIncompleteRentals()
        {
            var now = DateTime.Now;
            var secondScooter = new Scooter("2", 0.1m);
            var rentedScooters = new List<RentedScooter>
            {
                new(DEFAULT_ID, now.AddMinutes(-10)) {EndRent = now, TotalRentPrice = 10m },
                new("2", now.AddMinutes(-5)) { EndRent = null }
            };

            _mocker.GetMock<IRentalRecordsService>()
                .Setup(s => s.GetRentedScooterList())
                .Returns(rentedScooters);
            _mocker.GetMock<IScooterService>()
                .Setup(s => s.GetScooterById("2"))
                .Returns(secondScooter);

            var result = _rentalCompany.CalculateIncome(null, true);

            result.Should().Be(10.5m);
        }

        private void MakeRentedScooterMock(RentedScooter rentedScooter)
        {
            _mocker.GetMock<IRentalRecordsService>()
                .Setup(s => s.EndRent(DEFAULT_ID, It.IsAny<DateTime>()))
                .Returns(rentedScooter);
        }
    }
}