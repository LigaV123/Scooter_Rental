using FluentAssertions;
using ScooterRental.Exceptions;

namespace ScooterRental.Test
{
    [TestClass]
    public class ScooterServiceTests
    {
        private IScooterService _scooterService;
        private List<Scooter> _scooterList;
        private const string SCOOTER_ID = "1";
        private const decimal PRICE_PER_MINUTE = 0.2m;

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
        public void AddScooter_WithIdAndPricePerMinute_ScooterIsAdded()
        {
            _scooterService.AddScooter(SCOOTER_ID, PRICE_PER_MINUTE);
            var scooter = _scooterList.First();

            _scooterList.Count.Should().Be(1);
            scooter.Id.Should().Be(SCOOTER_ID);
            scooter.PricePerMinute.Should().Be(PRICE_PER_MINUTE);
        }

        [TestMethod]
        public void AddScooter_DuplicateScooterId_ThrowsDuplicateScooterException()
        {
            _scooterList.Add(new Scooter(SCOOTER_ID, PRICE_PER_MINUTE));
            Action action = () =>_scooterService.AddScooter(SCOOTER_ID, PRICE_PER_MINUTE);

            action.Should().Throw<DuplicateScooterException>();
        }

        [TestMethod]
        public void AddScooter_WithNegativePrice_ThrowsNegativePriceException()
        {
            Action action = () => _scooterService.AddScooter(SCOOTER_ID, -10m);

            action.Should().Throw<NegativePriceException>();
        }

        [TestMethod]
        public void AddScooter_WithZeroPrice_ThrowsNegativePriceException()
        {
            Action action = () => _scooterService.AddScooter(SCOOTER_ID, 0m);

            action.Should().Throw<NegativePriceException>();
        }

        [TestMethod]
        public void AddScooter_WithEmptyId_ThrowsInvalidIDException()
        {
            Action action = () => _scooterService.AddScooter("", PRICE_PER_MINUTE);

            action.Should().Throw<InvalidIDException>();
        }

        [TestMethod]
        public void RemoveScooter_WithSpecificId_ScooterIsRemoved()
        {
            _scooterService.AddScooter(SCOOTER_ID, PRICE_PER_MINUTE);
            _scooterService.RemoveScooter(SCOOTER_ID);

            _scooterList.Count.Should().Be(0);
        }

        [TestMethod]
        public void RemoveScooter_WithNonexistentScooterId_ThrowsNonexistentScooterIdException()
        {
            _scooterService.AddScooter(SCOOTER_ID, PRICE_PER_MINUTE);
            Action action = () =>_scooterService.RemoveScooter("123");

            _scooterList.Count.Should().Be(1);
            action.Should().Throw<NonexistentScooterIdException>();
        }

        [TestMethod]
        public void RemoveScooter_ScooterIdWhoIsRented_ThrowsScooterRentedException()
        {
            _scooterService.AddScooter(SCOOTER_ID, PRICE_PER_MINUTE);
            _rentalCompany.StartRent(SCOOTER_ID);
            Action action = () => _scooterService.RemoveScooter(SCOOTER_ID);

            _scooterList.Count.Should().Be(1);
            _scooterList.First(s => s.Id == SCOOTER_ID).IsRented.Should().BeTrue();
            action.Should().Throw<ScooterRentedException>();
        }

        [TestMethod]
        public void GetScooters_ScooterListWith2AddedScooters_ReturnsScooterListWith2Scooters()
        {
            _scooterService.AddScooter(SCOOTER_ID, PRICE_PER_MINUTE);
            _scooterService.AddScooter("2", 1m);

            _scooterService.GetScooters().Count.Should().Be(2);
        }

        [TestMethod]
        public void GetScooters_2ScooterInListWith1ScooterAvailable_ReturnsScooterListWith1Scooter()
        {
            _scooterService.AddScooter(SCOOTER_ID, PRICE_PER_MINUTE);
            _scooterService.AddScooter("2", 1m);
            _rentalCompany.StartRent(SCOOTER_ID);

            _scooterList.First(s => s.Id == SCOOTER_ID).IsRented.Should().BeTrue();
            _scooterService.GetScooters().Count.Should().Be(1);
        }

        [TestMethod]
        public void GetScooters_ScooterListWith1RentedScooter_ThrowsNoAvailableScooterException()
        {
            _scooterService.AddScooter(SCOOTER_ID, PRICE_PER_MINUTE);
            _rentalCompany.StartRent(SCOOTER_ID);
            Action action = () => _scooterService.GetScooters();

            action.Should().Throw<NoAvailableScooterException>();
        }

        [TestMethod]
        public void GetScooterById_ScooterWithSpecificId_ReturnsScooter()
        {
            _scooterService.AddScooter(SCOOTER_ID, PRICE_PER_MINUTE);

            _scooterService.GetScooterById(SCOOTER_ID).Id.Should().Be("1");
        }

        [TestMethod]
        public void GetScooterById_WithNonexistentScooterId_ThrowsNonexistentScooterIdException()
        {
            _scooterService.AddScooter(SCOOTER_ID, PRICE_PER_MINUTE);
            Action action = () => _scooterService.GetScooterById("1hj23");

            action.Should().Throw<NonexistentScooterIdException>();
        }
    }
}