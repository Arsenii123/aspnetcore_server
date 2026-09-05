namespace mvc.Repositories.Interfaces
{
    public interface IEntityRepository<T> where T : class
    {
        Task<List<T>> GetEntityListAsync();
        Task<T> GetEntityAsync(int id);
        Task CreateAsync(T entity);
        void Update(T entity);
        Task DeleteAsync(int id);
        Task SaveChangesAsync();
    }
}