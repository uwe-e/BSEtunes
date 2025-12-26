namespace BSEtunes.Application.Services
{
    public interface ISystemService
    {
        Task<bool> IsDatabaseAccessibleAsync();
    }
}
