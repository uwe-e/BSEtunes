using BSEtunes.Infrastructure.Repositories;

namespace BSEtunes.Application.Services
{
    public class SystemService : ISystemService
    {
        private readonly IDatabaseHealthRepository _repository;

        public SystemService(IDatabaseHealthRepository repository)
        {
            _repository = repository;
        }

        public async Task<bool> IsDatabaseAccessibleAsync()
        {
            return await _repository.IsDatabaseAccessibleAsync();
        }
    }
}
