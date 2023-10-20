using ScooterRental.Exceptions;

namespace ScooterRental
{
    public class ScooterService : IScooterService
    {
        private readonly List<Scooter> _scooters;

        public ScooterService(List<Scooter> scooterList)
        {
            _scooters = scooterList;
        }

        public void AddScooter(string id, decimal pricePerMinute)
        {
            if (_scooters.Any(s => s.Id == id))
            {
                throw new DuplicateScooterException();
            }

            if (pricePerMinute <= 0)
            {
                throw new NegativePriceException();
            }

            if (string.IsNullOrEmpty(id))
            {
                throw new InvalidIDException();
            }

            _scooters.Add(new Scooter(id, pricePerMinute));
        }
        private void CheckIfScooterExists(string id)
        {
            var uniqueScooters = _scooters.Where(s => s.Id != id).ToList();
            if (_scooters.Count == uniqueScooters.Count)
            {
                throw new NonexistentScooterIdException();
            }
        }

        public void RemoveScooter(string id)
        {
            CheckIfScooterExists(id);

            if (_scooters.Any(s => s.Id == id && s.IsRented))
            {
                throw new ScooterRentedException();
            }

            var scooterToRemove = _scooters.SingleOrDefault(s => s.Id == id);
            _scooters.Remove(scooterToRemove);
        }

        public IList<Scooter> GetScooters()
        {
            
            var availableScooters = _scooters.Where(s => !s.IsRented).ToList();
            if (availableScooters.Count == 0)
            {
                throw new NoAvailableScooterException();
            }

            return availableScooters;
        }

        public Scooter GetScooterById(string scooterId)
        {
            CheckIfScooterExists(scooterId);

            return _scooters.Single(s => s.Id == scooterId);
        }
    }
}
