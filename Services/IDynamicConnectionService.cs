namespace Bridge_App.Services
{
    public interface IDynamicConnectionService
    {
        Task<string> GetDynamicConnectionAsync(int companyKey, int projectKey);
        void SetConnectionString(string connectionString);
        string? GetCurrentConnectionString();
    }
}
